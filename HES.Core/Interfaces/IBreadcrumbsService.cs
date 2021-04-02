using HES.Core.Models.Breadcrumb;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IBreadcrumbsService
    {
        event Func<List<Breadcrumb>, Task> OnSet;
        Task SetDashboard();
        Task SetEmployees();
        Task SetEmployeeDetails(string name);
        Task SetHardwareVaults();
        Task SetTemplates();
        Task SetDataProtection();
        Task SetGroups();
        Task SetAdministrators();
        Task SetHardwareVaultProfiles();
        Task SetSharedAccounts();
        Task SetAuditWorkstationEvents();
        Task SetAuditWorkstationSessions();
        Task SetAuditSummaries();
        Task SetGroupDetails(string name);
        Task SetLicenseOrders();
        Task SetParameters();
        Task SetOrgStructure();
        Task SetWorkstations();
        Task SetWorkstationDetails(string name);
        Task SetTwoFactorAuthentication();
        Task SetShowRecoveryCodes();
        Task SetResetAuthenticator();
        Task SetProfile();
        Task SetPersonalData();
        Task SetGenerateRecoveryCodes();
        Task SetEnableAuthenticator();
        Task SetDisable2fa();
        Task SetDeletePersonalData();
        Task SetAlarm();
    }
}