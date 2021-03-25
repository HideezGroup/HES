using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Employees;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Accounts;
using HES.Core.Models.Web.AppUsers;
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

namespace HES.Core.Services
{
    public class EmployeeService : IEmployeeService, IDisposable
    {
        private readonly IAsyncRepository<Employee> _employeeRepository;
        private readonly IAsyncRepository<ApplicationUser> _applicationUserRepository;
        private readonly IHardwareVaultService _hardwareVaultService;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly ISoftwareVaultService _softwareVaultService;
        private readonly IAccountService _accountService;
        private readonly ISharedAccountService _sharedAccountService;
        private readonly IWorkstationService _workstationService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFido2Service _fido2Service;
        private readonly IConfiguration _configuration;

        public EmployeeService(IAsyncRepository<Employee> employeeRepository,
                               IAsyncRepository<ApplicationUser> applicationUserRepository,
                               IHardwareVaultService hardwareVaultService,
                               IHardwareVaultTaskService hardwareVaultTaskService,
                               ISoftwareVaultService softwareVaultService,
                               IAccountService accountService,
                               ISharedAccountService sharedAccountService,
                               IWorkstationService workstationService,
                               IDataProtectionService dataProtectionService,
                               UserManager<ApplicationUser> userManager,
                               IFido2Service fido2Service,
                               IConfiguration configuration)
        {
            _employeeRepository = employeeRepository;
            _applicationUserRepository = applicationUserRepository;
            _hardwareVaultService = hardwareVaultService;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _softwareVaultService = softwareVaultService;
            _accountService = accountService;
            _sharedAccountService = sharedAccountService;
            _workstationService = workstationService;
            _dataProtectionService = dataProtectionService;
            _userManager = userManager;
            _fido2Service = fido2Service;
            _configuration = configuration;
        }

        #region Employee

        public IQueryable<Employee> EmployeeQuery()
        {
            return _employeeRepository.Query();
        }

        public async Task<Employee> GetEmployeeByIdAsync(string id, bool asNoTracking = false, bool byActiveDirectoryGuid = false)
        {
            var query = _employeeRepository
                .Query()
                .Include(e => e.Department.Company)
                .Include(e => e.Position)
                .Include(e => e.SoftwareVaults)
                .Include(e => e.SoftwareVaultInvitations)
                .Include(e => e.HardwareVaults)
                .ThenInclude(e => e.HardwareVaultProfile)
                .Include(e => e.Accounts)
                .AsQueryable();

            if (asNoTracking)
                query = query.AsNoTracking();

            if (byActiveDirectoryGuid)
                return await query.FirstOrDefaultAsync(e => e.ActiveDirectoryGuid == id);

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public Task UnchangedEmployeeAsync(Employee employee)
        {
            return _employeeRepository.UnchangedAsync(employee);
        }

        public async Task<List<Employee>> GetEmployeesAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions)
        {
            var query = _employeeRepository
                .Query()
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

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetEmployeesCountAsync(DataLoadingOptions<EmployeeFilter> dataLoadingOptions)
        {
            var query = _employeeRepository
            .Query()
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

            return await query.CountAsync();
        }

        public async Task<IList<string>> GetEmployeeVaultIdsAsync(string employeeId)
        {
            var employee = await GetEmployeeByIdAsync(employeeId);
            return employee.HardwareVaults.Select(x => x.Id).ToList();
        }

        public async Task<Employee> CreateEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // If the field is NULL then the unique check does not work; therefore, we write empty
            employee.LastName = employee.LastName ?? string.Empty;

            employee.DepartmentId = string.IsNullOrWhiteSpace(employee.DepartmentId) ? null : employee.DepartmentId;
            employee.PositionId = string.IsNullOrWhiteSpace(employee.PositionId) ? null : employee.PositionId;

            var exist = await _employeeRepository.ExistAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName);
            if (exist)
                throw new AlreadyExistException($"{employee.FirstName} {employee.LastName} already exists.");

            return await _employeeRepository.AddAsync(employee);
        }

        public async Task<Employee> ImportEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // If the field is NULL then the unique check does not work, therefore we write empty field
            employee.LastName = employee.LastName ?? string.Empty;

            var employeeByGuid = await _employeeRepository.Query().FirstOrDefaultAsync(x => x.ActiveDirectoryGuid == employee.ActiveDirectoryGuid);
            if (employeeByGuid != null)
            {
                return employeeByGuid;
            }

            var employeeByName = await _employeeRepository.Query().FirstOrDefaultAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName);
            if (employeeByName != null)
            {
                employeeByName.ActiveDirectoryGuid = employee.ActiveDirectoryGuid;
                return await _employeeRepository.UpdateAsync(employeeByName);
            }

            return await _employeeRepository.AddAsync(employee);
        }

        public async Task EditEmployeeAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // If the field is NULL then the unique check does not work; therefore, we write empty
            employee.LastName = employee.LastName ?? string.Empty;

            employee.DepartmentId = string.IsNullOrWhiteSpace(employee.DepartmentId) ? null : employee.DepartmentId;
            employee.PositionId = string.IsNullOrWhiteSpace(employee.PositionId) ? null : employee.PositionId;

            var exist = await _employeeRepository.ExistAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName && x.Id != employee.Id);
            if (exist)
            {
                throw new AlreadyExistException($"{employee.FirstName} {employee.LastName} already exists.");
            }

            var properties = new string[]
            {
                nameof(Employee.ActiveDirectoryGuid),
                nameof(Employee.FirstName),
                nameof(Employee.LastName),
                nameof(Employee.Email),
                nameof(Employee.PhoneNumber),
                nameof(Employee.DepartmentId),
                nameof(Employee.PositionId),
                nameof(Employee.WhenChanged),
            };

            await _employeeRepository.UpdateOnlyPropAsync(employee, properties);
        }

        public async Task DeleteEmployeeAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var employee = await _employeeRepository.GetByIdAsync(id);

            if (employee == null)
                throw new Exception("Employee not found");

            var hardwareVaults = await _hardwareVaultService
                .VaultQuery()
                .Where(x => x.EmployeeId == id)
                .AnyAsync();

            if (hardwareVaults)
                throw new Exception("First untie the hardware vault before removing.");

            var softwareVaults = await _softwareVaultService
                .SoftwareVaultQuery()
                .Where(x => x.EmployeeId == id)
                .AnyAsync();

            if (softwareVaults)
                throw new Exception("First untie the software vault before removing.");

            await _employeeRepository.DeleteAsync(employee);
        }

        public async Task UpdateLastSeenAsync(string vaultId)
        {
            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);
            if (vault?.EmployeeId == null)
                return;

            var employee = await _employeeRepository.GetByIdAsync(vault.EmployeeId);
            if (employee == null)
                return;

            employee.LastSeen = DateTime.UtcNow;
            await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { nameof(Employee.LastSeen) });
        }

        public async Task<bool> CheckEmployeeNameExistAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            // If the field is NULL then the unique check does not work; therefore, we write empty
            employee.LastName = employee.LastName ?? string.Empty;
            return await _employeeRepository.ExistAsync(x => x.FirstName == employee.FirstName && x.LastName == employee.LastName);
        }

        public async Task RemoveFromHideezKeyOwnersAsync(string employeeId)
        {
            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            var employee = await GetEmployeeByIdAsync(employeeId);

            var vaultsIds = employee.HardwareVaults.Select(x => x.Id).ToList();

            foreach (var vaultId in vaultsIds)
            {
                await RemoveHardwareVaultAsync(vaultId, VaultStatusReason.Withdrawal);
            }

            employee.ActiveDirectoryGuid = null;
            employee.WhenChanged = null;
            await _employeeRepository.UpdateAsync(employee);
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
                throw new ArgumentNullException(nameof(employee));

            var user = await _applicationUserRepository
                .Query()
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
                IsSsoEnabled = user != null ? true : false,
                UserEmail = user.Email,
                UserRole = user.UserRoles.FirstOrDefault().Role.Name,
                SecurityKeyName = cred.Count > 0 ? "Added" : "Not added"
            };
        }

        public async Task EnableSsoAsync(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

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
                throw new ArgumentNullException(nameof(employee));

            var user = await _userManager.FindByEmailAsync(employee.Email);
            if (user == null)
                throw new HESException(HESCode.UserNotFound);

            await _fido2Service.RemoveCredentialsByUsername(employee.Email);
            await _userManager.DeleteAsync(user);
        }

        #endregion

        #region Hardware Vault

        public async Task AddHardwareVaultAsync(string employeeId, string vaultId)
        {
            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(employeeId);
            if (employee == null)
                throw new Exception("Employee not found");

            if (employee.HardwareVaults.Count > 0)
                throw new Exception("Cannot add more than one hardware vault.");

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vault} not found");

            if (vault.Status != VaultStatus.Ready)
                throw new Exception($"Vault {vaultId} in a status that does not allow to reserve.");

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
                vault.NeedSync = true;

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
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            _dataProtectionService.Validate();

            var vault = await _hardwareVaultService.GetVaultByIdAsync(vaultId);

            if (vault == null)
                throw new Exception($"Vault {vaultId} not found.");

            if (vault.Status != VaultStatus.Reserved && vault.Status != VaultStatus.Active &&
                vault.Status != VaultStatus.Locked && vault.Status != VaultStatus.Suspended)
            {
                throw new Exception($"Vault {vaultId} in a status that does not allow to remove.");
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultTaskService.DeleteTasksByVaultIdAsync(vaultId);
                await _workstationService.DeleteProximityByVaultIdAsync(vaultId);

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
                        await _accountService.DeleteAccountsByEmployeeIdAsync(vault.EmployeeId);
                    }

                    vault.StatusReason = reason;
                    await _hardwareVaultService.SetDeactivatedStatusAsync(vault);
                }

                transactionScope.Complete();
            }
        }

        #endregion

        #region Accounts

        public async Task<Account> GetAccountByIdAsync(string accountId)
        {
            return await _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .FirstOrDefaultAsync(x => x.Id == accountId);
        }

        public async Task<List<Account>> GetAccountsAsync(DataLoadingOptions<AccountFilter> dataLoadingOptions)
        {
            var query = _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .Where(x => x.EmployeeId == dataLoadingOptions.EntityId && x.Deleted == false)
                .AsQueryable();

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

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetAccountsCountAsync(DataLoadingOptions<AccountFilter> dataLoadingOptions)
        {
            var query = _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .Where(x => x.EmployeeId == dataLoadingOptions.EntityId && x.Deleted == false)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x =>
                                    x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Urls.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Apps.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Login.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public async Task<List<Account>> GetAccountsByEmployeeIdAsync(string employeeId)
        {
            return await _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Include(x => x.SharedAccount)
                .Where(x => x.EmployeeId == employeeId && x.Deleted == false)
                .ToListAsync();
        }

        public async Task<Account> CreatePersonalAccountAsync(AccountAddModel personalAccount)
        {
            if (personalAccount == null)
                throw new ArgumentNullException(nameof(personalAccount));

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

            Employee employee = await GetEmployeeByIdAsync(personalAccount.EmployeeId);
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();

            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountCreateTask(vault.Id, account.Id, account.Password, account.OtpSecret));
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.AddAsync(account);
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
            var query = _accountService.Query().Where(x => x.EmployeeId == employeeId && x.Name == name && x.Login == login && x.Deleted == false);

            if (accountId != null)
                query = query.Where(x => x.Id != accountId);

            var exist = await query.AnyAsync();

            if (exist)
                throw new HESException(HESCode.AccountExist);

            return login;
        }

        public async Task SetAsPrimaryAccountAsync(string employeeId, string accountId)
        {
            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            if (accountId == null)
                throw new ArgumentNullException(nameof(accountId));

            var employee = await GetEmployeeByIdAsync(employeeId);
            if (employee == null)
                throw new Exception($"Employee not found");

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                employee.PrimaryAccountId = accountId;
                await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { nameof(Employee.PrimaryAccountId) });

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
            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee.PrimaryAccountId == null)
            {
                employee.PrimaryAccountId = accountId;
                await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { nameof(Employee.PrimaryAccountId) });
            }
        }

        private async Task RemovePrimaryAccountIdAsync(string employeeId)
        {
            var employee = await _employeeRepository.GetByIdAsync(employeeId);

            if (employee == null)
                throw new Exception("Employee not found");

            employee.PrimaryAccountId = null;

            await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { nameof(Employee.PrimaryAccountId) });
        }

        public async Task EditPersonalAccountAsync(AccountEditModel personalAccount)
        {
            if (personalAccount == null)
                throw new ArgumentNullException(nameof(personalAccount));

            _dataProtectionService.Validate();
            await ValidateAccountNameAndLoginAsync(personalAccount.EmployeeId, personalAccount.Name, personalAccount.GetLogin(), personalAccount.Id);
            personalAccount.Urls = Validation.VerifyUrls(personalAccount.Urls);

            var employee = await GetEmployeeByIdAsync(personalAccount.EmployeeId);
            if (employee == null)
                throw new HESException(HESCode.EmployeeNotFound);

            var account = await _accountService.GetAccountByIdAsync(personalAccount.Id);
            if (account == null)
                throw new HESException(HESCode.AccountNotFound);

            account = personalAccount.SetNewValue(account);

            // Create tasks if there are vaults
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();
            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountUpdateTask(vault.Id, personalAccount.Id));
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.Name), nameof(Account.Login), nameof(Account.Urls), nameof(Account.Apps), nameof(Account.UpdatedAt) });

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
                throw new ArgumentNullException(nameof(account));

            if (accountPassword == null)
                throw new ArgumentNullException(nameof(accountPassword));

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);

            account.UpdatedAt = DateTime.UtcNow;
            account.PasswordUpdatedAt = DateTime.UtcNow;

            // Update password field if there are no vaults
            if (employee.HardwareVaults.Count == 0)
                account.Password = _dataProtectionService.Encrypt(accountPassword.Password);

            // Create tasks if there are vaults
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();
            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountPwdUpdateTask(vault.Id, account.Id, _dataProtectionService.Encrypt(accountPassword.Password)));
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.UpdatedAt), nameof(Account.PasswordUpdatedAt), nameof(Account.Password) });

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
                throw new ArgumentNullException(nameof(account));

            if (accountOtp == null)
                throw new ArgumentNullException(nameof(accountOtp));

            _dataProtectionService.Validate();

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);

            account.UpdatedAt = DateTime.UtcNow;
            account.OtpUpdatedAt = Validation.VerifyOtpSecret(accountOtp.OtpSecret) == null ? null : (DateTime?)DateTime.UtcNow;

            // Update otp field if there are no vaults
            if (employee.HardwareVaults.Count == 0)
                account.OtpSecret = _dataProtectionService.Encrypt(accountOtp.OtpSecret);

            // Create tasks if there are vaults
            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();
            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountOtpUpdateTask(vault.Id, account.Id, _dataProtectionService.Encrypt(accountOtp.OtpSecret ?? string.Empty)));
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.UpdatedAt), nameof(Account.OtpUpdatedAt), nameof(Account.OtpSecret) });

                if (tasks.Count > 0)
                {
                    await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                    await _hardwareVaultService.UpdateNeedSyncAsync(employee.HardwareVaults, true);
                }

                transactionScope.Complete();
            }
        }

        public Task UnchangedPersonalAccountAsync(Account account)
        {
            return _accountService.UnchangedAsync(account);
        }

        public async Task<Account> AddSharedAccountAsync(string employeeId, string sharedAccountId)
        {
            if (employeeId == null)
                throw new ArgumentNullException(nameof(employeeId));

            if (sharedAccountId == null)
                throw new ArgumentNullException(nameof(sharedAccountId));

            _dataProtectionService.Validate();

            var sharedAccount = await _sharedAccountService.GetSharedAccountByIdAsync(sharedAccountId);
            if (sharedAccount == null)
                throw new Exception("Shared Account not found");

            var exist = await _accountService.ExistAsync(x => x.EmployeeId == employeeId &&
                                                         x.Name == sharedAccount.Name &&
                                                         x.Login == sharedAccount.Login &&
                                                         x.Deleted == false);
            if (exist)
                throw new Exception("An account with the same name and login exists");

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

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _accountService.AddAsync(account);
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
            if (accountId == null)
                throw new ArgumentNullException(nameof(accountId));

            _dataProtectionService.Validate();

            var account = await GetAccountByIdAsync(accountId);
            if (account == null)
                throw new NotFoundException("Account not found");

            var employee = await GetEmployeeByIdAsync(account.EmployeeId);

            var isPrimary = employee.PrimaryAccountId == accountId;
            if (isPrimary)
                employee.PrimaryAccountId = null;

            account.Deleted = true;
            account.UpdatedAt = DateTime.UtcNow;
            account.Password = null;
            account.OtpSecret = null;

            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();

            foreach (var vault in employee.HardwareVaults)
            {
                tasks.Add(_hardwareVaultTaskService.GetAccountDeleteTask(vault.Id, account.Id));
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                if (isPrimary)
                    await _employeeRepository.UpdateOnlyPropAsync(employee, new string[] { nameof(Employee.PrimaryAccountId) });

                await _accountService.UpdateOnlyPropAsync(account, new string[] { nameof(Account.Deleted), nameof(Account.UpdatedAt), nameof(Account.Password), nameof(Account.OtpSecret) });
                await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                await _hardwareVaultService.UpdateNeedSyncAsync(employee.HardwareVaults, true);

                transactionScope.Complete();
            }

            return account;
        }

        private string GenerateMasterPassword()
        {
            var buf = AesCryptoHelper.CreateRandomBuf(32);
            for (int i = 0; i < 32; i++)
            {
                if (buf[i] == 0)
                    buf[i] = 0xff;
            }
            var pass = ConvertUtils.ByteArrayToHexString(buf);
            return pass;
        }

        #endregion

        public void Dispose()
        {
            _employeeRepository.Dispose();
            _hardwareVaultService.Dispose();
            _hardwareVaultTaskService.Dispose();
            _softwareVaultService.Dispose();
            _accountService.Dispose();
            _sharedAccountService.Dispose();
            _workstationService.Dispose();
        }
    }
}