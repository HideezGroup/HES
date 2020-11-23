using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models.Employees;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IEmployeeService: IDisposable    
    {
        IQueryable<Employee> EmployeeQuery();
        Task<List<Employee>> GetEmployeesAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions);
        Task<int> GetEmployeesCountAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions);
        Task<Employee> GetEmployeeByIdAsync(string id, bool asNoTracking = false, bool byActiveDirectoryGuid = false);
        Task<IList<string>> GetEmployeeVaultIdsAsync(string employeeId);
        Task<Employee> ImportEmployeeAsync(Employee employee);
        Task SyncEmployeeAsync(List<Employee> impotedEmployees);
        Task SyncEmployeeAccessAsync(List<string> membersGuid);
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task<bool> CheckEmployeeNameExistAsync(Employee employee);
        Task EditEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(string id);
        Task UpdateLastSeenAsync(string vaultId);
        Task UnchangedEmployeeAsync(Employee employee);
        Task ToLocalUserAsync(string employeeId);
        Task AddHardwareVaultAsync(string employeeId, string vaultId);
        Task RemoveHardwareVaultAsync(string vaultId, VaultStatusReason reason, bool isNeedBackup = false);
        Task<Account> CreatePersonalAccountAsync(PersonalAccount personalAccount, bool isWorkstationAccount = false);
        Task<Account> CreateWorkstationAccountAsync(WorkstationAccount workstationAccount);
        Task<Account> CreateWorkstationAccountAsync(WorkstationDomain workstationAccount);
        Task<List<Account>> GetAccountsAsync(DataLoadingOptions<AccountFilter> dataLoadingOptions);
        Task<int> GetAccountsCountAsync(DataLoadingOptions<AccountFilter> dataLoadingOptions);
        Task<List<Account>> GetAccountsByEmployeeIdAsync(string employeeId);
        Task SetAsWorkstationAccountAsync(string employeeId, string accountId);
        Task<Account> GetAccountByIdAsync(string accountId);
        Task EditPersonalAccountAsync(Account account);
        Task EditPersonalAccountPwdAsync(Account account, AccountPassword accountPassword);
        Task EditPersonalAccountOtpAsync(Account account, AccountOtp accountOtp);
        Task<Account> AddSharedAccountAsync(string employeeId, string sharedAccountId);
        Task UnchangedPersonalAccountAsync(Account account);
        Task<Account> DeleteAccountAsync(string accountId);
    }
}