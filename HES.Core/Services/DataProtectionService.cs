using HES.Core.Constants;
using HES.Core.Crypto;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public enum ProtectionStatus
    {
        Off,
        On,
        Busy,
        Activate,
        NotFinishedPasswordChange
    }

    /// <summary>
    /// When protection is enabled, fields are encrypted
    /// Accounts
    ///     - Password
    ///     - OtpSecret    
    /// AppSettings
    ///     - Key: domain - field: Password
    ///     - Key: licensing - field: ApiKey
    /// HardwareVaultsActivations
    ///     - ActivationCode
    /// HardwareVaults   
    ///     - MasterPassword
    /// HardwareVaultTasks
    ///     - Password
    ///     - OtpSecret
    /// SharedAccounts
    ///     - Password
    ///     - OtpSecret
    /// SoftwareVaultInvitations
    ///     - ActivationCode   
    /// </summary>
    public class DataProtectionService : IDataProtectionService
    {
        public IServiceProvider Services { get; }
        private readonly ILogger<DataProtectionService> _logger;

        private DataProtectionKey _key;
        private bool _protectionEnabled;
        private bool _protectionBusy;
        private bool _protectionActivated;
        private bool _notFinishedPasswordChange;

        public DataProtectionService(IServiceProvider services, ILogger<DataProtectionService> logger)
        {
            Services = services;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            _protectionEnabled = false;
            _protectionActivated = false;
            _notFinishedPasswordChange = false;

            try
            {
                var prms = await ReadDataProtectionEntity();

                if (prms != null)
                {
                    _protectionEnabled = true;

                    var password = TryGetStoredPassword();

                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        await ActivateProtectionAsync(password);
                    }
                }
            }
            catch (NotFinishedPasswordChangeException)
            {
                _protectionEnabled = true;
                _notFinishedPasswordChange = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            finally
            {
                if (_protectionEnabled && !_protectionActivated)
                {
                    using var scope = Services.CreateScope();
                    var scopedApplicationUserService = scope.ServiceProvider.GetRequiredService<IApplicationUserService>();
                    await scopedApplicationUserService.SendActivateDataProtectionAsync();
                    scopedApplicationUserService.Dispose();
                }
            }
        }

        public ProtectionStatus Status()
        {
            if (!_protectionEnabled)
                return ProtectionStatus.Off;

            if (_notFinishedPasswordChange)
                return ProtectionStatus.NotFinishedPasswordChange;

            if (!_protectionActivated)
                return ProtectionStatus.Activate;

            return ProtectionStatus.On;
        }

        public void Validate()
        {
            if (_notFinishedPasswordChange)
                throw new Exception("Data protection not finished password change.");

            if (_protectionEnabled && !_protectionActivated)
                throw new Exception("Data protection not activated.");

            if (_protectionBusy)
                throw new Exception("Data protection is busy.");
        }

        public string Encrypt(string plainText)
        {
            if (plainText == null)
                return null;

            if (!_protectionEnabled)
                return plainText;

            Validate();

            return _key.Encrypt(plainText);
        }

        public string Decrypt(string cipherText)
        {
            if (cipherText == null)
                return null;

            if (!_protectionEnabled)
                return cipherText;

            Validate();

            return _key.Decrypt(cipherText);
        }

        public async Task<bool> ActivateProtectionAsync(string password)
        {
            try
            {
                if (_protectionBusy)
                    throw new Exception("Data protection is busy.");
                _protectionBusy = true;

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentNullException(nameof(password));

                if (_protectionActivated)
                    throw new Exception("Data protection is already activated.");

                var dataProtectionEntity = await ReadDataProtectionEntity();
                if (dataProtectionEntity == null)
                    throw new Exception("Data protection parameters not found.");

                _key = new DataProtectionKey(dataProtectionEntity.Id, dataProtectionEntity.Params);

                _protectionActivated = _key.ValidatePassword(password);

                return _protectionActivated;
            }
            finally
            {
                _protectionBusy = false;
            }
        }

        public async Task EnableProtectionAsync(string password)
        {
            try
            {
                if (_protectionBusy)
                    throw new Exception("Data protection is busy.");
                _protectionBusy = true;

                if (string.IsNullOrWhiteSpace(password))
                    throw new ArgumentNullException(nameof(password));

                if (_protectionEnabled)
                    throw new Exception("Data protection is already enabled.");

                var prms = DataProtectionKey.CreateParams(password);

                var dataProtectionEntity = await SaveDataProtectionEntity(prms);

                _key = new DataProtectionKey(dataProtectionEntity.Id, dataProtectionEntity.Params);
                _key.ValidatePassword(password);

                await EncryptDatabase(_key);

                _protectionEnabled = true;
                _protectionActivated = true;
            }
            finally
            {
                _protectionBusy = false;
            }
        }

        public async Task DisableProtectionAsync(string password)
        {
            try
            {
                if (_protectionBusy)
                    throw new Exception("Data protection is busy.");
                _protectionBusy = true;

                if (!_protectionEnabled)
                    throw new Exception("Data protection is not enabled.");

                if (!_protectionActivated)
                    throw new Exception("Data protection is not activated.");


                var dataProtectionEntity = await ReadDataProtectionEntity();
                if (dataProtectionEntity == null)
                    throw new Exception("Data protection parameters not found.");

                var key = new DataProtectionKey(dataProtectionEntity.Id, dataProtectionEntity.Params);

                if (!key.ValidatePassword(password))
                    throw new Exception("Incorrect password");

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await DeleteDataProtectionEntity(key.KeyId);
                    await DecryptDatabase(key);
                    transactionScope.Complete();
                }

                _key = null;
                _protectionActivated = false;
                _protectionEnabled = false;
            }
            finally
            {
                _protectionBusy = false;
            }
        }

        public async Task ChangeProtectionPasswordAsync(string oldPassword, string newPassword)
        {
            try
            {
                if (_protectionBusy)
                    throw new Exception("Data protection is busy.");
                _protectionBusy = true;

                if (!_protectionActivated)
                    throw new Exception("Data protection is not activated");

                var oldDataProtectionEntity = await ReadDataProtectionEntity();
                if (oldDataProtectionEntity == null)
                    throw new Exception("Data protection parameters not found.");

                var oldKey = new DataProtectionKey(oldDataProtectionEntity.Id, oldDataProtectionEntity.Params);

                if (!oldKey.ValidatePassword(oldPassword))
                    throw new Exception("Incorrect old password");

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    // Creating the key for the new password
                    var prms = DataProtectionKey.CreateParams(newPassword);
                    var newDataProtectionEntity = await SaveDataProtectionEntity(prms);
                    var newKey = new DataProtectionKey(newDataProtectionEntity.Id, newDataProtectionEntity.Params);
                    newKey.ValidatePassword(newPassword);

                    await ReencryptDatabase(oldKey, newKey);
                    // Delete old key
                    await DeleteDataProtectionEntity(oldKey.KeyId);
                    // Set new key as a current key
                    _key = newKey;
                    transactionScope.Complete();
                }

                // Set activated if detected the not finished password change operation.
                _protectionActivated = true;
            }
            finally
            {
                _protectionBusy = false;
            }
        }

        private async Task ReencryptDatabase(DataProtectionKey key, DataProtectionKey newKey)
        {

            using var scope = Services.CreateScope();

            // Accounts
            var scopedAccountRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<Account>>();
            var accounts = await scopedAccountRepository.Query().ToListAsync();
            foreach (var account in accounts)
            {
                if (account.Password != null)
                {
                    var plainText = key.Decrypt(account.Password);
                    account.Password = newKey.Encrypt(plainText);
                }
                if (account.OtpSecret != null)
                {
                    var plainText = key.Decrypt(account.OtpSecret);
                    account.OtpSecret = newKey.Encrypt(plainText);
                }
            }

            // AppSettings
            var scopedAppSettingsRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<AppSettings>>();
            var domainSettings = await scopedAppSettingsRepository.GetByIdAsync(ServerConstants.Domain);
            if (domainSettings != null)
            {
                var settings = JsonConvert.DeserializeObject<LdapSettings>(domainSettings.Value);
                var plainText = key.Decrypt(settings.Password);
                settings.Password = newKey.Encrypt(plainText);
                var json = JsonConvert.SerializeObject(settings);
                domainSettings.Value = json;
            }
            var licenseSettings = await scopedAppSettingsRepository.GetByIdAsync(ServerConstants.Licensing);
            if (licenseSettings != null)
            {
                var settings = JsonConvert.DeserializeObject<LicensingSettings>(licenseSettings.Value);
                var plainText = key.Decrypt(settings.ApiKey);
                settings.ApiKey = newKey.Encrypt(plainText);
                var json = JsonConvert.SerializeObject(settings);
                licenseSettings.Value = json;
            }

            // HardwareVaultsActivations
            var scopedHardwareVaultActivationRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVaultActivation>>();
            var hardwareVaultActivations = await scopedHardwareVaultActivationRepository.Query().ToListAsync();
            foreach (var hardwareVaultActivation in hardwareVaultActivations)
            {
                var plainText = key.Decrypt(hardwareVaultActivation.AcivationCode);
                hardwareVaultActivation.AcivationCode = newKey.Encrypt(plainText);
            }

            // HardwareVaults   
            var scopedHardwareVaultRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVault>>();
            var hardwareVaults = await scopedHardwareVaultRepository.Query().ToListAsync();
            foreach (var hardwareVault in hardwareVaults)
            {
                if (hardwareVault.MasterPassword != null)
                {
                    var plainText = key.Decrypt(hardwareVault.MasterPassword);
                    hardwareVault.MasterPassword = newKey.Encrypt(plainText);
                }
            }

            // HardwareVaultTasks
            var scopedHardwareVaultTaskRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVaultTask>>();
            var hardwareVaultTasks = await scopedHardwareVaultTaskRepository.Query().ToListAsync();
            foreach (var task in hardwareVaultTasks)
            {
                if (task.Password != null)
                {
                    var plainText = key.Decrypt(task.Password);
                    task.Password = newKey.Encrypt(plainText);
                }
                if (task.OtpSecret != null)
                {
                    var plainText = key.Decrypt(task.OtpSecret);
                    task.OtpSecret = newKey.Encrypt(plainText);
                }
            }

            // SharedAccounts
            var scopedSharedAccountRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<SharedAccount>>();
            var sharedAccounts = await scopedSharedAccountRepository.Query().ToListAsync();
            foreach (var account in sharedAccounts)
            {
                if (account.Password != null)
                {
                    var plainText = key.Decrypt(account.Password);
                    account.Password = newKey.Encrypt(plainText);
                }
                if (account.OtpSecret != null)
                {
                    var plainText = key.Decrypt(account.OtpSecret);
                    account.OtpSecret = newKey.Encrypt(plainText);
                }
            }

            // SoftwareVaultInvitations
            //var scopedSoftwareVaultInvitationRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<SoftwareVaultInvitation>>();
            //var softwareVaultInvitations = await scopedSoftwareVaultInvitationRepository.Query().ToListAsync();
            //foreach (var softwareVaultInvitation in softwareVaultInvitations)
            //{
            //    
            //}

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await scopedAccountRepository.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.Password), nameof(Account.OtpSecret) });
                if (domainSettings != null)
                    await scopedAppSettingsRepository.UpdateAsync(domainSettings);
                if (licenseSettings != null)
                    await scopedAppSettingsRepository.UpdateAsync(licenseSettings);
                await scopedHardwareVaultActivationRepository.UpdateOnlyPropAsync(hardwareVaultActivations, new string[] { nameof(HardwareVaultActivation.AcivationCode) });
                await scopedHardwareVaultRepository.UpdateOnlyPropAsync(hardwareVaults, new string[] { nameof(HardwareVault.MasterPassword) });
                await scopedHardwareVaultTaskRepository.UpdateOnlyPropAsync(hardwareVaultTasks, new string[] { nameof(HardwareVaultTask.Password), nameof(HardwareVaultTask.OtpSecret) });
                await scopedSharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { nameof(SharedAccount.Password), nameof(SharedAccount.OtpSecret) });
                //await scopedSoftwareVaultInvitationRepository.UpdateOnlyPropAsync(softwareVaultInvitations, new string[] { nameof(SoftwareVaultInvitation.ActivationCode) });
                transactionScope.Complete();
            }
        }

        private async Task EncryptDatabase(DataProtectionKey key)
        {
            using var scope = Services.CreateScope();

            // Accounts
            var scopedAccountRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<Account>>();
            var accounts = await scopedAccountRepository.Query().ToListAsync();
            foreach (var account in accounts)
            {
                if (account.Password != null)
                    account.Password = key.Encrypt(account.Password);
                if (account.OtpSecret != null)
                    account.OtpSecret = key.Encrypt(account.OtpSecret);
            }

            // AppSettings
            var scopedAppSettingsRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<AppSettings>>();
            var domainSettings = await scopedAppSettingsRepository.GetByIdAsync(ServerConstants.Domain);
            if (domainSettings != null)
            {
                var settings = JsonConvert.DeserializeObject<LdapSettings>(domainSettings.Value);
                settings.Password = key.Encrypt(settings.Password);
                var json = JsonConvert.SerializeObject(settings);
                domainSettings.Value = json;
            }
            var licenseSettings = await scopedAppSettingsRepository.GetByIdAsync(ServerConstants.Licensing);
            if (licenseSettings != null)
            {
                var settings = JsonConvert.DeserializeObject<LicensingSettings>(licenseSettings.Value);
                settings.ApiKey = key.Encrypt(settings.ApiKey);
                var json = JsonConvert.SerializeObject(settings);
                licenseSettings.Value = json;
            }

            // HardwareVaultsActivations
            var scopedHardwareVaultActivationRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVaultActivation>>();
            var hardwareVaultActivations = await scopedHardwareVaultActivationRepository.Query().ToListAsync();
            foreach (var hardwareVaultActivation in hardwareVaultActivations)
            {
                hardwareVaultActivation.AcivationCode = key.Encrypt(hardwareVaultActivation.AcivationCode);
            }

            // HardwareVaults   
            var scopedHardwareVaultRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVault>>();
            var hardwareVaults = await scopedHardwareVaultRepository.Query().ToListAsync();
            foreach (var hardwareVault in hardwareVaults)
            {
                if (hardwareVault.MasterPassword != null)
                    hardwareVault.MasterPassword = key.Encrypt(hardwareVault.MasterPassword);
            }

            // HardwareVaultTasks
            var scopedHardwareVaultTaskRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVaultTask>>();
            var hardwareVaultTasks = await scopedHardwareVaultTaskRepository.Query().ToListAsync();
            foreach (var task in hardwareVaultTasks)
            {
                if (task.Password != null)
                    task.Password = key.Encrypt(task.Password);
                if (task.OtpSecret != null)
                    task.OtpSecret = key.Encrypt(task.OtpSecret);
            }

            // SharedAccounts
            var scopedSharedAccountRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<SharedAccount>>();
            var sharedAccounts = await scopedSharedAccountRepository.Query().ToListAsync();
            foreach (var account in sharedAccounts)
            {
                if (account.Password != null)
                    account.Password = key.Encrypt(account.Password);
                if (account.OtpSecret != null)
                    account.OtpSecret = key.Encrypt(account.OtpSecret);
            }

            // SoftwareVaultInvitations
            //var scopedSoftwareVaultInvitationRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<SoftwareVaultInvitation>>();
            //var softwareVaultInvitations = await scopedSoftwareVaultInvitationRepository.Query().ToListAsync();
            //foreach (var softwareVaultInvitation in softwareVaultInvitations)
            //{
            //    softwareVaultInvitation.ActivationCode = key.Encrypt(softwareVaultInvitation.ActivationCode);
            //}

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await scopedAccountRepository.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.Password), nameof(Account.OtpSecret) });
                if (domainSettings != null)
                    await scopedAppSettingsRepository.UpdateAsync(domainSettings);
                if (licenseSettings != null)
                    await scopedAppSettingsRepository.UpdateAsync(licenseSettings);
                await scopedHardwareVaultActivationRepository.UpdateOnlyPropAsync(hardwareVaultActivations, new string[] { nameof(HardwareVaultActivation.AcivationCode) });
                await scopedHardwareVaultRepository.UpdateOnlyPropAsync(hardwareVaults, new string[] { nameof(HardwareVault.MasterPassword) });
                await scopedHardwareVaultTaskRepository.UpdateOnlyPropAsync(hardwareVaultTasks, new string[] { nameof(HardwareVaultTask.Password), nameof(HardwareVaultTask.OtpSecret) });
                await scopedSharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { nameof(SharedAccount.Password), nameof(SharedAccount.OtpSecret) });
                //await scopedSoftwareVaultInvitationRepository.UpdateOnlyPropAsync(softwareVaultInvitations, new string[] { nameof(SoftwareVaultInvitation.ActivationCode) });
                transactionScope.Complete();
            }
        }

        private async Task DecryptDatabase(DataProtectionKey key)
        {
            using var scope = Services.CreateScope();

            // Accounts
            var scopedAccountRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<Account>>();
            var accounts = await scopedAccountRepository.Query().ToListAsync();
            foreach (var account in accounts)
            {
                if (account.Password != null)
                    account.Password = key.Decrypt(account.Password);
                if (account.OtpSecret != null)
                    account.OtpSecret = key.Decrypt(account.OtpSecret);
            }

            // AppSettings
            var scopedAppSettingsRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<AppSettings>>();
            var domainSettings = await scopedAppSettingsRepository.GetByIdAsync(ServerConstants.Domain);
            if (domainSettings != null)
            {
                var settings = JsonConvert.DeserializeObject<LdapSettings>(domainSettings.Value);
                settings.Password = key.Decrypt(settings.Password);
                var json = JsonConvert.SerializeObject(settings);
                domainSettings.Value = json;
            }
            var licenseSettings = await scopedAppSettingsRepository.GetByIdAsync(ServerConstants.Licensing);
            if (licenseSettings != null)
            {
                var settings = JsonConvert.DeserializeObject<LicensingSettings>(licenseSettings.Value);
                settings.ApiKey = key.Decrypt(settings.ApiKey);
                var json = JsonConvert.SerializeObject(settings);
                licenseSettings.Value = json;
            }

            // HardwareVaultsActivations
            var scopedHardwareVaultActivationRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVaultActivation>>();
            var hardwareVaultActivations = await scopedHardwareVaultActivationRepository.Query().ToListAsync();
            foreach (var hardwareVaultActivation in hardwareVaultActivations)
            {
                hardwareVaultActivation.AcivationCode = key.Decrypt(hardwareVaultActivation.AcivationCode);
            }

            // HardwareVaults   
            var scopedHardwareVaultRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVault>>();
            var hardwareVaults = await scopedHardwareVaultRepository.Query().ToListAsync();
            foreach (var hardwareVault in hardwareVaults)
            {
                if (hardwareVault.MasterPassword != null)
                    hardwareVault.MasterPassword = key.Decrypt(hardwareVault.MasterPassword);
            }

            // HardwareVaultTasks
            var scopedHardwareVaultTaskRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<HardwareVaultTask>>();
            var hardwareVaultTasks = await scopedHardwareVaultTaskRepository.Query().ToListAsync();
            foreach (var task in hardwareVaultTasks)
            {
                if (task.Password != null)
                    task.Password = key.Decrypt(task.Password);
                if (task.OtpSecret != null)
                    task.OtpSecret = key.Decrypt(task.OtpSecret);
            }

            // SharedAccounts
            var scopedSharedAccountRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<SharedAccount>>();
            var sharedAccounts = await scopedSharedAccountRepository.Query().ToListAsync();
            foreach (var account in sharedAccounts)
            {
                if (account.Password != null)
                    account.Password = key.Decrypt(account.Password);
                if (account.OtpSecret != null)
                    account.OtpSecret = key.Decrypt(account.OtpSecret);
            }

            // SoftwareVaultInvitations
            //var scopedSoftwareVaultInvitationRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<SoftwareVaultInvitation>>();
            //var softwareVaultInvitations = await scopedSoftwareVaultInvitationRepository.Query().ToListAsync();
            //foreach (var softwareVaultInvitation in softwareVaultInvitations)
            //{
            //    softwareVaultInvitation.ActivationCode = key.Encrypt(softwareVaultInvitation.ActivationCode);
            //}

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await scopedAccountRepository.UpdateOnlyPropAsync(accounts, new string[] { nameof(Account.Password), nameof(Account.OtpSecret) });
                if (domainSettings != null)
                    await scopedAppSettingsRepository.UpdateAsync(domainSettings);
                if (licenseSettings != null)
                    await scopedAppSettingsRepository.UpdateAsync(licenseSettings);
                await scopedHardwareVaultActivationRepository.UpdateOnlyPropAsync(hardwareVaultActivations, new string[] { nameof(HardwareVaultActivation.AcivationCode) });
                await scopedHardwareVaultRepository.UpdateOnlyPropAsync(hardwareVaults, new string[] { nameof(HardwareVault.MasterPassword) });
                await scopedHardwareVaultTaskRepository.UpdateOnlyPropAsync(hardwareVaultTasks, new string[] { nameof(HardwareVaultTask.Password), nameof(HardwareVaultTask.OtpSecret) });
                await scopedSharedAccountRepository.UpdateOnlyPropAsync(sharedAccounts, new string[] { nameof(SharedAccount.Password), nameof(SharedAccount.OtpSecret) });
                //await scopedSoftwareVaultInvitationRepository.UpdateOnlyPropAsync(softwareVaultInvitations, new string[] { nameof(SoftwareVaultInvitation.ActivationCode) });
                transactionScope.Complete();
            }
        }

        private string TryGetStoredPassword()
        {
            using var scope = Services.CreateScope();
            var scopedConfiguration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            return scopedConfiguration.GetValue<string>("DataProtection:Password");
        }

        #region Data Protection Params

        private async Task<DataProtection> ReadDataProtectionEntity()
        {
            using var scope = Services.CreateScope();
            var scopedDataProtectionRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DataProtection>>();

            var list = await scopedDataProtectionRepository
                .Query()
                .ToListAsync();

            if (list.Count == 0)
                return null;

            if (list.Count > 1)
                throw new NotFinishedPasswordChangeException("Detected not finished password change operation");

            var entity = list[0];
            if (string.IsNullOrEmpty(entity.Value))
                throw new Exception("Data Protection parameters is empty");

            entity.Params = JsonConvert.DeserializeObject<DataProtectionParams>(entity.Value);

            return entity;
        }

        private async Task<DataProtection> SaveDataProtectionEntity(DataProtectionParams prms)
        {
            using var scope = Services.CreateScope();
            var scopedDataProtectionRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DataProtection>>();

            var entity = await scopedDataProtectionRepository.AddAsync(new DataProtection()
            {
                Value = JsonConvert.SerializeObject(prms),
                Params = prms
            });

            return entity;
        }

        private async Task DeleteDataProtectionEntity(int id)
        {
            using var scope = Services.CreateScope();
            var scopedDataProtectionRepository = scope.ServiceProvider.GetRequiredService<IAsyncRepository<DataProtection>>();

            var entity = await scopedDataProtectionRepository
                .Query()
                .Where(v => v.Id == id)
                .FirstOrDefaultAsync();

            if (entity != null)
                await scopedDataProtectionRepository.DeleteAsync(entity);
        }

        #endregion
    }
}