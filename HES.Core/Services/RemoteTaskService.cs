using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class RemoteTaskService : IRemoteTaskService
    {
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly IEmployeeService _employeeService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly ILdapService _ldapService;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IPageSyncService _synchronizationService;

        public RemoteTaskService(IHardwareVaultService hardwareVaultService,
                                 IHardwareVaultTaskService hardwareVaultTaskService,
                                 IEmployeeService employeeService,
                                 IDataProtectionService dataProtectionService,
                                 ILdapService ldapService,
                                 IAppSettingsService appSettingsService,
                                 IPageSyncService synchronizationService)
        {
            _hardwareVaultService = hardwareVaultService;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _employeeService = employeeService;
            _dataProtectionService = dataProtectionService;
            _ldapService = ldapService;
            _appSettingsService = appSettingsService;
            _synchronizationService = synchronizationService;
        }

        public async Task ExecuteRemoteTasks(string vaultId, Device remoteDevice, bool primaryAccountOnly)
        {
            _dataProtectionService.Validate();

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);
            if (vault == null)
            {
                throw new HESException(HESCode.HardwareVaultNotFound);
            }
    
            var tasksQuery = _hardwareVaultTaskService
                .TaskQuery()
                .Include(t => t.HardwareVault)
                .Include(t => t.Account)
                .Where(t => t.HardwareVaultId == vaultId);

            if (primaryAccountOnly)
            {
                tasksQuery = tasksQuery.Where(x => x.AccountId == vault.Employee.PrimaryAccountId || x.Operation == TaskOperation.Primary);
            }

            tasksQuery = tasksQuery.OrderBy(x => x.CreatedAt).AsNoTracking();
            var tasks = await tasksQuery.ToListAsync();

            while (tasks.Any())
            {
                foreach (var task in tasks)
                {
                    task.Password = _dataProtectionService.Decrypt(task.Password);
                    task.OtpSecret = _dataProtectionService.Decrypt(task.OtpSecret);
                    await ExecuteRemoteTask(remoteDevice, task);
                    await TaskCompleted(task.Id);
                }

                tasks = await tasksQuery.ToListAsync();
            }

            await _hardwareVaultService.UpdateNeedSyncAsync(vault, false);
            await _synchronizationService.HardwareVaultStateChanged(vault.Id);
        }

        private async Task ExecuteRemoteTask(Device remoteDevice, HardwareVaultTask task)
        {
            switch (task.Operation)
            {
                case TaskOperation.Create:
                    if (task.Account.UpdateInActiveDirectory)
                    {
                        var ldapSettings = await _appSettingsService.GetLdapSettingsAsync();
                        if (ldapSettings == null)
                        {
                            throw new Exception("To update the account password from the active directory, the active directory settings are required.");
                        }
                        await _ldapService.SetUserPasswordAsync(task.HardwareVault.EmployeeId, task.Password, ldapSettings);
                    }
                    await AddAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Update:
                    await UpdateAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Delete:
                    await DeleteAccountAsync(remoteDevice, task);
                    break;
                case TaskOperation.Primary:
                    await SetAccountAsPrimaryAsync(remoteDevice, task);
                    break;
                case TaskOperation.Profile:
                    await ProfileVaultAsync(remoteDevice, task);
                    break;
            }
        }

        private async Task TaskCompleted(string taskId)
        {
            var task = await _hardwareVaultTaskService.GetTaskByIdAsync(taskId);
            if (task == null)
            {
                throw new Exception($"Task {taskId} not found");
            }

            switch (task.Operation)
            {
                case TaskOperation.Create:
                    await _employeeService.UpdateAfterAccountCreateAsync(task.Account, task.Timestamp);
                    break;
                case TaskOperation.Update:
                case TaskOperation.Delete:
                case TaskOperation.Primary:
                    await _employeeService.UpdateAfterAccountModifyAsync(task.Account, task.Timestamp);
                    break;
            }

            await _hardwareVaultTaskService.DeleteTaskAsync(task);
        }

        private async Task AddAccountAsync(Device remoteDevice, HardwareVaultTask task)
        {
            var account = await _employeeService.GetAccountByIdAsync(task.AccountId, true);
            bool isPrimary = account.Employee.PrimaryAccountId == task.AccountId;
            var pm = new DevicePasswordManager(remoteDevice, null);
            await pm.SaveOrUpdateAccount(new StorageId(account.StorageId), task.Timestamp, account.Name, task.Password, account.Login, task.OtpSecret, account.Apps, account.Urls, isPrimary, new AccountFlagsOptions() { IsReadOnly = true });
        }

        private async Task UpdateAccountAsync(Device remoteDevice, HardwareVaultTask task)
        {
            var account = await _employeeService.GetAccountByIdAsync(task.AccountId, true);
            bool isPrimary = account.Employee.PrimaryAccountId == task.AccountId;
            var storageId = new StorageId(account.StorageId);
            var pm = new DevicePasswordManager(remoteDevice, null);
            await pm.SaveOrUpdateAccount(storageId, task.Timestamp, account.Name, task.Password, account.Login, task.OtpSecret, account.Apps, account.Urls, isPrimary, new AccountFlagsOptions() { IsReadOnly = true });
        }

        private async Task SetAccountAsPrimaryAsync(Device remoteDevice, HardwareVaultTask task)
        {
            var account = await _employeeService.GetAccountByIdAsync(task.AccountId, true);
            var storageId = new StorageId(account.StorageId);
            var pm = new DevicePasswordManager(remoteDevice, null);
            await pm.SaveOrUpdateAccount(storageId, task.Timestamp, null, null, null, null, null, null, true, new AccountFlagsOptions() { IsReadOnly = true });
        }

        private async Task DeleteAccountAsync(Device remoteDevice, HardwareVaultTask task)
        {
            var account = await _employeeService.GetAccountByIdAsync(task.AccountId, true);
            bool isPrimary = account.Employee.PrimaryAccountId == task.AccountId;
            var storageId = new StorageId(account.StorageId);
            var pm = new DevicePasswordManager(remoteDevice, null);
            await pm.DeleteAccount(storageId, isPrimary);
        }

        private async Task ProfileVaultAsync(Device remoteDevice, HardwareVaultTask task)
        {
            var accessParams = await _hardwareVaultService.GetAccessParamsAsync(task.HardwareVaultId);
            var key = ConvertUtils.HexStringToBytes(task.Password);
            await remoteDevice.Access(DateTime.UtcNow, key, accessParams);
        }

        public async Task AccessVaultAsync(Device remoteDevice, HardwareVault vault)
        {
            var accessParams = await _hardwareVaultService.GetAccessParamsAsync(vault.Id);
            var key = ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(vault.MasterPassword));
            await remoteDevice.Access(DateTime.UtcNow, key, accessParams);
        }

        public async Task LinkVaultAsync(Device remoteDevice, HardwareVault vault)
        {
            if (!remoteDevice.AccessLevel.IsLinkRequired)
            {
                return;
            }

            var code = Encoding.UTF8.GetBytes(await _hardwareVaultService.GetVaultActivationCodeAsync(vault.Id));
            var key = ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(vault.MasterPassword));
            await remoteDevice.Link(key, code, 3);
            await _hardwareVaultService.SetVaultStatusAppliedAsync(vault);
        }

        public async Task SuspendVaultAsync(Device remoteDevice, HardwareVault vault)
        {
            if (vault.IsStatusApplied)
            {
                return;
            }

            var code = Encoding.UTF8.GetBytes(await _hardwareVaultService.GetVaultActivationCodeAsync(vault.Id));
            var key = ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(vault.MasterPassword));
            await remoteDevice.LockDeviceCode(key, code, 3);
            await _hardwareVaultService.SetVaultStatusAppliedAsync(vault);
        }

        public async Task WipeVaultAsync(Device remoteDevice, HardwareVault vault)
        {
            if (!remoteDevice.AccessLevel.IsLinkRequired)
            {
                await remoteDevice.Wipe(ConvertUtils.HexStringToBytes(_dataProtectionService.Decrypt(vault.MasterPassword)));
            }
        }
    }
}