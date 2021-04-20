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
    public class SharedAccountService : ISharedAccountService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly IDataProtectionService _dataProtectionService;

        public SharedAccountService(IApplicationDbContext dbContext,
                                    IHardwareVaultTaskService hardwareVaultTaskService,
                                    IDataProtectionService dataProtectionService)
        {
            _dbContext = dbContext;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _dataProtectionService = dataProtectionService;
        }

        public void Unchanged(SharedAccount account)
        {
            _dbContext.Unchanged(account);
        }

        public async Task<SharedAccount> GetSharedAccountByIdAsync(string sharedAccountId)
        {
            return await _dbContext.SharedAccounts.FindAsync(sharedAccountId);
        }

        public async Task<List<SharedAccount>> GetSharedAccountsAsync(DataLoadingOptions<SharedAccountsFilter> dataLoadingOptions)
        {
            return await SharedAccountQuery(dataLoadingOptions).Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetSharedAccountsCountAsync(DataLoadingOptions<SharedAccountsFilter> dataLoadingOptions)
        {
            return await SharedAccountQuery(dataLoadingOptions).CountAsync();
        }

        private IQueryable<SharedAccount> SharedAccountQuery(DataLoadingOptions<SharedAccountsFilter> dataLoadingOptions)
        {
            var query = _dbContext.SharedAccounts
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

            return query;
        }

        public async Task<List<SharedAccount>> GetAllSharedAccountsAsync()
        {
            return await _dbContext.SharedAccounts
                .Where(d => d.Deleted == false)
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<SharedAccount> CreateSharedAccountAsync(SharedAccountAddModel sharedAccountModel)
        {
            if (sharedAccountModel == null)
            {
                throw new ArgumentNullException(nameof(sharedAccountModel));
            }

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

            var result = _dbContext.SharedAccounts.Add(sharedAccount);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        private async Task<string> ValidateAccountNameAndLoginAsync(string name, string login, string accountId = null)
        {
            var query = _dbContext.SharedAccounts.Where(x => x.Name == name && x.Login == login && x.Deleted == false);

            if (!string.IsNullOrWhiteSpace(accountId))
            {
                query = query.Where(x => x.Id != accountId);
            }

            var exist = await query.AnyAsync();
            if (exist)
            {
                throw new HESException(HESCode.SharedAccountExist);
            }

            return login;
        }

        public async Task<List<string>> EditSharedAccountAsync(SharedAccountEditModel sharedAccountModel)
        {
            if (sharedAccountModel == null)
            {
                throw new ArgumentNullException(nameof(sharedAccountModel));
            }

            _dataProtectionService.Validate();

            await ValidateAccountNameAndLoginAsync(sharedAccountModel.Name, sharedAccountModel.GetLogin(), sharedAccountModel.Id);
            sharedAccountModel.Urls = Validation.VerifyUrls(sharedAccountModel.Urls);

            var sharedAccount = await GetSharedAccountByIdAsync(sharedAccountModel.Id);
            if (sharedAccount == null)
            {
                throw new HESException(HESCode.SharedAccountNotFound);
            }

            sharedAccount = sharedAccountModel.SetNewValue(sharedAccount);

            // Get all accounts where used this shared account
            var accounts = await GetAccountsBySharedAccountIdAsync(sharedAccount.Id);

            var tasks = new List<HardwareVaultTask>();
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
                _dbContext.SharedAccounts.UpdateRange(sharedAccount);
                await _dbContext.SaveChangesAsync();

                await UpdateAccountsAsync(accounts);
                await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                transactionScope.Complete();
            }

            return accounts.SelectMany(x => x.Employee.HardwareVaults.Select(s => s.Id)).ToList();
        }

        public async Task<List<string>> EditSharedAccountPwdAsync(SharedAccount sharedAccount, AccountPassword accountPassword)
        {
            if (sharedAccount == null)
            {
                throw new ArgumentNullException(nameof(sharedAccount));
            }

            if (accountPassword == null)
            {
                throw new ArgumentNullException(nameof(accountPassword));
            }

            _dataProtectionService.Validate();

            // Update Shared Account
            sharedAccount.Password = _dataProtectionService.Encrypt(accountPassword.Password);
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;

            // Get all accounts where used this shared account
            var accounts = await GetAccountsBySharedAccountIdAsync(sharedAccount.Id);

            var tasks = new List<HardwareVaultTask>();
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
                _dbContext.SharedAccounts.UpdateRange(sharedAccount);
                await _dbContext.SaveChangesAsync();

                await UpdateAccountsAsync(accounts);
                await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                transactionScope.Complete();
            }

            return accounts.SelectMany(x => x.Employee.HardwareVaults.Select(s => s.Id)).ToList();
        }

        public async Task<List<string>> EditSharedAccountOtpAsync(SharedAccount sharedAccount, AccountOtp accountOtp)
        {
            if (sharedAccount == null)
            {
                throw new ArgumentNullException(nameof(sharedAccount));
            }
            
            if (accountOtp == null)
            {
                throw new ArgumentNullException(nameof(accountOtp));
            }

            _dataProtectionService.Validate();

            Validation.VerifyOtpSecret(accountOtp.OtpSecret);

            // Update Shared Account
            sharedAccount.OtpSecret = !string.IsNullOrWhiteSpace(accountOtp.OtpSecret) ? _dataProtectionService.Encrypt(accountOtp.OtpSecret) : null;
            sharedAccount.OtpSecretChangedAt = !string.IsNullOrWhiteSpace(accountOtp.OtpSecret) ? new DateTime?(DateTime.UtcNow) : null;

            // Get all accounts where used this shared account
            var accounts = await GetAccountsBySharedAccountIdAsync(sharedAccount.Id);

            var tasks = new List<HardwareVaultTask>();
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
                _dbContext.SharedAccounts.UpdateRange(sharedAccount);
                await _dbContext.SaveChangesAsync();

                await UpdateAccountsAsync(accounts);
                await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                transactionScope.Complete();
            }

            return accounts.SelectMany(x => x.Employee.HardwareVaults.Select(s => s.Id)).ToList();
        }

        public async Task<List<string>> DeleteSharedAccountAsync(string sharedAccountId)
        {
            if (string.IsNullOrWhiteSpace(sharedAccountId))
            {
                throw new ArgumentNullException(nameof(sharedAccountId));
            }

            _dataProtectionService.Validate();

            var sharedAccount = await GetSharedAccountByIdAsync(sharedAccountId);
            if (sharedAccount == null)
            {
                throw new HESException(HESCode.SharedAccountNotFound);
            }

            sharedAccount.Deleted = true;

            // Get all accounts where used this shared account
            var accounts = await GetAccountsBySharedAccountIdAsync(sharedAccount.Id);

            var tasks = new List<HardwareVaultTask>();
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
                _dbContext.SharedAccounts.UpdateRange(sharedAccount);
                await _dbContext.SaveChangesAsync();

                await UpdateAccountsAsync(accounts);
                await _hardwareVaultTaskService.AddRangeTasksAsync(tasks);
                transactionScope.Complete();
            }

            return accounts.SelectMany(x => x.Employee.HardwareVaults.Select(s => s.Id)).ToList();
        }

        private async Task<List<Account>> GetAccountsBySharedAccountIdAsync(string sharedAccountId)
        {
            return await _dbContext.Accounts
                .Include(x => x.Employee.HardwareVaults)
                .Where(x => x.SharedAccountId == sharedAccountId && x.Deleted == false)
                .AsNoTracking()
                .ToListAsync();
        }

        private async Task UpdateAccountsAsync(List<Account> accounts)
        {
            _dbContext.Accounts.UpdateRange(accounts);
            await _dbContext.SaveChangesAsync();
        }
    }
}