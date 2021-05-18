using HES.Core.Entities;
using HES.Core.Helpers;
using HES.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class HardwareVaultTaskService : IHardwareVaultTaskService
    {
        private readonly IApplicationDbContext _dbContext;

        public HardwareVaultTaskService(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IQueryable<HardwareVaultTask> TaskQuery()
        {
            return _dbContext.HardwareVaultTasks.AsQueryable();
        }

        public async Task<HardwareVaultTask> GetTaskByIdAsync(string taskId)
        {
            return await _dbContext.HardwareVaultTasks
               .Include(x => x.HardwareVault)
               .Include(x => x.Account.Employee.HardwareVaults)
               .FirstOrDefaultAsync(x => x.Id == taskId);
        }

        public async Task<List<HardwareVaultTask>> GetHardwareVaultTasksNoTrackingAsync()
        {
            return await _dbContext.HardwareVaultTasks
            .Include(x => x.HardwareVault.Employee)
            .Include(x => x.Account.Employee)
            .OrderByDescending(x => x.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
        }

        public async Task AddRangeTasksAsync(IList<HardwareVaultTask> vaultTasks)
        {
            _dbContext.HardwareVaultTasks.AddRange(vaultTasks);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddPrimaryAsync(string vaultId, string accountId)
        {
            var previousTask = await _dbContext.HardwareVaultTasks
                .FirstOrDefaultAsync(x => x.HardwareVaultId == vaultId && x.Operation == TaskOperation.Primary);

            if (previousTask != null)
            {
                _dbContext.HardwareVaultTasks.Remove(previousTask);
            }

            var task = new HardwareVaultTask()
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Primary,
                Timestamp = UnixTimeHelper.GetUnixTimeUtcNow(),
                HardwareVaultId = vaultId,
                AccountId = accountId
            };

            _dbContext.HardwareVaultTasks.Add(task);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddProfileAsync(HardwareVault vault)
        {
            var previousProfileTask = await _dbContext.HardwareVaultTasks
                .FirstOrDefaultAsync(x => x.HardwareVaultId == vault.Id && x.Operation == TaskOperation.Profile);

            if (previousProfileTask != null)
            {
                _dbContext.HardwareVaultTasks.Remove(previousProfileTask);
            }

            var newProfileTask = new HardwareVaultTask
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Profile,
                Timestamp = UnixTimeHelper.GetUnixTimeUtcNow(),
                HardwareVaultId = vault.Id,
                Password = vault.MasterPassword,
            };

            _dbContext.HardwareVaultTasks.Add(newProfileTask);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(HardwareVaultTask vaultTask)
        {
            _dbContext.HardwareVaultTasks.Remove(vaultTask);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteTasksByVaultIdAsync(string vaultId)
        {
            var tasks = _dbContext.HardwareVaultTasks.Where(t => t.HardwareVaultId == vaultId).AsQueryable();
            _dbContext.HardwareVaultTasks.RemoveRange(tasks);
            await _dbContext.SaveChangesAsync();
        }

        public HardwareVaultTask GetAccountCreateTask(string vaultId, string accountId, string password, string otp)
        {
            return new HardwareVaultTask
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Create,
                Timestamp = UnixTimeHelper.GetUnixTimeUtcNow(),
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
                Timestamp = UnixTimeHelper.GetUnixTimeUtcNow(),
                HardwareVaultId = vaultId,
                AccountId = accountId
            };
        }

        public HardwareVaultTask GetAccountPwdUpdateTask(string vaultId, string accountId, string password)
        {
            return new HardwareVaultTask
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Update,
                Timestamp = UnixTimeHelper.GetUnixTimeUtcNow(),
                HardwareVaultId = vaultId,
                AccountId = accountId,
                Password = password
            };
        }

        public HardwareVaultTask GetAccountOtpUpdateTask(string vaultId, string accountId, string otp)
        {
            return new HardwareVaultTask
            {
                CreatedAt = DateTime.UtcNow,
                Operation = TaskOperation.Update,
                Timestamp = UnixTimeHelper.GetUnixTimeUtcNow(),
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
                Timestamp = UnixTimeHelper.ConvertToUnixTime(DateTime.UtcNow),
                HardwareVaultId = vaultId,
                AccountId = accountId
            };
        }
    }
}