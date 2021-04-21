using HES.Core.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IHardwareVaultTaskService
    {
        IQueryable<HardwareVaultTask> TaskQuery();
        Task<HardwareVaultTask> GetTaskByIdAsync(string id);
        Task<List<HardwareVaultTask>> GetHardwareVaultTasksNoTrackingAsync();
        Task AddRangeTasksAsync(IList<HardwareVaultTask> vaultTasks);
        Task AddPrimaryAsync(string vaultId, string accountId);
        Task AddProfileAsync(HardwareVault vault);
        Task DeleteTaskAsync(HardwareVaultTask vaultTask);
        Task DeleteTasksByVaultIdAsync(string vaultId);
        HardwareVaultTask GetAccountCreateTask(string vaultId, string accountId, string password, string otp);
        HardwareVaultTask GetAccountUpdateTask(string vaultId, string accountId);
        HardwareVaultTask GetAccountPwdUpdateTask(string vaultId, string accountId, string password);
        HardwareVaultTask GetAccountOtpUpdateTask(string vaultId, string accountId, string otp);
        HardwareVaultTask GetAccountDeleteTask(string vaultId, string accountId);
    }
}