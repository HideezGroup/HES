using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using HES.Core.Models.Web.AppSettings;
using HES.Web.Components;
using Hideez.SDK.Communication.Security;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Web.Pages.Employees
{
    public partial class GenerateAdPassword : HESComponentBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IAccountService AccountService { get; set; }
        public ILdapService LdapService { get; set; }
        public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<GenerateAdPassword> Logger { get; set; }
        [Parameter] public string AccountId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }

        public Account Account { get; set; }
        public LdapSettings LdapSettings { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                AccountService = ScopedServices.GetRequiredService<IAccountService>();
                LdapService = ScopedServices.GetRequiredService<ILdapService>();
                RemoteDeviceConnectionsService = ScopedServices.GetRequiredService<IRemoteDeviceConnectionsService>();

                Account = await AccountService.GetAccountByIdAsync(AccountId);
                if (Account == null)
                    throw new Exception("Account not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(Account.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Account.Id, Account);

                LdapSettings = await AppSettingsService.GetLdapSettingsAsync();

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task GenerateAccountPasswordAsync()
        {
            try
            {
                if (LdapSettings?.Password == null)
                    throw new Exception("Active Directory credentials not set in parameters page.");

                var accountPassword = new AccountPassword() { Password = PasswordGenerator.Generate() };

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await EmployeeService.EditPersonalAccountPwdAsync(Account, accountPassword);
                    await LdapService.SetUserPasswordAsync(Account.EmployeeId, accountPassword.Password, LdapSettings);
                    transactionScope.Complete();
                }

                RemoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await EmployeeService.GetEmployeeVaultIdsAsync(Account.EmployeeId));
                await ToastService.ShowToastAsync("Account password updated.", ToastType.Success);
                await SynchronizationService.UpdateEmployeeDetails(ExceptPageId, Account.EmployeeId);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Account.Id);
        }
    }
}