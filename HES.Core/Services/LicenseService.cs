using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.API.License;
using HES.Core.Models.Web;
using HES.Core.Models.Web.DataTableComponent;
using HES.Core.Models.Web.LicenseOrders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class LicenseService : ILicenseService, IDisposable
    {
        private readonly IAsyncRepository<LicenseOrder> _licenseOrderRepository;
        private readonly IAsyncRepository<HardwareVaultLicense> _hardwareVaultLicenseRepository;
        private readonly IAsyncRepository<HardwareVault> _hardwareVaultRepository;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LicenseService> _logger;

        public LicenseService(IAsyncRepository<LicenseOrder> licenseOrderRepository,
                                   IAsyncRepository<HardwareVaultLicense> hardwareVaultLicenseRepository,
                                   IAsyncRepository<HardwareVault> hardwareVaultRepository,
                                   IAppSettingsService appSettingsService,
                                   IApplicationUserService applicationUserService,
                                   IHttpClientFactory httpClientFactory,
                                   ILogger<LicenseService> logger)
        {
            _licenseOrderRepository = licenseOrderRepository;
            _hardwareVaultLicenseRepository = hardwareVaultLicenseRepository;
            _hardwareVaultRepository = hardwareVaultRepository;
            _appSettingsService = appSettingsService;
            _applicationUserService = applicationUserService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        #region HttpClient

        private async Task<HttpClient> CreateHttpClient()
        {
            var licensing = await _appSettingsService.GetLicensingSettingsAsync();

            if (licensing == null)
                throw new Exception("Api Key is empty.");

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(licensing.ApiAddress);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        private async Task<HttpResponseMessage> HttpClientPostOrderAsync(LicenseOrderDto licenseOrderDto)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(licenseOrderDto), Encoding.UTF8, "application/json");
            var path = $"api/Licenses/CreateLicenseOrder";
            var client = await CreateHttpClient();
            return await client.PostAsync(path, stringContent);
        }

        private async Task<HttpResponseMessage> HttpClientGetLicenseOrderStatusAsync(string orderId)
        {
            var path = $"api/Licenses/GetLicenseOrderStatus/{orderId}";
            var client = await CreateHttpClient();
            return await client.GetAsync(path);
        }

        private async Task<HttpResponseMessage> HttpClientGetDeviceLicensesAsync(string orderId)
        {
            var path = $"/api/Licenses/GetDeviceLicenses/{orderId}";
            var client = await CreateHttpClient();
            return await client.GetAsync(path);
        }

        #endregion

        #region Order

        public async Task<List<LicenseOrder>> GetLicenseOrdersAsync()
        {
            return await _licenseOrderRepository.Query().ToListAsync();
        }

        public async Task<List<LicenseOrder>> GetLicenseOrdersAsync(DataLoadingOptions<LicenseOrderFilter> dataLoadingOptions)
        {
            var query = _licenseOrderRepository
                .Query()
                .Include(x => x.HardwareVaultLicenses)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Note))
                {
                    query = query.Where(x => x.Note.Contains(dataLoadingOptions.Filter.Note, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.ContactEmail))
                {
                    query = query.Where(w => w.ContactEmail.Contains(dataLoadingOptions.Filter.ContactEmail, StringComparison.OrdinalIgnoreCase));
                }
                if (!dataLoadingOptions.Filter.ProlongLicense != null)
                {
                    query = query.Where(w => w.ProlongExistingLicenses == dataLoadingOptions.Filter.ProlongLicense);
                }
                if (dataLoadingOptions.Filter.OrderStatus != null)
                {
                    query = query.Where(x => x.OrderStatus == dataLoadingOptions.Filter.OrderStatus);
                }
                if (dataLoadingOptions.Filter.LicenseStartDateStart != null)
                {
                    query = query.Where(w => w.StartDate >= dataLoadingOptions.Filter.LicenseStartDateStart);
                }
                if (dataLoadingOptions.Filter.LicenseStartDateEnd != null)
                {
                    query = query.Where(x => x.StartDate <= dataLoadingOptions.Filter.LicenseStartDateEnd);
                }
                if (dataLoadingOptions.Filter.LicenseEndDateStart != null)
                {
                    query = query.Where(w => w.EndDate >= dataLoadingOptions.Filter.LicenseEndDateStart);
                }
                if (dataLoadingOptions.Filter.LicenseEndDateEnd != null)
                {
                    query = query.Where(x => x.EndDate <= dataLoadingOptions.Filter.LicenseEndDateEnd);
                }
                if (dataLoadingOptions.Filter.CreatedDateStart != null)
                {
                    query = query.Where(w => w.CreatedAt >= dataLoadingOptions.Filter.CreatedDateStart);
                }
                if (dataLoadingOptions.Filter.CreatedDateEnd != null)
                {
                    query = query.Where(x => x.CreatedAt <= dataLoadingOptions.Filter.CreatedDateEnd);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.ContactEmail.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Note.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            // Sort Direction
            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(LicenseOrder.ContactEmail):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.ContactEmail) : query.OrderByDescending(x => x.ContactEmail);
                    break;
                case nameof(LicenseOrder.Note):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Note) : query.OrderByDescending(x => x.Note);
                    break;
                case nameof(LicenseOrder.ProlongExistingLicenses):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.ProlongExistingLicenses) : query.OrderByDescending(x => x.ProlongExistingLicenses);
                    break;
                case nameof(LicenseOrder.StartDate):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.StartDate) : query.OrderByDescending(x => x.StartDate);
                    break;
                case nameof(LicenseOrder.EndDate):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.EndDate) : query.OrderByDescending(x => x.EndDate);
                    break;
                case nameof(LicenseOrder.CreatedAt):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt);
                    break;
                case nameof(LicenseOrder.OrderStatus):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.OrderStatus) : query.OrderByDescending(x => x.OrderStatus);
                    break;
            }

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetLicenseOrdersCountAsync(DataLoadingOptions<LicenseOrderFilter> dataLoadingOptions)
        {
            var query = _licenseOrderRepository
                .Query()
                .Include(x => x.HardwareVaultLicenses)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Note))
                {
                    query = query.Where(x => x.Note.Contains(dataLoadingOptions.Filter.Note, StringComparison.OrdinalIgnoreCase));
                }
                if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.ContactEmail))
                {
                    query = query.Where(w => w.ContactEmail.Contains(dataLoadingOptions.Filter.ContactEmail, StringComparison.OrdinalIgnoreCase));
                }
                if (!dataLoadingOptions.Filter.ProlongLicense != null)
                {
                    query = query.Where(w => w.ProlongExistingLicenses == dataLoadingOptions.Filter.ProlongLicense);
                }
                if (dataLoadingOptions.Filter.OrderStatus != null)
                {
                    query = query.Where(x => x.OrderStatus == dataLoadingOptions.Filter.OrderStatus);
                }
                if (dataLoadingOptions.Filter.LicenseStartDateStart != null)
                {
                    query = query.Where(w => w.StartDate >= dataLoadingOptions.Filter.LicenseStartDateStart);
                }
                if (dataLoadingOptions.Filter.LicenseStartDateEnd != null)
                {
                    query = query.Where(x => x.StartDate <= dataLoadingOptions.Filter.LicenseStartDateEnd);
                }
                if (dataLoadingOptions.Filter.LicenseEndDateStart != null)
                {
                    query = query.Where(w => w.EndDate >= dataLoadingOptions.Filter.LicenseEndDateStart);
                }
                if (dataLoadingOptions.Filter.LicenseEndDateEnd != null)
                {
                    query = query.Where(x => x.EndDate <= dataLoadingOptions.Filter.LicenseEndDateEnd);
                }
                if (dataLoadingOptions.Filter.CreatedDateStart != null)
                {
                    query = query.Where(w => w.CreatedAt >= dataLoadingOptions.Filter.CreatedDateStart);
                }
                if (dataLoadingOptions.Filter.CreatedDateEnd != null)
                {
                    query = query.Where(x => x.CreatedAt <= dataLoadingOptions.Filter.CreatedDateEnd);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.ContactEmail.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.Note.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public async Task<LicenseOrder> GetLicenseOrderByIdAsync(string orderId)
        {
            return await _licenseOrderRepository
                .Query()
                .Include(x => x.HardwareVaultLicenses)
                .FirstOrDefaultAsync(x => x.Id == orderId);
        }

        public async Task<LicenseOrder> CreateOrderAsync(LicenseOrder licenseOrder, List<HardwareVault> hardwareVaults)
        {
            if (licenseOrder == null)
                throw new ArgumentNullException(nameof(licenseOrder));

            LicenseOrder createdOrder;

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                createdOrder = await _licenseOrderRepository.AddAsync(licenseOrder);
                await AddOrUpdateHardwareVaultEmptyLicensesAsync(createdOrder.Id, hardwareVaults.Select(x => x.Id).ToList());
                transactionScope.Complete();
            }

            return createdOrder;
        }

        public async Task<LicenseOrder> EditOrderAsync(LicenseOrder licenseOrder, List<HardwareVault> hardwareVaults)
        {
            if (licenseOrder == null)
                throw new ArgumentNullException(nameof(licenseOrder));

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _licenseOrderRepository.UpdateAsync(licenseOrder);
                await AddOrUpdateHardwareVaultEmptyLicensesAsync(licenseOrder.Id, hardwareVaults.Select(x => x.Id).ToList());
                transactionScope.Complete();
            }

            return licenseOrder;
        }

        public async Task<List<LicenseOrder>> AddOrderRangeAsync(List<LicenseOrder> licenseOrders)
        {
            if (licenseOrders == null)
                throw new ArgumentNullException(nameof(licenseOrders));

            return await _licenseOrderRepository.AddRangeAsync(licenseOrders) as List<LicenseOrder>;
        }

        public async Task DeleteOrderAsync(LicenseOrder licenseOrder)
        {
            if (licenseOrder == null)
                throw new Exception("Order does not exist.");

            await _licenseOrderRepository.DeleteAsync(licenseOrder);
        }

        public async Task SendOrderAsync(LicenseOrder licenseOrder)
        {
            var vaultLicenses = await GetLicensesByOrderIdAsync(licenseOrder.Id);
            if (vaultLicenses == null)
                throw new Exception("Hardware vault licenses not found.");

            var licensing = await _appSettingsService.GetLicensingSettingsAsync();

            if (licensing == null)
                throw new Exception("Api Key is empty.");

            var licenseOrderDto = new LicenseOrderDto()
            {
                Id = licenseOrder.Id,
                ContactEmail = licenseOrder.ContactEmail,
                CustomerNote = licenseOrder.Note,
                LicenseStartDate = licenseOrder.StartDate,
                LicenseEndDate = licenseOrder.EndDate,
                ProlongExistingLicenses = licenseOrder.ProlongExistingLicenses,
                CustomerId = licensing.ApiKey,
                Devices = vaultLicenses.Select(d => d.HardwareVaultId).ToList()
            };

            var response = await HttpClientPostOrderAsync(licenseOrderDto);
            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new Exception(errorMessage);
            }

            licenseOrder.OrderStatus = LicenseOrderStatus.Sent;
            await _licenseOrderRepository.UpdateOnlyPropAsync(licenseOrder, new string[] { "OrderStatus" });
        }

        public async Task UpdateLicenseOrdersAsync()
        {
            var orders = await GetOpenLicenseOrdersAsync();

            foreach (var order in orders)
            {
                var response = await HttpClientGetLicenseOrderStatusAsync(order.Id);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Response code: {response.StatusCode} Response message: {response.Content.ReadAsStringAsync()}");
                    continue;
                }

                var data = await response.Content.ReadAsStringAsync();
                var status = JsonConvert.DeserializeObject<LicenseOrderStatus>(data);

                // Status has not changed
                if (status == order.OrderStatus)
                    continue;

                if (status == LicenseOrderStatus.Completed)
                    await UpdateHardwareVaultsLicensesAsync(order.Id);

                order.OrderStatus = status;

                await _licenseOrderRepository.UpdateOnlyPropAsync(order, new string[] { "OrderStatus" });
                await _applicationUserService.SendLicenseChangedAsync(order.CreatedAt, status);
            }
        }

        private async Task<List<LicenseOrder>> GetOpenLicenseOrdersAsync()
        {
            return await _licenseOrderRepository
                .Query()
                .Where(x => x.OrderStatus == LicenseOrderStatus.Sent ||
                            x.OrderStatus == LicenseOrderStatus.Processing ||
                            x.OrderStatus == LicenseOrderStatus.WaitingForPayment)
                .ToListAsync();
        }

        #endregion

        #region License

        public async Task<List<HardwareVaultLicense>> GetLicensesAsync()
        {
            return await _hardwareVaultLicenseRepository
                .Query()
                .ToListAsync();
        }

        public async Task<List<HardwareVaultLicense>> GetActiveLicensesAsync(string vaultId)
        {
            return await _hardwareVaultLicenseRepository
                .Query()
                .Where(x => x.EndDate >= DateTime.UtcNow.Date && x.HardwareVaultId == vaultId && x.Data != null)
                .ToListAsync();
        }

        public async Task<List<HardwareVaultLicense>> GetNewLicensesByHardwareVaultIdAsync(string vaultId)
        {
            return await _hardwareVaultLicenseRepository
                .Query()
                .Where(x => x.AppliedAt == null && x.HardwareVaultId == vaultId && x.Data != null)
                .ToListAsync();
        }

        public async Task<List<HardwareVaultLicense>> GetLicensesByOrderIdAsync(string orderId)
        {
            return await _hardwareVaultLicenseRepository
                .Query()
                .Where(d => d.LicenseOrderId == orderId)
                .ToListAsync();
        }

        public async Task<List<HardwareVaultLicense>> AddOrUpdateHardwareVaultEmptyLicensesAsync(string orderId, List<string> vaultIds)
        {
            var existsHardwareVaultLicenses = await GetLicensesByOrderIdAsync(orderId);

            if (existsHardwareVaultLicenses != null)
                await _hardwareVaultLicenseRepository.DeleteRangeAsync(existsHardwareVaultLicenses);

            var hardwareVaultLicenses = new List<HardwareVaultLicense>();

            foreach (var vaultId in vaultIds)
            {
                hardwareVaultLicenses.Add(new HardwareVaultLicense()
                {
                    LicenseOrderId = orderId,
                    HardwareVaultId = vaultId
                });
            }

            return await _hardwareVaultLicenseRepository.AddRangeAsync(hardwareVaultLicenses) as List<HardwareVaultLicense>;
        }

        public async Task<List<HardwareVaultLicense>> AddHardwareVaultLicenseRangeAsync(List<HardwareVaultLicense> hardwareVaultLicenses)
        {
            if (hardwareVaultLicenses == null)
                throw new ArgumentNullException(nameof(hardwareVaultLicenses));

            return await _hardwareVaultLicenseRepository.AddRangeAsync(hardwareVaultLicenses) as List<HardwareVaultLicense>;
        }

        public async Task UpdateHardwareVaultsLicenseStatusAsync()
        {
            var hardwareVaultsChangedStatus = new List<HardwareVault>();

            var hardwareVaults = await _hardwareVaultRepository
                .Query()
                .Where(d => d.LicenseEndDate != null)
                .ToListAsync();

            foreach (var hardwareVault in hardwareVaults)
            {
                if (hardwareVault.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays > 90)
                {
                    if (hardwareVault.LicenseStatus != VaultLicenseStatus.Valid)
                    {
                        hardwareVault.LicenseStatus = VaultLicenseStatus.Valid;
                        hardwareVaultsChangedStatus.Add(hardwareVault);
                    }
                }
                else if (hardwareVault.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays > 30)
                {
                    if (hardwareVault.LicenseStatus != VaultLicenseStatus.Warning)
                    {
                        hardwareVault.LicenseStatus = VaultLicenseStatus.Warning;
                        hardwareVaultsChangedStatus.Add(hardwareVault);
                    }
                }
                else if (hardwareVault.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays > 0)
                {
                    if (hardwareVault.LicenseStatus != VaultLicenseStatus.Critical)
                    {
                        hardwareVault.LicenseStatus = VaultLicenseStatus.Critical;
                        hardwareVaultsChangedStatus.Add(hardwareVault);
                    }
                }
                else if (hardwareVault.LicenseEndDate.Value.Date.Subtract(DateTime.UtcNow.Date).TotalDays < 0)
                {
                    if (hardwareVault.LicenseStatus != VaultLicenseStatus.Expired)
                    {
                        hardwareVault.LicenseStatus = VaultLicenseStatus.Expired;
                        hardwareVaultsChangedStatus.Add(hardwareVault);
                    }
                }
            }

            if (hardwareVaultsChangedStatus.Count > 0)
            {
                await _hardwareVaultRepository.UpdateOnlyPropAsync(hardwareVaults, new string[] { nameof(HardwareVault.LicenseStatus) });
                await _applicationUserService.SendHardwareVaultLicenseStatus(hardwareVaultsChangedStatus);
            }
        }

        public async Task ChangeLicenseAppliedAsync(string vaultId, string licenseId)
        {
            var hardwareVaultLicense = await _hardwareVaultLicenseRepository
                .Query()
                .Where(d => d.HardwareVaultId == vaultId && d.Id == licenseId)
                .FirstOrDefaultAsync();

            if (hardwareVaultLicense == null)
                throw new Exception("Hardware vault license not found.");

            var vault = await _hardwareVaultRepository.GetByIdAsync(vaultId);
            if (vault == null)
                throw new Exception($"[{vaultId}] Hardware vault not found.");

            // Set license is applied
            hardwareVaultLicense.AppliedAt = DateTime.UtcNow;

            // Check if there are more licenses
            var newLicenses = await GetNewLicensesByHardwareVaultIdAsync(vaultId);
            if (newLicenses.Count == 0)
                vault.HasNewLicense = false;

            // Set license end date to vault
            if (vault.LicenseEndDate.HasValue)
            {
                vault.LicenseEndDate = vault.LicenseEndDate < hardwareVaultLicense.EndDate ? hardwareVaultLicense.EndDate : vault.LicenseEndDate;
            }
            else
            {
                vault.LicenseEndDate = hardwareVaultLicense.EndDate;
            }

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _hardwareVaultLicenseRepository.UpdateOnlyPropAsync(hardwareVaultLicense, new string[] { nameof(HardwareVaultLicense.AppliedAt) });
                await _hardwareVaultRepository.UpdateOnlyPropAsync(vault, new string[] { nameof(HardwareVault.HasNewLicense), nameof(HardwareVault.LicenseEndDate) });
                transactionScope.Complete();
            }
        }

        private async Task UpdateHardwareVaultsLicensesAsync(string orderId)
        {
            if (orderId == null)
                throw new ArgumentNullException(nameof(orderId));

            var response = await HttpClientGetDeviceLicensesAsync(orderId);

            response.EnsureSuccessStatusCode();

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                var licenses = JsonConvert.DeserializeObject<List<HardwareVaultLicenseDto>>(data);

                // Get dummy licenses 
                var dummyLicenses = await GetLicensesByOrderIdAsync(orderId);

                // Get vaults to update
                var vaultsIds = licenses.Select(d => d.DeviceId).ToList();
                var vaults = await _hardwareVaultRepository.Query().Where(x => vaultsIds.Contains(x.Id)).ToListAsync();

                foreach (var license in licenses)
                {
                    var dummyLicense = dummyLicenses.FirstOrDefault(c => c.HardwareVaultId == license.DeviceId);
                    dummyLicense.ImportedAt = DateTime.UtcNow;
                    dummyLicense.EndDate = license.LicenseEndDate;
                    dummyLicense.Data = Convert.FromBase64String(license.Data);

                    var device = vaults.FirstOrDefault(d => d.Id == license.DeviceId);
                    device.HasNewLicense = true;
                    device.LicenseEndDate = dummyLicense.EndDate;
                }

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await _hardwareVaultLicenseRepository.UpdateOnlyPropAsync(dummyLicenses, new string[] { "ImportedAt", "EndDate", "Data" });
                    await _hardwareVaultRepository.UpdateOnlyPropAsync(vaults, new string[] { "HasNewLicense", "LicenseEndDate" });
                    transactionScope.Complete();
                }
            }
        }

        #endregion
        public void Dispose()
        {
            _licenseOrderRepository.Dispose();
            _hardwareVaultLicenseRepository.Dispose();
            _hardwareVaultRepository.Dispose();
            _appSettingsService.Dispose();
        }
    }
}