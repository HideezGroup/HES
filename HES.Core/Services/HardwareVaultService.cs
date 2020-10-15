using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API.HardwareVault;
using HES.Core.Models.Web;
using HES.Core.Models.Web.HardwareVaults;
using Hideez.SDK.Communication;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.HES.DTO;
using Hideez.SDK.Communication.Remote;
using Hideez.SDK.Communication.Security;
using Hideez.SDK.Communication.Utils;
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
    public class HardwareVaultService : IHardwareVaultService, IDisposable
    {
        private readonly IAsyncRepository<HardwareVault> _hardwareVaultRepository;
        private readonly IAsyncRepository<HardwareVaultActivation> _hardwareVaultActivationRepository;
        private readonly IAsyncRepository<HardwareVaultProfile> _hardwareVaultProfileRepository;
        private readonly ILicenseService _licenseService;
        private readonly IHardwareVaultTaskService _hardwareVaultTaskService;
        private readonly IAccountService _accountService;
        private readonly IWorkstationService _workstationService;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IDataProtectionService _dataProtectionService;
        private readonly IHttpClientFactory _httpClientFactory;

        public HardwareVaultService(IAsyncRepository<HardwareVault> hardwareVaultRepository,
                                    IAsyncRepository<HardwareVaultActivation> hardwareVaultActivationRepository,
                                    IAsyncRepository<HardwareVaultProfile> hardwareVaultProfileRepository,
                                    ILicenseService licenseService,
                                    IHardwareVaultTaskService hardwareVaultTaskService,
                                    IAccountService accountService,
                                    IWorkstationService workstationService,
                                    IAppSettingsService appSettingsService,
                                    IDataProtectionService dataProtectionService,
                                    IHttpClientFactory httpClientFactory)
        {
            _hardwareVaultRepository = hardwareVaultRepository;
            _hardwareVaultActivationRepository = hardwareVaultActivationRepository;
            _hardwareVaultProfileRepository = hardwareVaultProfileRepository;
            _licenseService = licenseService;
            _hardwareVaultTaskService = hardwareVaultTaskService;
            _accountService = accountService;
            _workstationService = workstationService;
            _appSettingsService = appSettingsService;
            _dataProtectionService = dataProtectionService;
            _httpClientFactory = httpClientFactory;
        }

        #region Vault

        public IQueryable<HardwareVault> VaultQuery()
        {
            return _hardwareVaultRepository.Query();
        }

        public async Task<HardwareVault> GetVaultByIdAsync(string vaultId)
        {
            return await _hardwareVaultRepository
                .Query()
                .Include(d => d.Employee.Department.Company)
                .Include(d => d.Employee.HardwareVaults)
                .Include(d => d.Employee.SoftwareVaults)
                .Include(d => d.HardwareVaultProfile)
                .Include(d => d.HardwareVaultTasks)
                .FirstOrDefaultAsync(d => d.Id == vaultId);
        }

        public async Task<List<HardwareVault>> GetVaultsWithoutLicenseAsync()
        {
            return await _hardwareVaultRepository
                    .Query()
                    .Where(x => x.LicenseStatus == VaultLicenseStatus.None ||
                                x.LicenseStatus == VaultLicenseStatus.Expired)
                    .AsNoTracking()
                    .ToListAsync();
        }

        public async Task<List<HardwareVault>> GetVaultsWithLicenseAsync()
        {
            return await _hardwareVaultRepository
                    .Query()
                     .Where(x => x.LicenseStatus != VaultLicenseStatus.None &&
                            x.LicenseStatus != VaultLicenseStatus.Expired)
                    .AsNoTracking()
                    .ToListAsync();
        }

        public async Task<List<HardwareVault>> GetVaultsAsync(DataLoadingOptions<HardwareVaultFilter> dataLoadingOptions)
        {
            var query = _hardwareVaultRepository
                .Query()
                .Include(d => d.Employee.Department.Company)
                .Include(d => d.HardwareVaultProfile)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Id != null)
                {
                    query = query.Where(w => w.Id.Contains(dataLoadingOptions.Filter.Id, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.MAC != null)
                {
                    query = query.Where(w => w.MAC.Contains(dataLoadingOptions.Filter.MAC, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Model != null)
                {
                    query = query.Where(w => w.Model.Contains(dataLoadingOptions.Filter.Model, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.RFID != null)
                {
                    query = query.Where(w => w.RFID.Contains(dataLoadingOptions.Filter.RFID, StringComparison.OrdinalIgnoreCase));
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
                    query = query.Where(w => w.Firmware.Contains(dataLoadingOptions.Filter.Firmware, StringComparison.OrdinalIgnoreCase));
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
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.Filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(x => x.Employee.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(x => x.Employee.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
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

                query = query.Where(x => x.Id.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.MAC.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Battery.ToString().Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Firmware.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.HardwareVaultProfile.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Model.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.RFID.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
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

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetVaultsCountAsync(DataLoadingOptions<HardwareVaultFilter> dataLoadingOptions)
        {
            var query = _hardwareVaultRepository
                .Query()
                .Include(d => d.HardwareVaultProfile)
                .Include(d => d.Employee.Department)
                .Include(d => d.Employee.Department.Company)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Id != null)
                {
                    query = query.Where(w => w.Id.Contains(dataLoadingOptions.Filter.Id, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.MAC != null)
                {
                    query = query.Where(w => w.MAC.Contains(dataLoadingOptions.Filter.MAC, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Model != null)
                {
                    query = query.Where(w => w.Model.Contains(dataLoadingOptions.Filter.Model, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.RFID != null)
                {
                    query = query.Where(w => w.RFID.Contains(dataLoadingOptions.Filter.RFID, StringComparison.OrdinalIgnoreCase));
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
                    query = query.Where(w => w.Firmware.Contains(dataLoadingOptions.Filter.Firmware, StringComparison.OrdinalIgnoreCase));
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
                    query = query.Where(x => (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.Filter.Employee, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Company != null)
                {
                    query = query.Where(x => x.Employee.Department.Company.Name.Contains(dataLoadingOptions.Filter.Company, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.Department != null)
                {
                    query = query.Where(x => x.Employee.Department.Name.Contains(dataLoadingOptions.Filter.Department, StringComparison.OrdinalIgnoreCase));
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

                query = query.Where(x => x.Id.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.MAC.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Battery.ToString().Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Firmware.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.HardwareVaultProfile.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    (x.Employee.FirstName + " " + x.Employee.LastName).Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Company.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Employee.Department.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Model.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.RFID.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public async Task ImportVaultsAsync()
        {
            var licensing = await _appSettingsService.GetLicensingSettingsAsync();

            if (licensing == null)
                throw new Exception("Api Key is empty.");

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

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultRepository.AddRangeAsync(hardwareVaultsToImport);
                await _licenseService.AddOrderRangeAsync(licenseOrdersToImport);
                await _licenseService.AddHardwareVaultLicenseRangeAsync(hardwareVaultLicensesToImport);
                transactionScope.Complete();
            }

            // Without transaction
            await _licenseService.UpdateHardwareVaultsLicenseStatusAsync();
        }

        public Task UnchangedVaultAsync(HardwareVault vault)
        {
            return _hardwareVaultRepository.UnchangedAsync(vault);
        }

        public async Task UpdateRfidAsync(HardwareVault vault)
        {
            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.RFID) });
        }

        public async Task UpdateNeedSyncAsync(HardwareVault vault, bool needSync)
        {
            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

            vault.NeedSync = needSync;
            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.NeedSync) });
        }

        public async Task UpdateNeedSyncAsync(List<HardwareVault> vaults, bool needSync)
        {
            if (vaults == null)
                throw new ArgumentNullException(nameof(vaults));

            vaults.ForEach(x => x.NeedSync = needSync);    
            await _hardwareVaultRepository.UpdateOnlyPropAsync(vaults, new string[] { nameof(HardwareVault.NeedSync) });
        }

        public async Task UpdateTimestampAsync(HardwareVault vault, uint timestamp)
        {
            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

            vault.Timestamp = timestamp;
            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.Timestamp) });
        }

        public async Task<HardwareVault> UpdateVaultAsync(HardwareVault vault)
        {
            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

            return await _hardwareVaultRepository.UpdateAsync(vault);
        }

        public async Task UpdateVaultInfoAsync(HwVaultInfoFromClientDto dto)
        {
            var vault = await _hardwareVaultRepository.GetByIdAsync(dto.VaultSerialNo);

            if (vault == null)
                throw new HideezException(HideezErrorCode.HesDeviceNotFound);

            vault.Timestamp = dto.StorageTimestamp;
            vault.Battery = dto.Battery;
            vault.Firmware = dto.FirmwareVersion;
            vault.LastSynced = DateTime.UtcNow;

            var properties = new string[]
            {
                nameof(HardwareVault.Timestamp),
                nameof(HardwareVault.Battery),
                nameof(HardwareVault.Firmware),
                nameof(HardwareVault.LastSynced)
            };

            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, properties);
        }

        public async Task SetReadyStatusAsync(HardwareVault vault)
        {
            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

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

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultRepository.UpdateAsync(vault);   
                await ChangeVaultActivationStatusAsync(vault.Id, HardwareVaultActivationStatus.Canceled);

                transactionScope.Complete();
            }
        }

        public async Task SetActiveStatusAsync(HardwareVault vault)
        {
            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

            vault.Status = VaultStatus.Active;
            vault.StatusReason = VaultStatusReason.None;
            vault.StatusDescription = null;

            var properties = new string[]
            {
                nameof(HardwareVault.Status),
                nameof(HardwareVault.StatusReason),
                nameof(HardwareVault.StatusDescription)
            };

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, properties);
                await ChangeVaultActivationStatusAsync(vault.Id, HardwareVaultActivationStatus.Activated);

                transactionScope.Complete();
            }
        }

        public async Task SetLockedStatusAsync(HardwareVault vault)
        {
            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

            vault.Status = VaultStatus.Locked;

            var properties = new string[]
            {
                nameof(HardwareVault.Status)
            };

            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, properties);
        }

        public async Task SetVaultStatusAppliedAsync(HardwareVault vault)
        {
            if (vault == null)
                throw new ArgumentNullException(nameof(vault));

            vault.IsStatusApplied = true;

            var properties = new string[]
            {
                nameof(HardwareVault.IsStatusApplied)
            };

            await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, properties);
        }

        public async Task<HardwareVaultActivation> CreateVaultActivationAsync(string vaultId)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var existActivation = await _hardwareVaultActivationRepository
                .Query()
                .FirstOrDefaultAsync(x => x.VaultId == vaultId && x.Status == HardwareVaultActivationStatus.Pending);

            if (existActivation != null)
            {
                existActivation.Status = HardwareVaultActivationStatus.Canceled;
                await _hardwareVaultActivationRepository.UpdateAsync(existActivation);
            }

            var vaultActivation = new HardwareVaultActivation()
            {
                VaultId = vaultId,
                AcivationCode = _dataProtectionService.Encrypt(new Random().Next(100000, 999999).ToString()),
                CreatedAt = DateTime.UtcNow,
                Status = HardwareVaultActivationStatus.Pending
            };

            return await _hardwareVaultActivationRepository.AddAsync(vaultActivation);
        }

        public async Task ChangeVaultActivationStatusAsync(string vaultId, HardwareVaultActivationStatus status)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vaultActivation = await _hardwareVaultActivationRepository
                .Query()
                .FirstOrDefaultAsync(x => x.VaultId == vaultId && x.Status == HardwareVaultActivationStatus.Pending);

            if (vaultActivation != null)
            {
                vaultActivation.Status = status;
                await _hardwareVaultActivationRepository.UpdateAsync(vaultActivation);
            }
        }

        public async Task<string> GetVaultActivationCodeAsync(string vaultId)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vaultActivation = await _hardwareVaultActivationRepository
                .Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.VaultId == vaultId && x.Status == HardwareVaultActivationStatus.Pending);

            if (vaultActivation == null)
                throw new Exception($"Activation code not found for {vaultId}");

            return _dataProtectionService.Decrypt(vaultActivation.AcivationCode);
        }

        public async Task ActivateVaultAsync(string vaultId)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            if (vault.Status != VaultStatus.Locked)
                throw new Exception($"Vault {vaultId} status ({vault.Status}) is not allowed to execute this operation");

            vault.Status = VaultStatus.Suspended;
            vault.StatusReason = VaultStatusReason.None;
            vault.StatusDescription = null;
            vault.IsStatusApplied = false;

            var properties = new string[]
            {
                nameof(HardwareVault.Status),
                nameof(HardwareVault.StatusReason),
                nameof(HardwareVault.StatusDescription),
                nameof(HardwareVault.IsStatusApplied)
            };

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await CreateVaultActivationAsync(vaultId);
                await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, properties);

                transactionScope.Complete();
            }
        }

        public async Task SuspendVaultAsync(string vaultId, string description)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            if (vault.Status != VaultStatus.Active)
                throw new Exception($"Vault {vaultId} status ({vault.Status}) is not allowed to execute this operation");

            vault.Status = VaultStatus.Suspended;
            vault.StatusReason = VaultStatusReason.None;
            vault.StatusDescription = description;
            vault.IsStatusApplied = false;

            var properties = new string[]
            {
                nameof(HardwareVault.Status),
                nameof(HardwareVault.StatusReason),
                nameof(HardwareVault.StatusDescription),
                nameof(HardwareVault.IsStatusApplied)
            };

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await CreateVaultActivationAsync(vaultId);
                await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, properties);

                transactionScope.Complete();
            }
        }

        public async Task VaultCompromisedAsync(string vaultId, VaultStatusReason reason, string description)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            string employeeId = null;
            if (vault.Employee.HardwareVaults.Count == 1)
                employeeId = vault.EmployeeId;

            vault.EmployeeId = null;
            vault.MasterPassword = null;
            vault.NeedSync = false;
            vault.Status = VaultStatus.Compromised;
            vault.StatusReason = reason;
            vault.StatusDescription = description;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultRepository.UpdateAsync(vault);
                await ChangeVaultActivationStatusAsync(vaultId, HardwareVaultActivationStatus.Canceled);
                await _hardwareVaultTaskService.DeleteTasksByVaultIdAsync(vaultId);

                if (employeeId != null)
                    await _accountService.DeleteAccountsByEmployeeIdAsync(employeeId);

                await _workstationService.DeleteProximityByVaultIdAsync(vaultId);

                transactionScope.Complete();
            }
        }

        #endregion

        #region Vault Profile

        public IQueryable<HardwareVaultProfile> ProfileQuery()
        {
            return _hardwareVaultProfileRepository.Query();
        }

        public async Task<HardwareVaultProfile> GetProfileByIdAsync(string profileId)
        {
            return await _hardwareVaultProfileRepository
                .Query()
                .Include(d => d.HardwareVaults)
                .FirstOrDefaultAsync(m => m.Id == profileId);
        }

        public async Task<List<HardwareVaultProfile>> GetHardwareVaultProfilesAsync(DataLoadingOptions<HardwareVaultProfileFilter> dataLoadingOptions)
        {
            var query = _hardwareVaultProfileRepository
                .Query()
                .Include(x => x.HardwareVaults)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Name != null)
                {
                    query = query.Where(w => w.Name.Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.CreatedAtFrom != null)
                {
                    query = query.Where(x => x.CreatedAt >= dataLoadingOptions.Filter.CreatedAtFrom.Value.Date.ToUniversalTime());
                }
                if (dataLoadingOptions.Filter.CreateddAtTo != null)
                {
                    query = query.Where(x => x.CreatedAt <= dataLoadingOptions.Filter.CreateddAtTo.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime());
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

                query = query.Where(x => x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.HardwareVaults.Count.ToString().Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
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

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetHardwareVaultProfileCountAsync(DataLoadingOptions<HardwareVaultProfileFilter> dataLoadingOptions)
        {
            var query = _hardwareVaultProfileRepository
                .Query()
                .Include(x => x.HardwareVaults)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (dataLoadingOptions.Filter.Name != null)
                {
                    query = query.Where(w => w.Name.Contains(dataLoadingOptions.Filter.Name, StringComparison.OrdinalIgnoreCase));
                }
                if (dataLoadingOptions.Filter.CreatedAtFrom != null)
                {
                    query = query.Where(x => x.CreatedAt >= dataLoadingOptions.Filter.CreatedAtFrom.Value.Date.ToUniversalTime());
                }
                if (dataLoadingOptions.Filter.CreateddAtTo != null)
                {
                    query = query.Where(x => x.CreatedAt <= dataLoadingOptions.Filter.CreateddAtTo.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59).ToUniversalTime());
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

                query = query.Where(x => x.Name.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.HardwareVaults.Count.ToString().Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
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
            return await _hardwareVaultProfileRepository
                .Query()
                .Include(d => d.HardwareVaults)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<HardwareVaultProfile> CreateProfileAsync(HardwareVaultProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            var exist = await _hardwareVaultProfileRepository
                .Query()
                .Where(d => d.Name == profile.Name)
                .AnyAsync();

            if (exist)
                throw new AlreadyExistException($"Name {profile.Name} is already taken.");

            profile.CreatedAt = DateTime.UtcNow;
            return await _hardwareVaultProfileRepository.AddAsync(profile);
        }

        public async Task EditProfileAsync(HardwareVaultProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            var exist = await _hardwareVaultProfileRepository
               .Query()
               .Where(d => d.Name == profile.Name && d.Id != profile.Id)
               .AnyAsync();

            if (exist)
                throw new AlreadyExistException($"Name {profile.Name} is already taken.");

            profile.UpdatedAt = DateTime.UtcNow;

            var vaults = await _hardwareVaultRepository
                .Query()
                .Where(x => x.HardwareVaultProfileId == profile.Id && (x.Status == VaultStatus.Active || x.Status == VaultStatus.Locked || x.Status == VaultStatus.Suspended))
                .ToListAsync();

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultProfileRepository.UpdateAsync(profile);

                foreach (var vault in vaults)
                {
                    await _hardwareVaultTaskService.AddProfileAsync(vault);
                }

                transactionScope.Complete();
            }
        }

        public async Task DeleteProfileAsync(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (id == "default")
            {
                throw new Exception("Cannot delete a default profile");
            }

            var deviceAccessProfile = await _hardwareVaultProfileRepository.GetByIdAsync(id);
            if (deviceAccessProfile == null)
            {
                throw new Exception("Profile not found");
            }

            if (deviceAccessProfile.HardwareVaults.Count > 0)
            {
                throw new Exception("Cannot delete a profile if setted to a hardware vaults");
            }

            await _hardwareVaultProfileRepository.DeleteAsync(deviceAccessProfile);
        }

        public async Task ChangeVaultProfileAsync(string vaultId, string profileId)
        {
            if (vaultId == null)
                throw new ArgumentNullException(nameof(vaultId));

            if (profileId == null)
                throw new ArgumentNullException(nameof(profileId));

            var vault = await GetVaultByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"Vault {vaultId} not found");

            if (vault.Status != VaultStatus.Active)
                throw new Exception($"Vault {vaultId} status ({vault.Status}) is not allowed to execute this operation");

            var profile = await _hardwareVaultProfileRepository.GetByIdAsync(profileId);
            if (profile == null)
                throw new Exception("Vault profile not found");

            vault.HardwareVaultProfileId = profileId;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.HardwareVaultProfileId) });
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

        public async Task UnchangedProfileAsync(HardwareVaultProfile profile)
        {
            await _hardwareVaultProfileRepository.UnchangedAsync(profile);
        }

        #endregion

        public void Dispose()
        {
            _hardwareVaultRepository.Dispose();
            _hardwareVaultActivationRepository.Dispose();
            _hardwareVaultProfileRepository.Dispose();
            _licenseService.Dispose();
            _hardwareVaultTaskService.Dispose();
            _accountService.Dispose();
            _workstationService.Dispose();
            _appSettingsService.Dispose();
        }
    }
}