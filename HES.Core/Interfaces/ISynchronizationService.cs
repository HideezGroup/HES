using System;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISynchronizationService
    {
        event Func<string, string, Task> UpdateAlarmPage;
        event Func<string, string, Task> UpdateEmployeePage;
        event Func<string, string, string, Task> UpdateEmployeeDetailsPage;
        event Func<string, string, Task> UpdateGroupsPage;
        event Func<string, string, string, Task> UpdateGroupDetailsPage;
        event Func<string, string, Task> UpdateHardwareVaultsPage;
        event Func<string, Task> UpdateHardwareVaultState;
        event Func<string, string, Task> UpdateTemplatesPage; 
        event Func<string, string, Task> UpdateSharedAccountsPage;
        event Func<string, string, Task> UpdateWorkstationsPage;
        event Func<string, string, string, Task> UpdateWorkstationDetailsPage;
        event Func<string, string, Task> UpdateDataProtectionPage;
        event Func<string, string, Task> UpdateAdministratorsPage;
        event Func<Task> UpdateAdministratorStatePage;
        event Func<string, string, Task> UpdateHardwareVaultProfilesPage;
        event Func<string, string, Task> UpdateLicensesPage;
        event Func<string, string, Task> UpdateParametersPage;
        event Func<string, string, Task> UpdateOrgSructureCompaniesPage;
        event Func<string, string, Task> UpdateOrgSructurePositionsPage;

        Task UpdateAlarm(string exceptPageId);
        Task UpdateEmployees(string exceptPageId);
        Task UpdateEmployeeDetails(string exceptPageId, string employeeId);
        Task UpdateGroups(string exceptPageId);
        Task UpdateGroupDetails(string exceptPageId, string groupId);
        Task UpdateHardwareVaults(string exceptPageId);
        Task HardwareVaultStateChanged(string hardwareVaultId);
        Task UpdateTemplates(string exceptPageId);
        Task UpdateSharedAccounts(string exceptPageId);
        Task UpdateWorkstations(string exceptPageId);
        Task UpdateWorkstationDetails(string exceptPageId, string workstationId);
        Task UpdateDataProtection(string exceptPageId);
        Task UpdateAdministrators(string exceptPageId);
        Task UpdateAdministratorState();
        Task UpdateHardwareVaultProfiles(string exceptPageId);
        Task UpdateLicenses(string exceptPageId);
        Task UpdateParameters(string exceptPageId);
        Task UpdateOrgSructureCompanies(string exceptPageId);
        Task UpdateOrgSructurePositions(string exceptPageId);
    }
}