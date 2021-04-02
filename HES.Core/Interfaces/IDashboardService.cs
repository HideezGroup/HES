using HES.Core.Entities;
using HES.Core.Models;
using HES.Core.Models.Dashboard;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDashboardService : IDisposable
    {
        Task<DashboardCard> GetServerCardAsync();
        string GetServerVersion();
        Task<int> GetHardwareVaultTasksCount();
        Task<List<HardwareVaultTask>> GetVaultTasks();
        Task<List<DashboardNotify>> GetServerNotifyAsync();
        Task<int> GetEmployeesCountAsync();
        Task<int> GetEmployeesOpenedSessionsCountAsync();
        Task<List<DashboardNotify>> GetEmployeesNotifyAsync();
        Task<DashboardCard> GetEmployeesCardAsync();
        Task<int> GetHardwareVaultsCountAsync();
        Task<int> GetReadyHardwareVaultsCountAsync();
        Task<List<DashboardNotify>> GetHardwareVaultsNotifyAsync();
        Task<DashboardCard> GetHardwareVaultsCardAsync();
        Task<int> GetWorkstationsCountAsync();
        Task<int> GetWorkstationsOnlineCountAsync();
        Task<List<DashboardNotify>> GetWorkstationsNotifyAsync();
        Task<DashboardCard> GetWorkstationsCardAsync();
    }
}