using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class HardwareVaultTaskService : IHardwareVaultTaskService, IDisposable
    {
        private readonly IAsyncRepository<HardwareVaultTask> _hardwareVaultTaskRepository;

        public HardwareVaultTaskService(IAsyncRepository<HardwareVaultTask> hardwareVaultTaskRepository)
        {
            _hardwareVaultTaskRepository = hardwareVaultTaskRepository;
        }

        public IQueryable<HardwareVaultTask> TaskQuery()
        {
            return _hardwareVaultTaskRepository.Query();
        }

        public async Task<HardwareVaultTask> GetTaskByIdAsync(string taskId)
        {
            return await _hardwareVaultTaskRepository
               .Query()
               .Include(x => x.HardwareVault)
               .Include(x => x.Account.Employee.HardwareVaults)
               .FirstOrDefaultAsync(x => x.Id == taskId);
        }

        public async Task<List<HardwareVaultTask>> GetHardwareVaultTasksAsync()
        {
            return await _hardwareVaultTaskRepository
            .Query()
            .Include(x => x.HardwareVault.Employee)
            .Include(x => x.Account.Employee)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
        }

        public async Task AddTaskAsync(HardwareVaultTask vaultTask)
        {
            await _hardwareVaultTaskRepository.AddAsync(vaultTask);
        }

        public async Task AddRangeTasksAsync(IList<HardwareVaultTask> vaultTasks)
        {
            await _hardwareVaultTaskRepository.AddRangeAsync(vaultTasks);
        }

        public async Task AddPrimaryAsync(string vaultId, string accountId)
        {
            var previousTask = await _hardwareVaultTaskRepository
                .Query()
                .FirstOrDefaultAsync(x => x.HardwareVaultId == vaultId && x.Operation == TaskOperation.Primary);

            if (previousTask != null)
                await _hardwareVaultTaskRepository.DeleteAsync(previousTask);

            var task = new HardwareVaultTask()
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Primary,
                Timestamp = UnixTime.GetUnixTimeUtcNow(),
                HardwareVaultId = vaultId,
                AccountId = accountId
            };

            await _hardwareVaultTaskRepository.AddAsync(task);
        }

        public async Task AddProfileAsync(HardwareVault vault)
        {
            var previousProfileTask = await _hardwareVaultTaskRepository
                .Query()
                .FirstOrDefaultAsync(x => x.HardwareVaultId == vault.Id && x.Operation == TaskOperation.Profile);

            var newProfileTask = new HardwareVaultTask
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Profile,
                Timestamp = UnixTime.GetUnixTimeUtcNow(),
                HardwareVaultId = vault.Id,
                Password = vault.MasterPassword,
            };

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (previousProfileTask != null)
                {
                    await _hardwareVaultTaskRepository.DeleteAsync(previousProfileTask);
                }

                await _hardwareVaultTaskRepository.AddAsync(newProfileTask);

                transactionScope.Complete();
            }
        }

        public async Task DeleteTaskAsync(HardwareVaultTask vaultTask)
        {
            await _hardwareVaultTaskRepository.DeleteAsync(vaultTask);
        }

        public async Task DeleteTasksByVaultIdAsync(string vaultId)
        {
            var allTasks = await _hardwareVaultTaskRepository
                .Query()
                .Where(t => t.HardwareVaultId == vaultId)
                .ToListAsync();

            await _hardwareVaultTaskRepository.DeleteRangeAsync(allTasks);
        }

        public HardwareVaultTask GetAccountCreateTask(string vaultId, string accountId, string password, string otp)
        {
            return new HardwareVaultTask
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Create,
                Timestamp = UnixTime.GetUnixTimeUtcNow(),
                HardwareVaultId = vaultId,
                AccountId = accountId,
                Password = password,
                OtpSecret = otp
            };
        }

        public HardwareVaultTask GetAccountUpdateTask(string vaultId, string accountId)
        {
            return new HardwareVaultTask
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Update,
                Timestamp = UnixTime.GetUnixTimeUtcNow(),
                HardwareVaultId = vaultId,
                AccountId = accountId              
            };
        }  
        
        public HardwareVaultTask GetAccountPwdUpdateTask(string vaultId, string accountId , string password)
        {
            return new HardwareVaultTask
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Update,
                Timestamp = UnixTime.GetUnixTimeUtcNow(),
                HardwareVaultId = vaultId,
                AccountId = accountId,
                Password = password
            };
        }  
        
        public HardwareVaultTask GetAccountOtpUpdateTask(string vaultId, string accountId , string otp)
        {
            return new HardwareVaultTask
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Update,
                Timestamp = UnixTime.GetUnixTimeUtcNow(),
                HardwareVaultId = vaultId,
                AccountId = accountId,
                OtpSecret = otp
            };      
        }

        public HardwareVaultTask GetAccountDeleteTask(string vaultId, string accountId)
        {
            return new HardwareVaultTask
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Delete,
                Timestamp = UnixTime.ConvertToUnixTime(DateTime.UtcNow),
                HardwareVaultId = vaultId,
                AccountId = accountId
            };
        }

        public void Dispose()
        {
            _hardwareVaultTaskRepository.Dispose();
        }
    }
}