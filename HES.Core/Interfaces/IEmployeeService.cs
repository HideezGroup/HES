﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models;
using HES.Core.Models.Employees;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Account;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IEmployeeService
    {
        IQueryable<Employee> EmployeeQuery();
        Task<Employee> GetEmployeeByIdAsync(string id);
        Task<List<Employee>> GetEmployeesAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions);
        Task<int> GetEmployeesCountAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions);
        Task<IList<string>> GetEmployeeVaultIdsAsync(string employeeId);
        Task UnchangedEmployeeAsync(Employee employee);
        Task<Employee> CreateEmployeeAsync(Employee employee);
        Task<Employee> ImportEmployeeAsync(Employee employee);
        Task EditEmployeeAsync(Employee employee);
        Task DeleteEmployeeAsync(string id);
        Task UpdateLastSeenAsync(string vaultId);
        Task<bool> CheckEmployeeNameExistAsync(Employee employee);
        Task AddHardwareVaultAsync(string employeeId, string vaultId);
        Task RemoveHardwareVaultAsync(string vaultId, VaultStatusReason reason, bool isNeedBackup = false);
        Task<List<Account>> GetAccountsAsync(int skip, int take, string sortColumn, ListSortDirection sortDirection, string searchText, string employeeId);
        Task<int> GetAccountsCountAsync(string searchText, string employeeId);
        Task<List<Account>> GetAccountsByEmployeeIdAsync(string employeeId);
        Task<Account> GetAccountByIdAsync(string accountId);
        Task<Account> CreatePersonalAccountAsync(PersonalAccount personalAccount, bool isWorkstationAccount = false);
        Task<Account> CreateWorkstationAccountAsync(HES.Core.Models.Web.Account.WorkstationAccount workstationAccount);
        Task<Account> CreateWorkstationAccountAsync(WorkstationLocal workstationAccount);
        Task<Account> CreateWorkstationAccountAsync(WorkstationDomain workstationAccount);
        Task<Account> CreateWorkstationAccountAsync(WorkstationMicrosoft workstationAccount);
        Task<Account> CreateWorkstationAccountAsync(WorkstationAzureAD workstationAccount);
        Task SetAsWorkstationAccountAsync(string employeeId, string accountId);
        Task EditPersonalAccountAsync(Account account);
        Task EditPersonalAccountPwdAsync(Account account, AccountPassword accountPassword);
        Task EditPersonalAccountOtpAsync(Account account, AccountOtp accountOtp);
        Task UnchangedPersonalAccountAsync(Account account);
        Task<Account> AddSharedAccountAsync(string employeeId, string sharedAccountId);
        Task<Account> DeleteAccountAsync(string accountId);
    }
}