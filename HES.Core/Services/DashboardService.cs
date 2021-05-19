using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models;
using HES.Core.Models.Dashboard;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IEmployeeService _employeeService;
        private readonly IWorkstationAuditService _workstationAuditService;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly IWorkstationService _workstationService;
        private readonly IHardwareVaultService _hardwareVaultService;

        public DashboardService(IEmployeeService employeeService,
                                IWorkstationAuditService workstationAuditService,
                                IHardwareVaultTaskService hardwareVaultTaskService,
                                IWorkstationService workstationService,
                                IHardwareVaultService hardwareVaultService)
        {
            _employeeService = employeeService;
            _workstationAuditService = workstationAuditService;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _workstationService = workstationService;
            _hardwareVaultService = hardwareVaultService;
        }

        #region Server

        public string GetServerVersion()
        {
            return ServerConstants.Version;
        }

        public async Task<int> GetHardwareVaultTasksCount()
        {
            return await _hardwareVaultTaskService.TaskQuery().CountAsync();
        }

        public async Task<List<HardwareVaultTask>> GetVaultTasks()
        {
            return await _hardwareVaultTaskService.TaskQuery().ToListAsync();
        }

        public async Task<List<DashboardNotify>> GetServerNotifyAsync()
        {
            var list = new List<DashboardNotify>();
            var longPendingTasksCount = await _hardwareVaultTaskService.TaskQuery().Where(d => d.CreatedAt <= DateTime.UtcNow.AddDays(-3)).CountAsync();

            if (longPendingTasksCount > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = Resources.Resource.Dashboard_ServerCard_LongPendingTasks,
                    Count = longPendingTasksCount,
                    Page = "long-pending-tasks"
                });
            }

            return list;
        }

        public async Task<DashboardCard> GetServerCardAsync()
        {
            return new DashboardCard()
            {
                CardTitle = Resources.Resource.Dashboard_ServerCard_Title,
                CardLogo = "/svg/logo-server.svg",
                LeftText = Resources.Resource.Dashboard_ServerCard_LeftText,
                LeftValue = $"{GetServerVersion()}",
                LeftLink = Resources.Resource.Dashboard_ServerCard_LeftLink,
                RightText = Resources.Resource.Dashboard_ServerCard_RightText,
                RightValue = $"{await GetHardwareVaultTasksCount()}",
                RightLink = "#",
                Notifications = await GetServerNotifyAsync()
            };
        }

        #endregion

        #region Employees

        public async Task<int> GetEmployeesCountAsync()
        {
            return await _employeeService.GetEmployeesCountAsync();
        }

        public async Task<int> GetEmployeesOpenedSessionsCountAsync()
        {
            return await _workstationAuditService
               .SessionQuery()
               .Where(w => w.EndDate == null)
               .CountAsync();
        }

        public async Task<List<DashboardNotify>> GetEmployeesNotifyAsync()
        {
            var list = new List<DashboardNotify>();

            var nonHideezUnlock = await _workstationAuditService
                .SessionQuery()
                .Where(w => w.StartDate >= DateTime.UtcNow.AddDays(-1) && w.UnlockedBy == Hideez.SDK.Communication.SessionSwitchSubject.NonHideez)
                .CountAsync();

            if (nonHideezUnlock > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = Resources.Resource.Dashboard_EmployeesCard_NonHideezUnlock,
                    Count = nonHideezUnlock,
                    Page = $"{Routes.WorkstationSessions}/NonHideezUnlock"
                });
            }

            var longOpenSession = await _workstationAuditService
                .SessionQuery()
                .Where(w => w.StartDate <= DateTime.UtcNow.AddHours(-12) && w.EndDate == null)
                .CountAsync();

            if (longOpenSession > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = Resources.Resource.Dashboard_EmployeesCard_LongOpenSession,
                    Count = longOpenSession,
                    Page = $"{Routes.WorkstationSessions}/LongOpenSession"
                });
            }

            return list;
        }

        public async Task<DashboardCard> GetEmployeesCardAsync()
        {
            return new DashboardCard()
            {
                CardTitle = Resources.Resource.Dashboard_EmployeesCard_Title,
                CardLogo = "/svg/logo-employees.svg",
                LeftText = Resources.Resource.Dashboard_EmployeesCard_LeftText,
                LeftValue = $"{await GetEmployeesCountAsync()}",
                LeftLink = "/Employees",
                RightText = Resources.Resource.Dashboard_EmployeesCard_RightText,
                RightValue = $"{await GetEmployeesOpenedSessionsCountAsync()}",
                RightLink = $"{Routes.WorkstationSessions}/OpenedSessions",
                Notifications = await GetEmployeesNotifyAsync()
            };
        }

        #endregion

        #region Hardware Vaults

        public async Task<int> GetHardwareVaultsCountAsync()
        {
            return await _hardwareVaultService.VaultQuery().CountAsync();
        }

        public async Task<int> GetReadyHardwareVaultsCountAsync()
        {
            return await _hardwareVaultService.VaultQuery().Where(d => d.EmployeeId == null).CountAsync();
        }

        public async Task<List<DashboardNotify>> GetHardwareVaultsNotifyAsync()
        {
            var list = new List<DashboardNotify>();

            var lowBattery = await _hardwareVaultService
                .VaultQuery()
                .Where(d => d.Battery <= 30)
                .CountAsync();

            if (lowBattery > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = Resources.Resource.Dashboard_HardwareVaultsCard_LowBattery,
                    Count = lowBattery,
                    Page = $"{Routes.HardwareVaults}/LowBattery"
                });
            }

            var vaultLocked = await _hardwareVaultService
                .VaultQuery()
                .Where(d => d.Status == VaultStatus.Locked)
                .CountAsync();

            if (vaultLocked > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = Resources.Resource.Dashboard_HardwareVaultsCard_VaultLocked,
                    Count = vaultLocked,
                    Page = $"{Routes.HardwareVaults}/VaultLocked"
                });
            }

            var licenseWarning = await _hardwareVaultService
                .VaultQuery()
                .Where(d => d.LicenseStatus == VaultLicenseStatus.Warning)
                .CountAsync();

            if (licenseWarning > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = Resources.Resource.Dashboard_HardwareVaultsCard_LicenseWarning,
                    Count = licenseWarning,
                    Page = $"{Routes.HardwareVaults}/LicenseWarning"
                });
            }

            var licenseCritical = await _hardwareVaultService
                .VaultQuery()
                .Where(d => d.LicenseStatus == VaultLicenseStatus.Critical)
                .CountAsync();

            if (licenseCritical > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = Resources.Resource.Dashboard_HardwareVaultsCard_LicenseCritical,
                    Count = licenseCritical,
                    Page = $"{Routes.HardwareVaults}/LicenseCritical"
                });
            }

            var licenseExpired = await _hardwareVaultService
                .VaultQuery()
                .Where(d => d.LicenseStatus == VaultLicenseStatus.Expired)
                .CountAsync();

            if (licenseExpired > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = Resources.Resource.Dashboard_HardwareVaultsCard_LicenseExpired,
                    Count = licenseExpired,
                    Page = $"{Routes.HardwareVaults}/LicenseExpired"
                });
            }

            return list;
        }

        public async Task<DashboardCard> GetHardwareVaultsCardAsync()
        {
            return new DashboardCard()
            {
                CardTitle = Resources.Resource.Dashboard_HardwareVaultsCard_Title,
                CardLogo = "/svg/logo-hardware-vaults.svg",
                LeftText = Resources.Resource.Dashboard_HardwareVaultsCard_LeftText,
                LeftValue = $"{await GetHardwareVaultsCountAsync()}",
                LeftLink = Routes.HardwareVaults,
                RightText = Resources.Resource.Dashboard_HardwareVaultsCard_RightText,
                RightValue = $"{await GetReadyHardwareVaultsCountAsync()}",
                RightLink = $"{Routes.HardwareVaults}/VaultReady",
                Notifications = await GetHardwareVaultsNotifyAsync()
            };
        }

        #endregion

        #region Workstations

        public async Task<int> GetWorkstationsCountAsync()
        {
            return await _workstationService.GetWorkstationsCountAsync();
        }

        public async Task<int> GetWorkstationsOnlineCountAsync()
        {
            return await Task.FromResult(RemoteWorkstationConnectionsService.GetWorkstationsOnlineCount());
        }

        public async Task<List<DashboardNotify>> GetWorkstationsNotifyAsync()
        {
            var list = new List<DashboardNotify>();

            var notApproveCount = await _workstationService.GetWorkstationsNotApproveCountAsync();

            if (notApproveCount > 0)
            {
                list.Add(new DashboardNotify()
                {
                    Message = Resources.Resource.Dashboard_WorkstationsCard_WaitingForApproval,
                    Count = notApproveCount,
                    Page = $"{Routes.Workstations}/NotApproved"
                });
            }

            return list;
        }

        public async Task<DashboardCard> GetWorkstationsCardAsync()
        {
            return new DashboardCard()
            {
                CardTitle = Resources.Resource.Dashboard_WorkstationsCard_Title,
                CardLogo = "/svg/logo-workstations.svg",
                LeftText = Resources.Resource.Dashboard_WorkstationsCard_LeftText,
                LeftValue = $"{await GetWorkstationsCountAsync()}",
                LeftLink = Routes.Workstations,
                RightText = Resources.Resource.Dashboard_WorkstationsCard_RightText,
                RightValue = $"{await GetWorkstationsOnlineCountAsync()}",
                RightLink = $"{Routes.Workstations}/Online",
                Notifications = await GetWorkstationsNotifyAsync()
            };
        }

        #endregion
    }
}