using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API.HardwareVault;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Filters;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.HES.DTO;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class HardwareVaultService : IHardwareVaultService
    {
        private readonly ILicenseService _licenseService;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly IWorkstationService _workstationService;
        private readonly IApplicationDbContext _dbContext;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly IHttpClientFactory _httpClientFactory;

        public HardwareVaultService(ILicenseService licenseService,
                                    IHardwareVaultTaskService hardwareVaultTaskService,
                                    IApplicationDbContext dbContext,
                                    IWorkstationService workstationService,
                                    IAppSettingsService appSettingsService,
                                    IDataProtectionService dataProtectionService,
                                    IHttpClientFactory httpClientFactory)
        {
            _licenseService = licenseService;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _dbContext = dbContext;
            _workstationService = workstationService;
            _appSettingsService = appSettingsService;
            _dataProtectionService = dataProtectionService;
            _httpClientFactory = httpClientFactory;
        }

        #region Vault

        public IQueryable<HardwareVault> VaultQuery()
        {
            return _dbContext.HardwareVaults.AsQueryable();
        }

        public async Task<HardwareVault> GetVaultByIdAsync(string vaultId)
        {
            return await _dbContext.HardwareVaults
                .Include(d => d.Employee.Department.Company)
                .Include(d => d.Employee.HardwareVaults)
                .Include(d => d.Employee.SoftwareVaults)
                .Include(d => d.HardwareVaultProfile)
                .Include(d => d.HardwareVaultTasks)
                .FirstOrDefaultAsync(d => d.Id == vaultId);
        }

        public async Task<List<HardwareVault>> GetVaultsWithoutLicenseAsync()
        {
            return await _dbContext.HardwareVaults
                    .Where(x => x.LicenseStatus == VaultLicenseStatus.None ||
                                x.LicenseStatus == VaultLicenseStatus.Expired)
                    .AsNoTracking()
                    .ToListAsync();
        }

        public async Task<List<HardwareVault>> GetVaultsWithLicenseAsync()
        {
            return await _dbContext.HardwareVaults
                     .Where(x => x.LicenseStatus != VaultLicenseStatus.None &&
                            x.LicenseStatus != VaultLicenseStatus.Expired)
                    .AsNoTracking()
                    .ToListAsync();
        }

        public async Task<List<HardwareVault>> GetVaultsAsync(DataLoadingOptions<HardwareVaultFilter> dataLoadingOptions)
        {
            return await HardwareVaultQuery(dataLoadingOptions).Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetVaultsCountAsync(DataLoadingOptions<HardwareVaultFilter> dataLoadingOptions)
        {
            return await HardwareVaultQuery(dataLoadingOptions).CountAsync();
        }

        private IQueryable<HardwareVault> HardwareVaultQuery(DataLoadingOptions<HardwareVaultFilter> dataLoadingOptions)
        {
            var query = _dbContext.HardwareVaults
                .Include(d => d.Employee.Department.Company)
                .Include(d => d.HardwareVaultProfile)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Id != null)
                {
                    query = query.Where(w => w.Id.Contains(dataLoadingOptions.Filter.Id));
                }
                if (dataLoadingOptions.Filter.MAC != null)
                {
                    query = query.Where(w => w.MAC.Contains(dataLoadingOptions.Filter.MAC));
                }
                if (dataLoadingOptions.Filter.Model != null)
                {
                    query = query.Where(w => w.Model.Contains(dataLoadingOptions.Filter.Model));
                }
                if (dataLoadingOptions.Filter.RFID != null)
                {
                    query = query.Where(w => w.RFID.Contains(dataLoadingOptions.Filter.RFID));
                }
                if (dataLoadingOptions.Filter.Battery != null)
                {
                    switch (dataLoadingOptions.Filter.Battery)
                    {
                        case "low":
                            query = query.Where(w => w.Battery <= 30);
                            break;
                        case "high":
                            query = query.Where(w => w.Battery >= 31);
                            break;
                    }
                }
                if (dataLoadingOptions.Filter.Firmware != null)
                {
                    query = query.Where(w => w.Firmware.Contains(dataLoadingOptions.Filter.Firmware));
                }
                if (dataLoadingOptions.Filter.LastSyncedStartDate != null)
                {
                    query = query.Where(x => x.LastSynced >= dataLoadingOptions.Filter.LastSyncedStartDate.Value.Date.ToUniversalTime());
                }
                if (dataLoadingOptions.Filter.LastSyncedEndDate != null)
                {
                    query = query.Where(x => x.LastSynced <= dataLoadingOptions.Filter.LastSyncedEndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime());
                }
                if (dataLoadingOptions.Filter.Employee != null)
                {
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.Filter.Employee));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(x => x.Employee.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(x => x.Employee.Department.Name.Contains(dataLoadingOptions.Filter.Department));
                }
                if (dataLoadingOptions.Filter.Status != null)
                {
                    query = query.Where(w => w.Status == dataLoadingOptions.Filter.Status);
                }
                if (dataLoadingOptions.Filter.LicenseStatus != null)
                {
                    query = query.Where(w => w.LicenseStatus == dataLoadingOptions.Filter.LicenseStatus);
                }
                if (dataLoadingOptions.Filter.LicenseEndDate != null)
                {
                    query = query.Where(x => x.LicenseEndDate <= dataLoadingOptions.Filter.LicenseEndDate.Value.Date);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Id.Contains(dataLoadingOptions.SearchText) ||
                                    x.MAC.Contains(dataLoadingOptions.SearchText) ||
                                    x.Battery.ToString().Contains(dataLoadingOptions.SearchText) ||
                                    x.Firmware.Contains(dataLoadingOptions.SearchText) ||
                                    x.HardwareVaultProfile.Name.Contains(dataLoadingOptions.SearchText) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.SearchText) ||
                                    x.Employee.Department.Company.Name.Contains(dataLoadingOptions.SearchText) ||
                                    x.Employee.Department.Name.Contains(dataLoadingOptions.SearchText) ||
                                    x.Model.Contains(dataLoadingOptions.SearchText) ||
                                    x.RFID.Contains(dataLoadingOptions.SearchText));
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(HardwareVault.Id):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Id) : query.OrderByDescending(x => x.Id);
                    break;
                case nameof(HardwareVault.MAC):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.MAC) : query.OrderByDescending(x => x.MAC);
                    break;
                case nameof(HardwareVault.Battery):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Battery) : query.OrderByDescending(x => x.Battery);
                    break;
                case nameof(HardwareVault.Firmware):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Firmware) : query.OrderByDescending(x => x.Firmware);
                    break;
                case nameof(HardwareVault.HardwareVaultProfile):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.HardwareVaultProfile.Name) : query.OrderByDescending(x => x.HardwareVaultProfile.Name);
                    break;
                case nameof(HardwareVault.Status):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Status) : query.OrderByDescending(x => x.Status);
                    break;
                case nameof(HardwareVault.LastSynced):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LastSynced) : query.OrderByDescending(x => x.LastSynced);
                    break;
                case nameof(HardwareVault.LicenseStatus):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LicenseStatus) : query.OrderByDescending(x => x.LicenseStatus);
                    break;
                case nameof(HardwareVault.LicenseEndDate):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.LicenseEndDate) : query.OrderByDescending(x => x.LicenseEndDate);
                    break;
                case nameof(HardwareVault.Employee):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.FirstName).ThenBy(x => x.Employee.LastName) : query.OrderByDescending(x => x.Employee.FirstName).ThenByDescending(x => x.Employee.LastName);
                    break;
                case nameof(HardwareVault.Employee.Department.Company):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Company.Name) : query.OrderByDescending(x => x.Employee.Department.Company.Name);
                    break;
                case nameof(HardwareVault.Employee.Department):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Employee.Department.Name) : query.OrderByDescending(x => x.Employee.Department.Name);
                    break;
                case nameof(HardwareVault.Model):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Model) : query.OrderByDescending(x => x.Model);
                    break;
                case nameof(HardwareVault.RFID):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.RFID) : query.OrderByDescending(x => x.RFID);
                    break;
            }

            return query;
        }

        public async Task ImportVaultsAsync()
        {
            var licensing = await _appSettingsService.GetLicenseSettingsAsync();

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(licensing.ApiAddress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var path = $"api/Devices/GetDevicesWithLicenses/{licensing.ApiKey}";
            var response = await client.GetAsync(path);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Api response: {error}");
            }

            var data = await response.Content.ReadAsStringAsync();
            var importDto = JsonConvert.DeserializeObject<ImportHardwareVaultDto>(data);

            var licenseOrdersToImport = new List<LicenseOrder>();
            var hardwareVaultLicensesToImport = new List<HardwareVaultLicense>();
            var hardwareVaultsToImport = new List<HardwareVault>();

            // Import license orders               
            var licenseOrders = await _licenseService.GetLicenseOrdersAsync();
            // Remove existing
            importDto.LicenseOrdersDto.RemoveAll(x => licenseOrders.Select(s => s.Id).Contains(x.Id));

            foreach (var licenseOrderDto in importDto.LicenseOrdersDto)
            {
                licenseOrdersToImport.Add(new LicenseOrder
                {
                    Id = licenseOrderDto.Id,
                    ContactEmail = licenseOrderDto.ContactEmail,
                    Note = licenseOrderDto.Note,
                    StartDate = licenseOrderDto.StartDate,
                    EndDate = licenseOrderDto.EndDate,
                    ProlongExistingLicenses = licenseOrderDto.ProlongExistingLicenses,
                    CreatedAt = licenseOrderDto.CreatedAt,
                    OrderStatus = licenseOrderDto.OrderStatus
                });
            }

            // Import hardware vault licenses
            var hardwareVaultLicenses = await _licenseService.GetLicensesAsync();
            // Remove existing
            importDto.HardwareVaultLicensesDto.RemoveAll(x => hardwareVaultLicenses.Select(s => s.LicenseOrderId).Contains(x.LicenseOrderId) && hardwareVaultLicenses.Select(s => s.HardwareVaultId).Contains(x.HardwareVaultId));

            foreach (var hardwareVaultLicenseDto in importDto.HardwareVaultLicensesDto)
            {
                hardwareVaultLicensesToImport.Add(new HardwareVaultLicense()
                {
                    HardwareVaultId = hardwareVaultLicenseDto.HardwareVaultId,
                    LicenseOrderId = hardwareVaultLicenseDto.LicenseOrderId,
                    ImportedAt = DateTime.UtcNow,
                    AppliedAt = null,
                    Data = hardwareVaultLicenseDto.Data == null ? null : Convert.FromBase64String(hardwareVaultLicenseDto.Data),
                    EndDate = hardwareVaultLicenseDto.EndDate
                });
            }

            // Import hardware vaults
            var hardwareVaults = await GetVaultsAsync(new DataLoadingOptions<HardwareVaultFilter>()
            {
                Take = await GetVaultsCountAsync(new DataLoadingOptions<HardwareVaultFilter>()),
                SortedColumn = nameof(HardwareVault.Id),
                SortDirection = ListSortDirection.Ascending

            });

            // Remove existing
            importDto.HardwareVaultsDto.RemoveAll(x => hardwareVaults.Select(s => s.Id).Contains(x.HardwareVaultId));

            foreach (var hardwareVaultDto in importDto.HardwareVaultsDto)
            {
                var hardwareVaultLicense = hardwareVaultLicensesToImport.OrderByDescending(x => x.EndDate).FirstOrDefault(x => x.HardwareVaultId == hardwareVaultDto.HardwareVaultId);

                hardwareVaultsToImport.Add(new HardwareVault()
                {
                    Id = hardwareVaultDto.HardwareVaultId,
                    MAC = hardwareVaultDto.MAC,
                    Model = hardwareVaultDto.Model,
                    RFID = hardwareVaultDto.RFID,
                    Battery = 100,
                    Firmware = hardwareVaultDto.Firmware,
                    Status = VaultStatus.Ready,
                    StatusReason = VaultStatusReason.None,
                    StatusDescription = null,
                    LastSynced = null,
                    NeedSync = false,
                    EmployeeId = null,
                    MasterPassword = null,
                    HardwareVaultProfileId = ServerConstants.DefaulHardwareVaultProfileId,
                    ImportedAt = DateTime.UtcNow,
                    HasNewLicense = hardwareVaultLicense == null ? false : true,
                    LicenseStatus = VaultLicenseStatus.None,
                    LicenseEndDate = hardwareVaultLicense == null ? null : hardwareVaultLicense.EndDate
                });
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                _dbContext.HardwareVaults.AddRange(hardwareVaultsToImport);
                await _dbContext.SaveChangesAsync();

                await _licenseService.AddOrderRangeAsync(licenseOrdersToImport);
                await _licenseService.AddHardwareVaultLicenseRangeAsync(hardwareVaultLicensesToImport);
                transactionScope.Complete();
            }

            // Without transaction
            await _licenseService.UpdateHardwareVaultsLicenseStatusAsync();
        }

        public void UnchangedVault(HardwareVault vault)
        {
            _dbContext.Unchanged(vault);
        }

        public async Task UpdateRfidAsync(HardwareVault vault)
        {
            if (vault == null)
            {
                throw new ArgumentNullException(nameof(vault));
            }

            await UpdateVaultAsync(vault);
        }

        public async Task UpdateNeedSyncAsync(HardwareVault vault, bool needSync)
        {
            if (vault == null)
            {
                throw new ArgumentNullException(nameof(vault));
            }

            vault.NeedSync = needSync;

            await UpdateVaultAsync(vault);
        }

        public async Task UpdateNeedSyncAsync(List<HardwareVault> vaults, bool needSync)
        {
            if (vaults == null)
            {
                throw new ArgumentNullException(nameof(vaults));
            }

            vaults.ForEach(x => x.NeedSync = needSync);

            _dbContext.HardwareVaults.UpdateRange(vaults);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateTimestampAsync(HardwareVault vault, uint timestamp)
        {
            if (vault == null)
            {
                throw new ArgumentNullException(nameof(vault));
            }

            vault.Timestamp = timestamp;

            await UpdateVaultAsync(vault);
        }

        public async Task<HardwareVault> UpdateVaultAsync(HardwareVault vault)
        {
            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

            var result = _dbContext.HardwareVaults.Update(vault);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task UpdateVaultInfoAsync(HwVaultInfoFromClientDto dto)
        {
            var vault = await GetVaultByIdAsync(dto.VaultSerialNo);

            if (vault == null)
            {
                throw new HideezException(HideezErrorCode.HesDeviceNotFound);
            }

            vault.Timestamp = dto.StorageTimestamp;
            vault.Battery = dto.Battery;
            vault.Firmware = dto.FirmwareVersion;
            vault.LastSynced = DateTime.UtcNow;

            await UpdateVaultAsync(vault);
        }

        public async Task SetReadyStatusAsync(HardwareVault vault)
        {
            if (vault == null)
            {
                throw new ArgumentNullException(nameof(vault));
            }

            vault.LastSynced = DateTime.UtcNow;
            vault.EmployeeId = null;
            vault.MasterPassword = null;
            vault.HardwareVaultProfileId = ServerConstants.DefaulHardwareVaultProfileId;
            vault.Status = VaultStatus.Ready;
            vault.StatusReason = VaultStatusReason.None;
            vault.StatusDescription = null;
            vault.HasNewLicense = false;
            vault.NeedSync = false;
            vault.IsStatusApplied = false;
            vault.Timestamp = 0;

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await UpdateVaultAsync(vault);
                await ChangeVaultActivationStatusAsync(vault.Id, HardwareVaultActivationStatus.Canceled);
                transactionScope.Complete();
            }
        }

        public async Task SetActiveStatusAsync(HardwareVault vault)
        {
            if (vault == null)
            {
                throw new ArgumentNullException(nameof(vault));
            }

            vault.Status = VaultStatus.Active;
            vault.StatusReason = VaultStatusReason.None;
            vault.StatusDescription = null;

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await UpdateVaultAsync(vault);
                await ChangeVaultActivationStatusAsync(vault.Id, HardwareVaultActivationStatus.Activated);

                transactionScope.Complete();
            }
        }

        public async Task SetLockedStatusAsync(HardwareVault vault)
        {
            if (vault == null)
            {
                throw new ArgumentNullException(nameof(vault));
            }

            vault.Status = VaultStatus.Locked;

            await UpdateVaultAsync(vault);
        }

        public async Task SetDeactivatedStatusAsync(HardwareVault vault)
        {
            if (vault == null)
            {
                throw new ArgumentNullException(nameof(vault));
            }

            vault.EmployeeId = null;
            vault.Status = VaultStatus.Deactivated;

            await UpdateVaultAsync(vault);
        }

        public async Task SetVaultStatusAppliedAsync(HardwareVault vault)
        {
            if (vault == null)
            {
                throw new ArgumentNullException(nameof(vault));
            }

            vault.IsStatusApplied = true;

            await UpdateVaultAsync(vault);
        }

        public async Task<HardwareVaultActivation> CreateVaultActivationAsync(string vaultId)
        {
            if (string.IsNullOrWhiteSpace(vaultId))
            {
                throw new ArgumentNullException(nameof(vaultId));
            }

            var existActivation = await _dbContext.HardwareVaultActivations
                .FirstOrDefaultAsync(x => x.VaultId == vaultId && x.Status == HardwareVaultActivationStatus.Pending);

            if (existActivation != null)
            {
                existActivation.Status = HardwareVaultActivationStatus.Canceled;
                _dbContext.HardwareVaultActivations.Update(existActivation);
                await _dbContext.SaveChangesAsync();
            }

            var vaultActivation = new HardwareVaultActivation()
            {
                VaultId = vaultId,
                AcivationCode = _dataProtectionService.Encrypt(new Random().Next(100000, 999999).ToString()),
                CreatedAt = DateTime.UtcNow,
                Status = HardwareVaultActivationStatus.Pending
            };

            var result = _dbContext.HardwareVaultActivations.Add(vaultActivation);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task ChangeVaultActivationStatusAsync(string vaultId, HardwareVaultActivationStatus status)
        {
            if (string.IsNullOrWhiteSpace(vaultId))
            {
                throw new ArgumentNullException(nameof(vaultId));
            }

            var vaultActivation = await _dbContext.HardwareVaultActivations
                .FirstOrDefaultAsync(x => x.VaultId == vaultId && x.Status == HardwareVaultActivationStatus.Pending);

            if (vaultActivation != null)
            {
                vaultActivation.Status = status;
                _dbContext.HardwareVaultActivations.Update(vaultActivation);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<string> GetVaultActivationCodeAsync(string vaultId)
        {
            if (string.IsNullOrWhiteSpace(vaultId))
            {
                throw new ArgumentNullException(nameof(vaultId));
            }

            var vaultActivation = await _dbContext.HardwareVaultActivations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.VaultId == vaultId && x.Status == HardwareVaultActivationStatus.Pending);

            if (vaultActivation == null)
            {
                throw new HESException(HESCode.ActivationCodeNotFound);
            }

            return _dataProtectionService.Decrypt(vaultActivation.AcivationCode);
        }

        public async Task ActivateVaultAsync(string vaultId)
        {
            if (string.IsNullOrWhiteSpace(vaultId))
            {
                throw new ArgumentNullException(nameof(vaultId));
            }

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
            {
                throw new HESException(HESCode.HardwareVaultNotFound);
            }

            if (vault.Status != VaultStatus.Locked)
            {
                throw new Exception($"Vault {vaultId} status ({vault.Status}) is not allowed to execute this operation");
            }

            vault.Status = VaultStatus.Suspended;
            vault.StatusReason = VaultStatusReason.None;
            vault.StatusDescription = null;
            vault.IsStatusApplied = false;

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await CreateVaultActivationAsync(vaultId);
                await UpdateVaultAsync(vault);
                transactionScope.Complete();
            }
        }

        public async Task SuspendVaultAsync(string vaultId, string description)
        {
            if (string.IsNullOrWhiteSpace(vaultId))
            {
                throw new ArgumentNullException(nameof(vaultId));
            }

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
            {
                throw new HESException(HESCode.HardwareVaultNotFound);
            }

            if (vault.Status != VaultStatus.Active)
            {
                throw new Exception($"Vault {vaultId} status ({vault.Status}) is not allowed to execute this operation");
            }

            vault.Status = VaultStatus.Suspended;
            vault.StatusReason = VaultStatusReason.None;
            vault.StatusDescription = description;
            vault.IsStatusApplied = false;

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await CreateVaultActivationAsync(vaultId);
                await UpdateVaultAsync(vault);
                transactionScope.Complete();
            }
        }

        public async Task VaultCompromisedAsync(string vaultId, VaultStatusReason reason, string description)
        {
            if (vaultId == null)
            {
                throw new ArgumentNullException(nameof(vaultId));
            }

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
            {
                throw new HESException(HESCode.HardwareVaultNotFound);
            }

            string employeeId = null;
            if (vault.Employee.HardwareVaults.Count == 1)
            {
                employeeId = vault.EmployeeId;
            }

            vault.EmployeeId = null;
            vault.MasterPassword = null;
            vault.NeedSync = false;
            vault.Status = VaultStatus.Compromised;
            vault.StatusReason = reason;
            vault.StatusDescription = description;

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await UpdateVaultAsync(vault);
                await ChangeVaultActivationStatusAsync(vaultId, HardwareVaultActivationStatus.Canceled);
                await _hardwareVaultTaskService.DeleteTasksByVaultIdAsync(vaultId);

                if (employeeId != null)
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

                await _workstationService.DeleteWorkstationHardwareVaultPairsByVaultIdAsync(vaultId);

                transactionScope.Complete();
            }
        }

        #endregion

        #region Vault Profile

        public async Task<HardwareVaultProfile> GetDefaultProfile()
        {
            return await _dbContext.HardwareVaultProfiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == "default");
        }

        public async Task<HardwareVaultProfile> GetProfileByIdAsync(string profileId)
        {
            return await _dbContext.HardwareVaultProfiles
                .Include(d => d.HardwareVaults)
                .FirstOrDefaultAsync(m => m.Id == profileId);
        }

        public async Task<List<HardwareVaultProfile>> GetHardwareVaultProfilesAsync(DataLoadingOptions<HardwareVaultProfileFilter> dataLoadingOptions)
        {
            return await HardwareVaultProfileQuery(dataLoadingOptions).Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetHardwareVaultProfileCountAsync(DataLoadingOptions<HardwareVaultProfileFilter> dataLoadingOptions)
        {
            return await HardwareVaultProfileQuery(dataLoadingOptions).CountAsync();
        }

        private IQueryable<HardwareVaultProfile> HardwareVaultProfileQuery(DataLoadingOptions<HardwareVaultProfileFilter> dataLoadingOptions)
        {
            var query = _dbContext.HardwareVaultProfiles
                .Include(x => x.HardwareVaults)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Name != null)
                {
                    query = query.Where(w => w.Name.Contains(dataLoadingOptions.Filter.Name));
                }
                if (dataLoadingOptions.Filter.CreatedAtFrom != null)
                {
                    query = query.Where(x => x.CreatedAt >= dataLoadingOptions.Filter.CreatedAtFrom.Value.Date.ToUniversalTime());
                }
                if (dataLoadingOptions.Filter.CreatedAtTo != null)
                {
                    query = query.Where(x => x.CreatedAt <= dataLoadingOptions.Filter.CreatedAtTo.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime());
                }
                if (dataLoadingOptions.Filter.UpdatedAtFrom != null)
                {
                    query = query.Where(x => x.UpdatedAt >= dataLoadingOptions.Filter.UpdatedAtFrom.Value.Date.ToUniversalTime());
                }
                if (dataLoadingOptions.Filter.UpdatedAtTo != null)
                {
                    query = query.Where(x => x.UpdatedAt <= dataLoadingOptions.Filter.UpdatedAtTo.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime());
                }
                if (dataLoadingOptions.Filter.HardwareVaultsCount != null)
                {
                    query = query.Where(w => w.HardwareVaults.Count == dataLoadingOptions.Filter.HardwareVaultsCount);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Name.Contains(dataLoadingOptions.SearchText) ||
                                    x.HardwareVaults.Count.ToString().Contains(dataLoadingOptions.SearchText));
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(HardwareVaultProfile.Name):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name);
                    break;
                case nameof(HardwareVaultProfile.CreatedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt);
                    break;
                case nameof(HardwareVaultProfile.UpdatedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.UpdatedAt) : query.OrderByDescending(x => x.UpdatedAt);
                    break;
                case nameof(HardwareVaultProfile.HardwareVaults):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.HardwareVaults.Count) : query.OrderByDescending(x => x.HardwareVaults.Count);
                    break;
            }

            return query;
        }

        public async Task<List<string>> GetVaultIdsByProfileTaskAsync()
        {
            return await _hardwareVaultTaskService
                .TaskQuery()
                .Where(x => x.Operation == TaskOperation.Profile)
                .Select(x => x.HardwareVaultId)
                .ToListAsync();
        }

        public async Task<List<HardwareVaultProfile>> GetProfilesAsync()
        {
            return await _dbContext.HardwareVaultProfiles
                .Include(d => d.HardwareVaults)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<HardwareVaultProfile> CreateProfileAsync(HardwareVaultProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            var exist = await _dbContext.HardwareVaultProfiles
                .Where(d => d.Name == profile.Name)
                .AnyAsync();

            if (exist)
            {
                throw new HESException(HESCode.ProfileNameAlreadyInUse);
            }

            profile.CreatedAt = DateTime.UtcNow;
            var result = _dbContext.HardwareVaultProfiles.Add(profile);
            await _dbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task EditProfileAsync(HardwareVaultProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            var exist = await _dbContext.HardwareVaultProfiles
               .Where(d => d.Name == profile.Name && d.Id != profile.Id)
               .AnyAsync();

            if (exist)
            {
                throw new HESException(HESCode.ProfileNameAlreadyInUse);
            }

            profile.UpdatedAt = DateTime.UtcNow;

            var vaults = await _dbContext.HardwareVaults
                .Where(x => x.HardwareVaultProfileId == profile.Id && (x.Status == VaultStatus.Active || x.Status == VaultStatus.Locked || x.Status == VaultStatus.Suspended))
                .ToListAsync();

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                _dbContext.HardwareVaultProfiles.Update(profile);
                await _dbContext.SaveChangesAsync();

                foreach (var vault in vaults)
                {
                    await _hardwareVaultTaskService.AddProfileAsync(vault);
                }

                transactionScope.Complete();
            }
        }

        public async Task DeleteProfileAsync(string profileId)
        {
            if (profileId == null)
            {
                throw new ArgumentNullException(nameof(profileId));
            }

            if (profileId == "default")
            {
                throw new HESException(HESCode.CannotDeleteDefaultProfile);
            }

            var deviceAccessProfile = await _dbContext.HardwareVaultProfiles.FindAsync(profileId);
            if (deviceAccessProfile == null)
            {
                throw new HESException(HESCode.HardwareVaultProfileNotFound);
            }

            if (deviceAccessProfile.HardwareVaults.Count > 0)
            {
                throw new Exception("Cannot delete a profile if setted to a hardware vaults");
            }

            _dbContext.HardwareVaultProfiles.Remove(deviceAccessProfile);
            await _dbContext.SaveChangesAsync();
        }

        public async Task ChangeVaultProfileAsync(string vaultId, string profileId)
        {
            if (string.IsNullOrWhiteSpace(vaultId))
            {
                throw new ArgumentNullException(nameof(vaultId));
            }

            if (string.IsNullOrWhiteSpace(profileId))
            {
                throw new ArgumentNullException(nameof(profileId));
            }

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
            {
                throw new HESException(HESCode.HardwareVaultNotFound);
            }

            if (vault.Status != VaultStatus.Active)
            {
                throw new Exception($"Vault {vaultId} status ({vault.Status}) is not allowed to execute this operation");
            }

            var profile = await _dbContext.HardwareVaultProfiles.FindAsync(profileId);
            if (profile == null)
            {
                throw new HESException(HESCode.HardwareVaultProfileNotFound);
            }

            vault.HardwareVaultProfileId = profileId;

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await UpdateVaultAsync(vault);
                await _hardwareVaultTaskService.AddProfileAsync(vault);
                transactionScope.Complete();
            }
        }

        public async Task<AccessParams> GetAccessParamsAsync(string vaultId)
        {
            var vault = await GetVaultByIdAsync(vaultId);

            return new AccessParams()
            {
                MasterKey_Bond = vault.HardwareVaultProfile.MasterKeyPairing,
                MasterKey_Connect = vault.HardwareVaultProfile.MasterKeyConnection,
                MasterKey_Channel = vault.HardwareVaultProfile.MasterKeyStorageAccess,

                Button_Bond = vault.HardwareVaultProfile.ButtonPairing,
                Button_Connect = vault.HardwareVaultProfile.ButtonConnection,
                Button_Channel = vault.HardwareVaultProfile.ButtonStorageAccess,

                Pin_Bond = vault.HardwareVaultProfile.PinPairing,
                Pin_Connect = vault.HardwareVaultProfile.PinConnection,
                Pin_Channel = vault.HardwareVaultProfile.PinStorageAccess,

                PinMinLength = vault.HardwareVaultProfile.PinLength,
                PinMaxTries = vault.HardwareVaultProfile.PinTryCount,
                PinExpirationPeriod = vault.HardwareVaultProfile.PinExpiration,
                ButtonExpirationPeriod = 0,
                MasterKeyExpirationPeriod = 0
            };
        }

        public void UnchangedProfile(HardwareVaultProfile profile)
        {
            _dbContext.Unchanged(profile);
        }

        #endregion
    }
}