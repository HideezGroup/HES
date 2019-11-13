﻿using HES.Core.Entities;
using HES.Core.Models;
using HES.Core.Interfaces;
using HES.Core.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class SharedAccountService : ISharedAccountService
    {
        private readonly IAsyncRepository<SharedAccount> _sharedAccountRepository;
        private readonly IDeviceAccountService _deviceAccountService;
        private readonly IDeviceTaskService _deviceTaskService;
        private readonly IDataProtectionService _dataProtectionService;

        public SharedAccountService(IAsyncRepository<SharedAccount> sharedAccountRepository,
                                    IDeviceAccountService deviceAccountService,
                                    IDeviceTaskService deviceTaskService,
                                    IDataProtectionService dataProtectionService)
        {
            _sharedAccountRepository = sharedAccountRepository;
            _deviceAccountService = deviceAccountService;
            _deviceTaskService = deviceTaskService;
            _dataProtectionService = dataProtectionService;
        }

        public IQueryable<SharedAccount> Query()
        {
            return _sharedAccountRepository.Query();
        }

        public async Task<SharedAccount> GetByIdAsync(dynamic id)
        {
            return await _sharedAccountRepository.GetByIdAsync(id);
        }

        public async Task<List<SharedAccount>> GetSharedAccountsAsync()
        {
            return await _sharedAccountRepository
                .Query()
                .Where(d => d.Deleted == false)
                .ToListAsync();
        }

        public async Task<SharedAccount> CreateSharedAccountAsync(SharedAccount sharedAccount)
        {
            _dataProtectionService.Validate();

            if (sharedAccount == null)
            {
                throw new ArgumentNullException(nameof(sharedAccount));
            }

            ValidationHepler.VerifyOtpSecret(sharedAccount.OtpSecret);

            var exist = await _sharedAccountRepository
                .Query()
                .Where(s => s.Name == sharedAccount.Name &&
                            s.Login == sharedAccount.Login &&
                            s.Deleted == false)
                .AsNoTracking()
                .AnyAsync();

            if (exist)
            {
                throw new Exception("An account with the same name and login exists.");
            }

            // Validate url
            sharedAccount.Urls = ValidationHepler.VerifyUrls(sharedAccount.Urls);

            // Set password
            sharedAccount.Password = _dataProtectionService.Protect(sharedAccount.Password);
            // Set password date change
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;
            // Set otp date change
            if (!string.IsNullOrWhiteSpace(sharedAccount.OtpSecret))
            {
                sharedAccount.OtpSecret = _dataProtectionService.Protect(sharedAccount.OtpSecret);
                sharedAccount.OtpSecretChangedAt = DateTime.UtcNow;
            }

            return await _sharedAccountRepository.AddAsync(sharedAccount);
        }

        public async Task CreateWorkstationSharedAccountAsync(WorkstationAccount workstationAccount)
        {
            if (workstationAccount == null)
            {
                throw new ArgumentNullException(nameof(workstationAccount));
            }

            var sharedAccount = new SharedAccount()
            {
                Name = workstationAccount.Name,
                Kind = AccountKind.Workstation,
                Password = workstationAccount.Password
            };

            switch (workstationAccount.AccountType)
            {
                case WorkstationAccountType.Local:
                    sharedAccount.Login = $".\\{workstationAccount.Login}";
                    break;
                case WorkstationAccountType.Domain:
                    sharedAccount.Login = $"{workstationAccount.Domain}\\{workstationAccount.Login}";
                    break;
                case WorkstationAccountType.Microsoft:
                    sharedAccount.Login = $"@\\{workstationAccount.Login}";
                    break;
            }

            await CreateSharedAccountAsync(sharedAccount);
        }

        public async Task<List<string>> EditSharedAccountAsync(SharedAccount sharedAccount)
        {
            _dataProtectionService.Validate();

            if (sharedAccount == null)
            {
                throw new Exception("The parameter must not be null.");
            }

            var exist = await _sharedAccountRepository
                .Query()
                .Where(s => s.Name == sharedAccount.Name)
                .Where(s => s.Login == sharedAccount.Login)
                .Where(s => s.Deleted == false)
                .Where(s => s.Id != sharedAccount.Id)
                .AnyAsync();

            if (exist)
            {
                throw new Exception("An account with the same name and login exists.");
            }

            // Validate url
            if (sharedAccount.Urls != null)
            {
                sharedAccount.Urls = ValidationHepler.VerifyUrls(sharedAccount.Urls);
            }

            // Update Shared Account
            string[] properties = { "Name", "Urls", "Apps", "Login" };
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, properties);

            // Update all device accounts
            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountService
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Status = AccountStatus.Updating;
                deviceAccount.UpdatedAt = DateTime.UtcNow;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    Name = sharedAccount.Name,
                    Urls = sharedAccount.Urls ?? string.Empty,
                    Apps = sharedAccount.Apps ?? string.Empty,
                    Login = sharedAccount.Login,
                    Password = null,
                    OtpSecret = null,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }

            // Update device accounts
            await _deviceAccountService.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Status", "UpdatedAt" });

            // Create Tasks
            await _deviceTaskService.AddRangeTasksAsync(tasks);

            var devices = deviceAccounts.Select(s => s.DeviceId).ToList();
            return devices;
        }

        public async Task<List<string>> EditSharedAccountPwdAsync(SharedAccount sharedAccount)
        {
            if (sharedAccount == null)
            {
                throw new ArgumentNullException(nameof(sharedAccount));
            }

            _dataProtectionService.Validate();

            // Update Shared Account
            sharedAccount.Password = _dataProtectionService.Protect(sharedAccount.Password);
            sharedAccount.PasswordChangedAt = DateTime.UtcNow;
            string[] properties = { "Password", "PasswordChangedAt" };
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, properties);

            // Update all device accounts
            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountService
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Status = AccountStatus.Updating;
                deviceAccount.UpdatedAt = DateTime.UtcNow;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    Password = sharedAccount.Password,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }

            // Update device accounts
            await _deviceAccountService.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Status", "UpdatedAt" });

            // Create Tasks
            await _deviceTaskService.AddRangeTasksAsync(tasks);

            var devices = deviceAccounts.Select(s => s.DeviceId).ToList();
            return devices;
        }

        public async Task<List<string>> EditSharedAccountOtpAsync(SharedAccount sharedAccount)
        {
            if (sharedAccount == null)
            {
                throw new ArgumentNullException(nameof(sharedAccount));
            }

            _dataProtectionService.Validate();

            ValidationHepler.VerifyOtpSecret(sharedAccount.OtpSecret);

            // Update Shared Account
            sharedAccount.OtpSecret = !string.IsNullOrWhiteSpace(sharedAccount.OtpSecret) ? _dataProtectionService.Protect(sharedAccount.OtpSecret) : null;
            sharedAccount.OtpSecretChangedAt = !string.IsNullOrWhiteSpace(sharedAccount.OtpSecret) ? new DateTime?(DateTime.UtcNow) : null;
            string[] properties = { "OtpSecret", "OtpSecretChangedAt" };
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, properties);

            // Update all device accounts
            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountService
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Status = AccountStatus.Updating;
                deviceAccount.UpdatedAt = DateTime.UtcNow;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    OtpSecret = sharedAccount.OtpSecret ?? string.Empty,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Update,
                    DeviceId = deviceAccount.DeviceId
                });
            }

            // Update device accounts
            await _deviceAccountService.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Status", "UpdatedAt" });

            // Create Tasks
            await _deviceTaskService.AddRangeTasksAsync(tasks);

            var devices = deviceAccounts.Select(s => s.DeviceId).ToList();
            return devices;
        }

        public async Task<List<string>> DeleteSharedAccountAsync(string id)
        {
            _dataProtectionService.Validate();

            if (id == null)
            {
                throw new Exception("The parameter must not be null.");
            }
            var sharedAccount = await _sharedAccountRepository.GetByIdAsync(id);
            if (sharedAccount == null)
            {
                throw new Exception("Shared account does not exist.");
            }

            sharedAccount.Deleted = true;
            await _sharedAccountRepository.UpdateOnlyPropAsync(sharedAccount, new string[] { "Deleted" });

            // Update all device accounts
            // Get all device accounts where equals this shared account
            var deviceAccounts = await _deviceAccountService
                .Query()
                .Where(d => d.Deleted == false)
                .Where(d => d.SharedAccountId == sharedAccount.Id)
                .ToListAsync();

            List<DeviceTask> tasks = new List<DeviceTask>();

            foreach (var deviceAccount in deviceAccounts)
            {
                deviceAccount.Status = AccountStatus.Removing;
                deviceAccount.UpdatedAt = DateTime.UtcNow;

                // Add Device Task
                tasks.Add(new DeviceTask
                {
                    DeviceAccountId = deviceAccount.Id,
                    CreatedAt = DateTime.UtcNow,
                    Operation = TaskOperation.Delete,
                    DeviceId = deviceAccount.DeviceId
                });
            }

            // Update device accounts
            await _deviceAccountService.UpdateOnlyPropAsync(deviceAccounts, new string[] { "Status", "UpdatedAt" });

            // Create Tasks
            await _deviceTaskService.AddRangeTasksAsync(tasks);

            var devices = deviceAccounts.Select(s => s.DeviceId).ToList();
            return devices;
        }

        public async Task<bool> ExistAync(Expression<Func<SharedAccount, bool>> predicate)
        {
            return await _sharedAccountRepository.ExistAsync(predicate);
        }
    }
}