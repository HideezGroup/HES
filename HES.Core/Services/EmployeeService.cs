using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Employees;
using HES.Core.Models.Accounts;
using HES.Core.Models.AppUsers;
using HES.Core.Models.DataTableComponent;
using HES.Core.Utilities;
using Hideez.SDK.Communication.PasswordManager;
using Hideez.SDK.Communication.Security;
using Hideez.SDK.Communication.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Linq.Expressions;

namespace HES.Core.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly ISharedAccountService _sharedAccountService;
        private readonly IWorkstationService _workstationService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFido2Service _fido2Service;
        private readonly IConfiguration _configuration;
        private readonly IApplicationDbContext _dbContext;


        public EmployeeService(
                               IHardwareVaultService hardwareVaultService,
                               IHardwareVaultTaskService hardwareVaultTaskService,
                               ISharedAccountService sharedAccountService,
                               IWorkstationService workstationService,
                               IDataProtectionService dataProtectionService,
                               UserManager<ApplicationUser> userManager,
                               IFido2Service fido2Service,
                               IConfiguration configuration,
                               IApplicationDbContext dbContext)
        {
            _hardwareVaultService = hardwareVaultService;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _sharedAccountService = sharedAccountService;
            _workstationService = workstationService;
            _dataProtectionService = dataProtectionService;
            _userManager = userManager;
            _fido2Service = fido2Service;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        #region Employee

        public async Task<Employee> GetEmployeeByIdAsync(string employeeId, bool asNoTracking = false, bool byActiveDirectoryGuid = false)
        {
            var query = _dbContext.Employees
                .Include(e => e.Department.Company)
                .Include(e => e.Position)
                .Include(e => e.SoftwareVaults)
                .Include(e => e.SoftwareVaultInvitations)
                .Include(e => e.HardwareVaults)
                .ThenInclude(e => e.HardwareVaultProfile)
                .Include(e => e.Accounts)
                .AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (byActiveDirectoryGuid)
            {
                return await query.FirstOrDefaultAsync(e => e.ActiveDirectoryGuid == employeeId);
            }

            return await query.FirstOrDefaultAsync(e => e.Id == employeeId);
        }

        public void UnchangedEmployee(Employee employee)
        {
            _dbContext.Unchanged(employee);
        }

        public async Task<List<Employee>> GetEmployeesAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions)
        {
            return await EmployeeQuery(dataLoadingOptions).Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetEmployeesCountAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions)
        {
            return await EmployeeQuery(dataLoadingOptions).CountAsync();
        }

        public async Task<int> GetEmployeesCountAsync()
        {
            return await _dbContext.Employees.CountAsync();
        }

        public async Task<List<Employee>> GetEmployeesADAsync()
        {
            return await _dbContext.Employees.Where(x => x.ActiveDirectoryGuid != null).ToListAsync();
        }

        public async Task<Employee> GetEmployeeByNameAsync(string firstName, string lastName)
        {
            return await _dbContext.Employees.FirstOrDefaultAsync(x => x.FirstName == firstName && x.LastName == lastName);
        }

        private IQueryable<Employee> EmployeeQuery(DataLoadingOptions<EmployeeFilter> dataLoadingOptions)
        {
            var query = _dbContext.Employees
                .Include(x => x.Department.Company)
                .Include(x => x.Position)
                .Include(x => x.HardwareVaults)
                .Include(x => x.SoftwareVaults)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(dataLoadingOptions.Filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Email != null)
                {
                    query = query.Where(w => w.Email.Contains(dataLoadingOptions.Filter.Email, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.PhoneNumber != null)
                {
                    query = query.Where(w => w.PhoneNumber.Contains(dataLoadingOptions.Filter.PhoneNumber, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(x => x.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(x => x.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Position != null)
                {
                    query = query.Where(x => x.Position.Name.Contains(dataLoadingOptions.Filter.Position, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.LastSeenStartDate != null)
                {
                    query = query.Where(w => w.LastSeen >= dataLoadingOptions.Filter.LastSeenStartDate);
                }
                if (dataLoadingOptions.Filter.LastSeenEndDate != null)
                {
                    query = query.Where(x => x.LastSeen <= dataLoadingOptions.Filter.LastSeenEndDate);
                }
                if (dataLoadingOptions.Filter.VaultsCount != null)
                {
                    query = query.Where(x => (x.HardwareVaults.Count + x.SoftwareVaults.Count) == dataLoadingOptions.Filter.VaultsCount);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => (x.FirstName + " " + x.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Email.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.PhoneNumber.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Position.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.HardwareVaults.Count + x.SoftwareVaults.Count).ToString().Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(Employee.FullName):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.FirstName).ThenBy(x => x.LastName) : query.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName);
                    break;
                case nameof(Employee.Email):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Email) : query.OrderByDescending(x => x.Email);
                    break;
                case nameof(Employee.PhoneNumber):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.PhoneNumber) : query.OrderByDescending(x => x.PhoneNumber);
                    break;
                case nameof(Employee.Department.Company):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Company.Name) : query.OrderByDescending(x => x.Department.Company.Name);
                    break;
                case nameof(Employee.Department):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Department.Name) : query.OrderByDescending(x => x.Department.Name);
                    break;
                case nameof(Employee.Position):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Position.Name) : query.OrderByDescending(x => x.Position.Name);
                    break;
                case nameof(Employee.LastSeen):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LastSeen) : query.OrderByDescending(x => x.LastSeen);
                    break;
                case nameof(Employee.VaultsCount):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.HardwareVaults.Count).ThenBy(x => x.SoftwareVaults.Count) : query.OrderByDescending(x => x.HardwareVaults.Count).ThenByDescending(x => x.SoftwareVaults.Count);
                    break;
            }

            return query;
        }

        public async Task<IList<string>> GetEmployeeVaultIdsAsync(string employeeId)
        {
            var employee = await GetEmployeeByIdAsync(employeeId);
            return employee.HardwareVaults.Select(x => x.Id).ToList();
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            // If the field is NULL then the unique check does not work; therefore, we write empty
            employee.LastName ??= string.Empty;

            employee.DepartmentId ??= employee.DepartmentId;
            employee.PositionId ??= employee.PositionId;

            await ThrowIfEmployeeExistAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName);

            return await CreateEmployeeInDatabase(employee);
        }

        private async Task<Employee> CreateEmployeeInDatabase(Employee employee)
        {
            var result = _dbContext.Employees.Add(employee);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        private async Task<Employee> UpdateEmployeeInDatabase(Employee employee)
        {
            var result = _dbContext.Employees.Update(employee);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        private async Task ThrowIfEmployeeExistAsync(Expression<Func<Employee, bool>> predicate)
        {
            var employeeExist = await _dbContext.ExistAsync(predicate);
            if (employeeExist)
            {
                throw new HESException(HESCode.EmployeeAlreadyExist);
            }
        }

        public async Task<Employee> ImportEmployeeAsync(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            // If the field is NULL then the unique check does not work, therefore we write empty field
            employee.LastName = employee.LastName ?? string.Empty;

            var employeeByGuid = await _dbContext.Employees.FirstOrDefaultAsync(x => x.ActiveDirectoryGuid == employee.ActiveDirectoryGuid);
            if (employeeByGuid != null)
            {
                return employeeByGuid;
            }

            var employeeByName = await _dbContext.Employees.FirstOrDefaultAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName);
            if (employeeByName != null)
            {
                employeeByName.ActiveDirectoryGuid = employee.ActiveDirectoryGuid;
                return await UpdateEmployeeInDatabase(employee);
            }

            return await CreateEmployeeInDatabase(employee);
        }

        public async Task EditEmployeeAsync(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            // If the field is NULL then the unique check does not work; therefore, we write empty
            employee.LastName ??= string.Empty;

            employee.DepartmentId ??= employee.DepartmentId;
            employee.PositionId ??= employee.PositionId;

            await ThrowIfEmployeeExistAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName && x.Id != employee.Id);

            await UpdateEmployeeInDatabase(employee);
        }

        public async Task DeleteEmployeeAsync(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            var employee = await GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                throw new HESException(HESCode.EmployeeNotFound);
            }

            var hardwareVaultsExist = await _dbContext.HardwareVaults
                .Where(x => x.EmployeeId == employeeId)
                .AnyAsync();

            if (hardwareVaultsExist)
            {
                throw new HESException(HESCode.HardwareVaultUntieBeforeRemove);
            }

            //TODO SoftwareVault

            _dbContext.Employees.Remove(employee);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateLastSeenAsync(string vaultId)
        {
            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);
            if (vault?.EmployeeId == null)
            {
                return;
            }

            var employee = await GetEmployeeByIdAsync(vault.EmployeeId);
            if (employee == null)
            {
                throw new HESException(HESCode.EmployeeNotFound);
            }

            employee.LastSeen = DateTime.UtcNow;
            await UpdateEmployeeInDatabase(employee);
        }

        public async Task<bool> CheckEmployeeNameExistAsync(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            // If the field is NULL then the unique check does not work; therefore, we write empty
            employee.LastName ??= string.Empty;
            return await _dbContext.ExistAsync<Employee>(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName);
        }

        public async Task RemoveFromHideezKeyOwnersAsync(string employeeId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            var employee = await GetEmployeeByIdAsync(employeeId);

            employee.HardwareVaults.ForEach(async (x) => await RemoveHardwareVaultAsync(x.Id, VaultStatusReason.Withdrawal));

            employee.ActiveDirectoryGuid = null;
            employee.WhenChanged = null;

            await UpdateEmployeeInDatabase(employee);
        }

        #endregion

        #region SSO

        public bool IsSaml2PEnabled()
        {
            if (!string.IsNullOrWhiteSpace(_configuration.GetValue<string>("SAML2P:LicenseName")) && !string.IsNullOrWhiteSpace(_configuration.GetValue<string>("SAML2P:LicenseKey")))
            {
                return true;
            }

            return false;
        }

        public async Task<UserSsoInfo> GetUserSsoInfoAsync(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            var user = await _dbContext.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Email == employee.Email);

            if (user == null)
            {
                return new UserSsoInfo();
            }

            var cred = await _fido2Service.GetCredentialsByUserEmail(employee.Email);

            return new UserSsoInfo
            {
                IsSsoEnabled = user != null,
                UserEmail = user.Email,
                UserRole = user.UserRoles.FirstOrDefault().Role.Name,
                SecurityKeyName = cred.Count > 0 ? "Added" : "Not added"
            };
        }

        public async Task EnableSsoAsync(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            var user = new ApplicationUser
            {
                FullName = employee.FullName,
                PhoneNumber = employee.PhoneNumber,
                UserName = employee.Email,
                Email = employee.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user);

            if (!result.Succeeded)
            {
                string errors = string.Empty;
                foreach (var item in result.Errors)
                    errors += $"Code: {item.Code} Description: {item.Description} {Environment.NewLine}";

                throw new Exception(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, ApplicationRoles.User);

            if (!roleResult.Succeeded)
            {
                string errors = string.Empty;
                foreach (var item in result.Errors)
                    errors += $"Code: {item.Code} Description: {item.Description} {Environment.NewLine}";

                throw new Exception(errors);
            }
        }

        public async Task DisableSsoAsync(Employee employee)
        {
            if (employee == null)
            {
                throw new ArgumentNullException(nameof(employee));
            }

            var user = await _userManager.FindByEmailAsync(employee.Email);
            if (user == null)
            {
                throw new HESException(HESCode.UserNotFound);
            }

            await _fido2Service.RemoveCredentialsByUsername(employee.Email);
            await _userManager.DeleteAsync(user);
        }

        #endregion

        #region Hardware Vault

        public async Task AddHardwareVaultAsync(string employeeId, string vaultId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ArgumentNullException(nameof(vaultId));
            }

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                throw new HESException(HESCode.EmployeeNotFound);
            }

            if (employee.HardwareVaults.Count > 0)
            {
                throw new HESException(HESCode.OneHardwareVaultConstraint);
            }

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);
            if (vault == null)
            {
                throw new HESException(HESCode.HardwareVaultNotFound);
            }

            if (vault.Status != VaultStatus.Ready)
            {
                throw new HESException(HESCode.HardwareVaultCannotReserve);
            }

            vault.EmployeeId = employeeId;
            vault.Status = VaultStatus.Reserved;
            vault.IsStatusApplied = false;
            vault.MasterPassword = _dataProtectionService.Encrypt(GenerateMasterPassword());

            var accounts = await GetAccountsByEmployeeIdAsync(employeeId);
            var tasks = new List<HardwareVaultTask>();

            // Create a task for accounts that were created without a vault
            foreach (var account in accounts.Where(x => x.Password != null))
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountCreateTask(vault.Id, account.Id, account.Password, account.OtpSecret));
            }

            if (tasks.Count > 0)
            {
                vault.NeedSync = true;
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultService.UpdateVaultAsync(vault);
                await _hardwareVaultService.CreateVaultActivationAsync(vaultId);

                if (tasks.Count > 0)
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);

                transactionScope.Complete();
            }
        }

        public async Task RemoveHardwareVaultAsync(string vaultId, VaultStatusReason reason, bool isNeedBackup = false)
        {
            if (string.IsNullOrWhiteSpace(vaultId))
            {
                throw new ArgumentNullException(nameof(vaultId));
            }

            _dataProtectionService.Validate();

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);
            if (vault == null)
            {
                throw new HESException(HESCode.HardwareVaultNotFound);
            }

            if (vault.Status != VaultStatus.Reserved && vault.Status != VaultStatus.Active &&
                vault.Status != VaultStatus.Locked && vault.Status != VaultStatus.Suspended)
            {
                throw new HESException(HESCode.HardwareVaultВoesNotAllowToRemove);
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultTaskService.DeleteTasksByVaultIdAsync(vaultId);
                await _workstationService.DeleteWorkstationHardwareVaultPairsByVaultIdAsync(vaultId);

                if (vault.Status == VaultStatus.Reserved && !vault.IsStatusApplied)
                {
                    await _hardwareVaultService.SetReadyStatusAsync(vault);
                }
                else
                {
                    var employeeVaultsCount = vault.Employee.HardwareVaults.Count();

                    if (employeeVaultsCount == 1)
                    {
                        await RemovePrimaryAccountIdAsync(vault.EmployeeId);
                        await DeleteAccountsByEmployeeIdAsync(vault.EmployeeId);
                    }

                    vault.StatusReason = reason;
                    await _hardwareVaultService.SetDeactivatedStatusAsync(vault);
                }

                transactionScope.Complete();
            }
        }

        #endregion

        #region Accounts

        public async Task<Account> GetAccountByIdAsync(string accountId, bool asNoTracking = false)
        {
            var query = _dbContext.Accounts
                .Include(x => x.Employee)
                .ThenInclude(x => x.HardwareVaults)
                .Include(x => x.SharedAccount)
                .AsQueryable();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync(x => x.Id == accountId);
        }

        public async Task<List<Account>> GetAccountsAsync(DataLoadingOptions<AccountFilter> dataLoadingOptions)
        {
            return await AccountQuery(dataLoadingOptions).Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetAccountsCountAsync(DataLoadingOptions<AccountFilter> dataLoadingOptions)
        {
            return await AccountQuery(dataLoadingOptions).CountAsync();
        }

        private IQueryable<Account> AccountQuery(DataLoadingOptions<AccountFilter> dataLoadingOptions)
        {
            var query = _dbContext.Accounts
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .Where(x => x.EmployeeId == dataLoadingOptions.EntityId && x.Deleted == false);

            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x =>
                                    x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Urls.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Apps.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Login.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(Account.Name):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                    break;
                case nameof(Account.Urls):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Urls) : query.OrderByDescending(x => x.Urls);
                    break;
                case nameof(Account.Apps):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Apps) : query.OrderByDescending(x => x.Apps);
                    break;
                case nameof(Account.Login):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Login) : query.OrderByDescending(x => x.Login);
                    break;
                case nameof(Account.AccountType):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.AccountType) : query.OrderByDescending(x => x.AccountType);
                    break;
                case nameof(Account.CreatedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt);
                    break;
                case nameof(Account.UpdatedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.UpdatedAt) : query.OrderByDescending(x => x.UpdatedAt);
                    break;
                case nameof(Account.PasswordUpdatedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.PasswordUpdatedAt) : query.OrderByDescending(x => x.PasswordUpdatedAt);
                    break;
                case nameof(Account.OtpUpdatedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.OtpUpdatedAt) : query.OrderByDescending(x => x.OtpUpdatedAt);
                    break;
            }

            return query;
        }

        public async Task<List<Account>> GetAccountsByEmployeeIdAsync(string employeeId)
        {
            return await _dbContext.Accounts
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .Where(x => x.EmployeeId == employeeId && x.Deleted == false)
                .ToListAsync();
        }

        public async Task<List<Account>> GetAccountsBySharedAccountIdAsync(string sharedAccountId)
        {
            return await _dbContext.Accounts
                .Include(x => x.Employee.HardwareVaults)
                .Where(x => x.SharedAccountId == sharedAccountId && x.Deleted == false)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Account> CreatePersonalAccountAsync(AccountAddModel personalAccount)
        {
            if (personalAccount == null)
            {
                throw new ArgumentNullException(nameof(personalAccount));
            }

            _dataProtectionService.Validate();

            var account = new Account()
            {
                Id = Guid.NewGuid().ToString(),
                Name = personalAccount.Name,
                Urls = Validation.VerifyUrls(personalAccount.Urls),
                Apps = personalAccount.Apps,
                Login = await ValidateAccountNameAndLoginAsync(personalAccount.EmployeeId, personalAccount.Name, personalAccount.GetLogin()),
                AccountType = AccountType.Personal,
                LoginType = personalAccount.LoginType,
                CreatedAt = DateTime.UtcNow,
                PasswordUpdatedAt = DateTime.UtcNow,
                OtpUpdatedAt = Validation.VerifyOtpSecret(personalAccount.OtpSecret) != null ? new DateTime?(DateTime.UtcNow) : null,
                Password = _dataProtectionService.Encrypt(personalAccount.Password),
                OtpSecret = _dataProtectionService.Encrypt(personalAccount.OtpSecret),
                UpdateInActiveDirectory = personalAccount.UpdateInActiveDirectory,
                EmployeeId = personalAccount.EmployeeId,
                StorageId = new StorageId().Data
            };

            var employee = await GetEmployeeByIdAsync(personalAccount.EmployeeId);
            var tasks = new List<HardwareVaultTask>();

            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountCreateTask(vault.Id, account.Id, account.Password, account.OtpSecret));
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                _dbContext.Accounts.Add(account);
                await _dbContext.SaveChangesAsync();
                await SetAsPrimaryAccountIfEmptyAsync(account.EmployeeId, account.Id);

                if (tasks.Count > 0)
                {
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                    await _hardwareVaultService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                }

                transactionScope.Complete();
            }

            return account;
        }

        private async Task<string> ValidateAccountNameAndLoginAsync(string employeeId, string name, string login, string accountId = null)
        {
            var query = _dbContext.Accounts.Where(x => x.EmployeeId == employeeId && x.Name == name && x.Login == login && x.Deleted == false);

            if (accountId != null)
            {
                query = query.Where(x => x.Id != accountId);
            }

            var exist = await query.AnyAsync();

            if (exist)
            {
                throw new HESException(HESCode.AccountExist);
            }

            return login;
        }

        public async Task SetAsPrimaryAccountAsync(string employeeId, string accountId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            if (string.IsNullOrWhiteSpace(accountId))
            {
                throw new ArgumentNullException(nameof(accountId));
            }

            var employee = await GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                throw new HESException(HESCode.EmployeeNotFound);
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                employee.PrimaryAccountId = accountId;
                await UpdateEmployeeInDatabase(employee);

                foreach (var vault in employee.HardwareVaults)
                {
                    await _hardwareVaultTaskService.AddPrimaryAsync(vault.Id, accountId);
                    await _hardwareVaultService.UpdateNeedSyncAsync(vault, true);
                }

                transactionScope.Complete();
            }
        }

        private async Task SetAsPrimaryAccountIfEmptyAsync(string employeeId, string accountId)
        {
            var employee = await GetEmployeeByIdAsync(employeeId);

            if (string.IsNullOrWhiteSpace(employee.PrimaryAccountId))
            {
                employee.PrimaryAccountId = accountId;
                await UpdateEmployeeInDatabase(employee);
            }
        }

        private async Task RemovePrimaryAccountIdAsync(string employeeId)
        {
            var employee = await GetEmployeeByIdAsync(employeeId);
            if (employee == null)
            {
                throw new HESException(HESCode.EmployeeNotFound);
            }

            employee.PrimaryAccountId = null;
            await UpdateEmployeeInDatabase(employee);
        }

        public async Task EditPersonalAccountAsync(AccountEditModel personalAccount)
        {
            if (personalAccount == null)
            {
                throw new ArgumentNullException(nameof(personalAccount));
            }

            _dataProtectionService.Validate();
            await ValidateAccountNameAndLoginAsync(personalAccount.EmployeeId, personalAccount.Name, personalAccount.GetLogin(), personalAccount.Id);
            personalAccount.Urls = Validation.VerifyUrls(personalAccount.Urls);

            var employee = await GetEmployeeByIdAsync(personalAccount.EmployeeId);
            if (employee == null)
            {
                throw new HESException(HESCode.EmployeeNotFound);
            }

            var account = await _dbContext.Accounts.FirstOrDefaultAsync(x => x.Id == personalAccount.Id);
            if (account == null)
            {
                throw new HESException(HESCode.AccountNotFound);
            }

            account = personalAccount.SetNewValue(account);

            // Create tasks if there are vaults
            var tasks = new List<HardwareVaultTask>();
            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountUpdateTask(vault.Id, personalAccount.Id));
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await UpdateAccountsAsync(account);

                if (tasks.Count > 0)
                {
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                    await _hardwareVaultService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                }

                transactionScope.Complete();
            }
        }

        public async Task EditPersonalAccountPwdAsync(Account account, AccountPassword accountPassword)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (accountPassword == null)
            {
                throw new ArgumentNullException(nameof(accountPassword));
            }

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);

            account.UpdatedAt = DateTime.UtcNow;
            account.PasswordUpdatedAt = DateTime.UtcNow;

            // Update password field if there are no vaults
            if (employee.HardwareVaults.Count == 0)
            {
                account.Password = _dataProtectionService.Encrypt(accountPassword.Password);
            }

            // Create tasks if there are vaults
            var tasks = new List<HardwareVaultTask>();
            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountPwdUpdateTask(vault.Id, account.Id, _dataProtectionService.Encrypt(accountPassword.Password)));
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await UpdateAccountsAsync(account);

                if (tasks.Count > 0)
                {
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                    await _hardwareVaultService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                }

                transactionScope.Complete();
            }
        }

        public async Task EditPersonalAccountOtpAsync(Account account, AccountOtp accountOtp)
        {
            if (account == null)
            {
                throw new ArgumentNullException(nameof(account));
            }

            if (accountOtp == null)
            {
                throw new ArgumentNullException(nameof(accountOtp));
            }

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);

            account.UpdatedAt = DateTime.UtcNow;
            account.OtpUpdatedAt = Validation.VerifyOtpSecret(accountOtp.OtpSecret) == null ? null : (DateTime?)DateTime.UtcNow;

            // Update otp field if there are no vaults
            if (employee.HardwareVaults.Count == 0)
                account.OtpSecret = _dataProtectionService.Encrypt(accountOtp.OtpSecret);

            // Create tasks if there are vaults
            var tasks = new List<HardwareVaultTask>();
            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountOtpUpdateTask(vault.Id, account.Id, _dataProtectionService.Encrypt(accountOtp.OtpSecret ?? string.Empty)));
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await UpdateAccountsAsync(account);

                if (tasks.Count > 0)
                {
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                    await _hardwareVaultService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                }

                transactionScope.Complete();
            }
        }

        public async Task UpdateAfterAccountCreateAsync(Account account, uint timestamp)
        {
            account.Timestamp = timestamp;
            account.Password = null;
            account.OtpSecret = null;
            account.UpdateInActiveDirectory = false;
            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAfterAccountModifyAsync(Account account, uint timestamp)
        {
            account.Timestamp = timestamp;
            _dbContext.Accounts.Update(account);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateAccountsAsync(params Account[] accounts)
        {
            _dbContext.Accounts.UpdateRange(accounts);
            await _dbContext.SaveChangesAsync();
        }

        public void UnchangedPersonalAccount(Account account)
        {
            _dbContext.Unchanged(account);
        }

        public async Task<Account> AddSharedAccountAsync(string employeeId, string sharedAccountId)
        {
            if (string.IsNullOrWhiteSpace(employeeId))
            {
                throw new ArgumentNullException(nameof(employeeId));
            }

            if (string.IsNullOrWhiteSpace(sharedAccountId))
            {
                throw new ArgumentNullException(nameof(sharedAccountId));
            }

            _dataProtectionService.Validate();

            var sharedAccount = await _sharedAccountService.GetSharedAccountByIdAsync(sharedAccountId);
            if (sharedAccount == null)
            {
                throw new HESException(HESCode.SharedAccountNotFound);
            }

            var accountExist = await _dbContext.ExistAsync<Account>(x => x.EmployeeId == employeeId &&
                                                         x.Name == sharedAccount.Name &&
                                                         x.Login == sharedAccount.Login &&
                                                         x.Deleted == false);
            if (accountExist)
            {
                throw new HESException(HESCode.SharedAccountExist);
            }

            var account = new Account
            {
                Id = Guid.NewGuid().ToString(),
                Name = sharedAccount.Name,
                Urls = sharedAccount.Urls,
                Apps = sharedAccount.Apps,
                Login = sharedAccount.Login,
                AccountType = AccountType.Shared,
                LoginType = sharedAccount.LoginType,
                CreatedAt = DateTime.UtcNow,
                PasswordUpdatedAt = DateTime.UtcNow,
                OtpUpdatedAt = sharedAccount.OtpSecret != null ? new DateTime?(DateTime.UtcNow) : null,
                EmployeeId = employeeId,
                SharedAccountId = sharedAccountId,
                Password = sharedAccount.Password,
                OtpSecret = sharedAccount.OtpSecret,
                StorageId = new StorageId().Data
            };

            var employee = await GetEmployeeByIdAsync(employeeId);
            var tasks = new List<HardwareVaultTask>();

            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountCreateTask(vault.Id, account.Id, sharedAccount.Password, sharedAccount.OtpSecret));
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                _dbContext.Accounts.Add(account);
                await _dbContext.SaveChangesAsync();
                await SetAsPrimaryAccountIfEmptyAsync(account.EmployeeId, account.Id);

                if (tasks.Count > 0)
                {
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                    await _hardwareVaultService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                }

                transactionScope.Complete();
            }

            return account;
        }

        public async Task<Account> DeleteAccountAsync(string accountId)
        {
            if (string.IsNullOrWhiteSpace(accountId))
            {
                throw new ArgumentNullException(nameof(accountId));
            }

            _dataProtectionService.Validate();

            var account = await GetAccountByIdAsync(accountId);
            if (account == null)
            {
                throw new HESException(HESCode.AccountNotFound);
            }

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);

            var isPrimary = employee.PrimaryAccountId == accountId;
            if (isPrimary)
            {
                employee.PrimaryAccountId = null;
            }

            account.Deleted = true;
            account.UpdatedAt = DateTime.UtcNow;
            account.Password = null;
            account.OtpSecret = null;

            var tasks = new List<HardwareVaultTask>();
            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountDeleteTask(vault.Id, account.Id));
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (isPrimary)
                {
                    await UpdateEmployeeInDatabase(employee);
                }

                await UpdateAccountsAsync(account);
                await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                await _hardwareVaultService.UpdateNeedSyncAsync(employee.HardwareVaults, true);

                transactionScope.Complete();
            }

            return account;
        }

        public async Task DeleteAccountsByEmployeeIdAsync(string employeeId)
        {
            var accounts = await _dbContext.Accounts
                   .Where(x => x.EmployeeId == employeeId && x.Deleted == false)
                   .ToListAsync();

            foreach (var account in accounts)
            {
                account.Deleted = true;
            }

            _dbContext.Accounts.UpdateRange(accounts);
            await _dbContext.SaveChangesAsync();
        }

        private string GenerateMasterPassword()
        {
            var buf = AesCryptoHelper.CreateRandomBuf(32);
            for (int i = 0; i < 32; i++)
            {
                if (buf[i] == 0)
                {
                    buf[i] = 0xff;
                }
            }

            return ConvertUtils.ByteArrayToHexString(buf);
        }

        #endregion
    }
}