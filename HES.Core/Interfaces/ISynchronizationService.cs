using System;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ISynchronizationService
    {
        event Func<string, Task> UpdateAlarmPage;
        event Func<string, Task> UpdateEmployeePage;
        event Func<string, string, Task> UpdateEmployeeDetailsPage;
        event Func<string, Task> UpdateGroupsPage;
        event Func<string, string, Task> UpdateGroupDetailsPage;
        event Func<string, Task> UpdateHardwareVaultsPage;
        event Func<string, Task> UpdateHardwareVaultState;
        event Func<string, Task> UpdateTemplatesPage; 
        event Func<string, Task> UpdateSharedAccountsPage;
        event Func<string, Task> UpdateWorkstationsPage;
        event Func<string, string, Task> UpdateWorkstationDetailsPage;
        event Func<string, Task> UpdateDataProtectionPage;
        event Func<string, Task> UpdateAdministratorsPage;
        event Func<Task> UpdateAdministratorStatePage;
        event Func<string, Task> UpdateHardwareVaultProfilesPage;
        event Func<string, Task> UpdateLicensesPage;
        event Func<string, Task> UpdateParametersPage;
        event Func<string, Task> UpdateOrgSructureCompaniesPage;
        event Func<string, Task> UpdateOrgSructurePositionsPage;

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