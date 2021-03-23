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
    public interface IEmployeeService : IDisposable
    {
        #region Employee

        IQueryable<Employee> EmployeeQuery();
        Task<List<Employee>> GetEmployeesAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions);
        Task<int> GetEmployeesCountAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions);
        Task<Employee> GetEmployeeByIdAsync(string id, bool asNoTracking = false, bool byActiveDirectoryGuid = false);
        Task<IList<string>> GetEmployeeVaultIdsAsync(string employeeId);
        Task<Employee> ImportEmployeeAsync(Employee employee);
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task<bool> CheckEmployeeNameExistAsync(Employee employee);
        Task EditEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(string id);
        Task UpdateLastSeenAsync(string vaultId);
        Task UnchangedEmployeeAsync(Employee employee);
        Task RemoveFromHideezKeyOwnersAsync(string employeeId);

        #endregion

        #region SSO

        Task<bool> IsSsoEnabledAsync(Employee employee);

        #endregion

        #region Hardware Vault

        Task AddHardwareVaultAsync(string employeeId, string vaultId);
        Task RemoveHardwareVaultAsync(string vaultId, VaultStatusReason reason, bool isNeedBackup = false);

        #endregion

        #region Accounts

        Task<List<Account>> GetAccountsAsync(DataLoadingOptions<AccountFilter> dataLoadingOptions);
        Task<int> GetAccountsCountAsync(DataLoadingOptions<AccountFilter> dataLoadingOptions);
        Task<List<Account>> GetAccountsByEmployeeIdAsync(string employeeId);
        Task SetAsPrimaryAccountAsync(string employeeId, string accountId);
        Task<Account> GetAccountByIdAsync(string accountId);
        Task<Account> CreatePersonalAccountAsync(AccountAddModel personalAccount);
        Task EditPersonalAccountAsync(AccountEditModel personalAccount);
        Task EditPersonalAccountPwdAsync(Account account, AccountPassword accountPassword);
        Task EditPersonalAccountOtpAsync(Account account, AccountOtp accountOtp);
        Task<Account> AddSharedAccountAsync(string employeeId, string sharedAccountId);
        Task UnchangedPersonalAccountAsync(Account account);
        Task<Account> DeleteAccountAsync(string accountId);

        #endregion
    }
}