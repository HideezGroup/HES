using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Accounts;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.SharedAccounts;
using HES.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class SharedAccountService : ISharedAccountService, IDisposable
    {
        private readonly IAsyncRepository<SharedAccount> _sharedAccountRepository;
        private readonly IAccountService _accountService;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly IDataProtectionService _dataProtectionService;

        public SharedAccountService(IAsyncRepository<SharedAccount> sharedAccountRepository,
                                    IAccountService accountService,
                                    IHardwareVaultTaskService hardwareVaultTaskService,
                                    IDataProtectionService dataProtectionService)
        {
            _sharedAccountRepository = sharedAccountRepository;
            _accountService = accountService;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _dataProtectionService = dataProtectionService;
        }

        public IQueryable<SharedAccount> Query()
        {
            return _sharedAccountRepository.Query();
        }

        public async Task UnchangedAsync(SharedAccount account)
        {
            await _sharedAccountRepository.UnchangedAsync(account);
        }

        public async Task<SharedAccount> GetSharedAccountByIdAsync(string id)
        {
            return await _sharedAccountRepository.GetByIdAsync(id);
        }

        public async Task<List<SharedAccount>> GetSharedAccountsAsync(DataLoadingOptions<SharedAccountsFilter> dataLoadingOptions)
        {
            var query = _sharedAccountRepository
                .Query()
                .Where(d => d.Deleted == false);

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Name != null)
                {
                    query = query.Where(w => w.Name.Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Urls != null)
                {
                    query = query.Where(w => w.Urls.Contains(dataLoadingOptions.Filter.Urls, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Apps != null)
                {
                    query = query.Where(w => w.Apps.Contains(dataLoadingOptions.Filter.Apps, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Login != null)
                {
                    query = query.Where(w => w.Login.Contains(dataLoadingOptions.Filter.Login, StringComparison.OrdinalIgnoreCase));
                }
            }

            //Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Urls.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Apps.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Login.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            //Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(SharedAccount.Name):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                    break;
                case nameof(SharedAccount.Urls):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Urls) : query.OrderByDescending(x => x.Urls);
                    break;
                case nameof(SharedAccount.Apps):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Apps) : query.OrderByDescending(x => x.Apps);
                    break;
                case nameof(SharedAccount.Login):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Login) : query.OrderByDescending(x => x.Login);
                    break;
                case nameof(SharedAccount.PasswordChangedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.PasswordChangedAt) : query.OrderByDescending(x => x.PasswordChangedAt);
                    break;
                case nameof(SharedAccount.OtpSecretChangedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.OtpSecretChangedAt) : query.OrderByDescending(x => x.OtpSecretChangedAt);
                    break;
            }

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetSharedAccountsCountAsync(DataLoadingOptions<SharedAccountsFilter> dataLoadingOptions)
        {
            var query = _sharedAccountRepository
                .Query()
                .Where(d => d.Deleted == false);

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Name != null)
                {
                    query = query.Where(w => w.Name.Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Urls != null)
                {
                    query = query.Where(w => w.Urls.Contains(dataLoadingOptions.Filter.Urls, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Apps != null)
                {
                    query = query.Where(w => w.Apps.Contains(dataLoadingOptions.Filter.Apps, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Login != null)
                {
                    query = query.Where(w => w.Login.Contains(dataLoadingOptions.Filter.Login, StringComparison.OrdinalIgnoreCase));
                }
            }

            //Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Urls.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Apps.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Login.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public async Task<List<SharedAccount>> GetAllSharedAccountsAsync()
        {
            return await _sharedAccountRepository
                .Query()
                .Where(d => d.Deleted == false)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<SharedAccount> CreateSharedAccountAsync(SharedAccountAddModel sharedAccountModel)
        {
            if (sharedAccountModel == null)
                throw new ArgumentNullException(nameof(sharedAccountModel));

            _dataProtectionService.Validate();

            var sharedAccount = new SharedAccount()
            {
                Name = sharedAccountModel.Name,
                Urls = Validation.VerifyUrls(sharedAccountModel.Urls),
                Apps = sharedAccountModel.Apps,
                Login = await ValidateAccountNameAndLoginAsync(sharedAccountModel.Name, sharedAccountModel.GetLogin()),
                LoginType = sharedAccountModel.LoginType,
                Password = _dataProtectionService.Encrypt(sharedAccountModel.Password),
                PasswordChangedAt = DateTime.UtcNow
            };

            if (!string.IsNullOrWhiteSpace(sharedAccountModel.OtpSecret))
            {
                Validation.VerifyOtpSecret(sharedAccountModel.OtpSecret);
                sharedAccount.OtpSecret = _dataProtectionService.Encrypt(sharedAccountModel.OtpSecret);
                sharedAccount.OtpSecretChangedAt = DateTime.UtcNow;
            }

            return await _sharedAccountRepository.AddAsync(sharedAccount);
        }

        private async Task<string> ValidateAccountNameAndLoginAsync(string name, string login, string accountId = null)
        {
            var query = _sharedAccountRepository.Query().Where(x => x.Name == name && x.Login == login && x.Deleted == false);

            if (accountId != null)
                query = query.Where(x => x.Id != accountId);

            var exist = await query.AnyAsync();
            if (exist)
                throw new HESException(HESCode.SharedAccountExist);

            return login;
        }

        public async Task<List<string>> EditSharedAccountAsync(SharedAccountEditModel sharedAccountModel)
        {
            if (sharedAccountModel == null)
                throw new ArgumentNullException(nameof(sharedAccountModel));

            _dataProtectionService.Validate();
            await ValidateAccountNameAndLoginAsync(sharedAccountModel.Name, sharedAccountModel.GetLogin(), sharedAccountModel.Id);
            sharedAccountModel.Urls = Validation.VerifyUrls(sharedAccountModel.Urls);

            var sharedAccount = await GetSharedAccountByIdAsync(sharedAccountModel.Id);
            if (sharedAccount == null)
                throw new HESException(HESCode.SharedAccountNotFound);

            sharedAccount = sharedAccountModel.SetNewValue(sharedAccount);

            // Get all accounts where used this shared account
            var accounts = await _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Where(x => x.SharedAccountId == sharedAccount.Id && x.Deleted == false)
                .AsNoTracking()
                .ToListAsync();

            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();

            foreach (var account in accounts)
            {
                account.Name = sharedAccount.Name;
                account.Urls = sharedAccount.Urls;
                account.Apps = sharedAccount.Apps;
                account.Login = sharedAccount.Login;
                account.UpdatedAt = DateTime.UtcNow;

                foreach (var hardwareVault in account.Employee.HardwareVaults)
                {
                    tasks.Add(_hardwareVaultTaskService.GetAccountUpdateTask(hardwareVault.Id, account.Id));
                }
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { nameof(SharedAccount.Name), nameof(SharedAccount.Urls), nameof(SharedAccount.Apps), nameof(SharedAccount.Login) });
                await _accountService.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.Name), nameof(Account.Urls), nameof(Account.Apps), nameof(Account.Login), nameof(Account.UpdatedAt) });
                await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                transactionScope.Complete();
            }

            return accounts.SelectMany(x => x.Employee.HardwareVaults.Select(s => s.Id)).ToList();
        }

        public async Task<List<string>> EditSharedAccountPwdAsync(SharedAccount sharedAccount, AccountPassword accountPassword)
        {
            if (sharedAccount == null)
                throw new ArgumentNullException(nameof(sharedAccount));

            if (accountPassword == null)
                throw new ArgumentNullException(nameof(accountPassword));

            _dataProtectionService.Validate();

            // Update Shared Account
            sharedAccount.Password = _dataProtectionService.Encrypt(accountPassword.Password);
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;

            // Get all accounts where used this shared account
            var accounts = await _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Where(x => x.SharedAccountId == sharedAccount.Id && x.Deleted == false)
                .AsNoTracking()
                .ToListAsync();

            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();

            foreach (var account in accounts)
            {
                account.UpdatedAt = DateTime.UtcNow;
                account.PasswordUpdatedAt = DateTime.UtcNow;

                foreach (var hardwareVault in account.Employee.HardwareVaults)
                {
                    tasks.Add(_hardwareVaultTaskService.GetAccountPwdUpdateTask(hardwareVault.Id, account.Id, sharedAccount.Password));
                }
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { nameof(SharedAccount.Password), nameof(SharedAccount.PasswordChangedAt) });
                await _accountService.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.UpdatedAt), nameof(Account.PasswordUpdatedAt) });
                await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                transactionScope.Complete();
            }

            return accounts.SelectMany(x => x.Employee.HardwareVaults.Select(s => s.Id)).ToList();
        }

        public async Task<List<string>> EditSharedAccountOtpAsync(SharedAccount sharedAccount, AccountOtp accountOtp)
        {
            if (sharedAccount == null)
                throw new ArgumentNullException(nameof(sharedAccount));
            if (accountOtp == null)
                throw new ArgumentNullException(nameof(accountOtp));

            _dataProtectionService.Validate();

            Validation.VerifyOtpSecret(accountOtp.OtpSecret);

            // Update Shared Account
            sharedAccount.OtpSecret = !string.IsNullOrWhiteSpace(accountOtp.OtpSecret) ? _dataProtectionService.Encrypt(accountOtp.OtpSecret) : null;
            sharedAccount.OtpSecretChangedAt = !string.IsNullOrWhiteSpace(accountOtp.OtpSecret) ? new DateTime?(DateTime.UtcNow) : null;

            // Get all accounts where used this shared account
            var accounts = await _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Where(x => x.SharedAccountId == sharedAccount.Id && x.Deleted == false)
                .AsNoTracking()
                .ToListAsync();

            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();

            foreach (var account in accounts)
            {
                account.UpdatedAt = DateTime.UtcNow;
                account.OtpUpdatedAt = sharedAccount.OtpSecretChangedAt;

                foreach (var vault in account.Employee.HardwareVaults)
                {
                    tasks.Add(_hardwareVaultTaskService.GetAccountOtpUpdateTask(vault.Id, account.Id, sharedAccount.OtpSecret));
                }
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { nameof(SharedAccount.OtpSecret), nameof(SharedAccount.OtpSecretChangedAt) });
                await _accountService.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.UpdatedAt), nameof(Account.OtpUpdatedAt) });
                await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                transactionScope.Complete();
            }

            return accounts.SelectMany(x => x.Employee.HardwareVaults.Select(s => s.Id)).ToList();
        }

        public async Task<List<string>> DeleteSharedAccountAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            _dataProtectionService.Validate();

            var sharedAccount = await _sharedAccountRepository.GetByIdAsync(id);
            if (sharedAccount == null)
                throw new Exception("Shared Account not found");

            sharedAccount.Deleted = true;

            // Get all accounts where used this shared account
            var accounts = await _accountService
                .Query()
                .Include(x => x.Employee.HardwareVaults)
                .Where(x => x.SharedAccountId == sharedAccount.Id && x.Deleted == false)
                .AsNoTracking()
                .ToListAsync();

            List<HardwareVaultTask> tasks = new List<HardwareVaultTask>();

            foreach (var account in accounts)
            {
                account.Deleted = true;
                account.UpdatedAt = DateTime.UtcNow;

                foreach (var hardwareVault in account.Employee.HardwareVaults)
                {
                    tasks.Add(_hardwareVaultTaskService.GetAccountDeleteTask(hardwareVault.Id, account.Id));
                }
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { nameof(SharedAccount.Deleted) });
                await _accountService.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.Deleted), nameof(Account.UpdatedAt) });
                await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                transactionScope.Complete();
            }

            return accounts.SelectMany(x => x.Employee.HardwareVaults.Select(s => s.Id)).ToList();
        }

        public void Dispose()
        {
            _sharedAccountRepository.Dispose();
            _accountService.Dispose();
            _hardwareVaultTaskService.Dispose();
        }
    }
}