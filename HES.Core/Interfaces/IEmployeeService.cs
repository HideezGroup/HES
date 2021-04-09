﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models.Employees;
using HES.Core.Models.Accounts;
using HES.Core.Models.AppUsers;
using HES.Core.Models.DataTableComponent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IEmployeeService
    {
        #region Employee

        Task<List<Employee>> GetEmployeesAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions);
        Task<int> GetEmployeesCountAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions);
        Task<Employee> GetEmployeeByIdAsync(string employeeId, bool asNoTracking = false, bool byActiveDirectoryGuid = false);
        Task<List<Employee>> GetEmployeesADAsync();
        Task<int> GetEmployeesCountAsync();
        Task<Employee> GetEmployeeByNameAsync(string firstName, string lastName);
        Task<IList<string>> GetEmployeeVaultIdsAsync(string employeeId);
        Task<Employee> ImportEmployeeAsync(Employee employee);
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task<bool> CheckEmployeeNameExistAsync(Employee employee);
        Task EditEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(string employeeId);
        Task UpdateLastSeenAsync(string vaultId);
        void UnchangedEmployee(Employee employee);
        Task RemoveFromHideezKeyOwnersAsync(string employeeId);

        #endregion

        #region SSO
        bool IsSaml2PEnabled();
        Task<UserSsoInfo> GetUserSsoInfoAsync(Employee employee);
        Task EnableSsoAsync(Employee employee);
        Task DisableSsoAsync(Employee employee);
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