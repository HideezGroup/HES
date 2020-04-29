﻿using HES.Core.Entities;
using HES.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IDashboardService
    {
        string GetServerVersion();
        Task<int> GetDeviceTasksCount();
        Task<List<HardwareVaultTask>> GetVaultTasks();
        Task<List<DashboardNotify>> GetServerNotifyAsync();
        Task<int> GetEmployeesCountAsync();
        Task<int> GetEmployeesOpenedSessionsCountAsync();
        Task<List<DashboardNotify>> GetEmployeesNotifyAsync();
        Task<int> GetHardwareVaultsCountAsync();
        Task<int> GetReadyHardwareVaultsCountAsync();
        Task<List<DashboardNotify>> GetHardwareVaultsNotifyAsync();
        Task<int> GetWorkstationsCountAsync();
        Task<int> GetWorkstationsOnlineCountAsync();
        Task<List<DashboardNotify>> GetWorkstationsNotifyAsync();
    }
}
