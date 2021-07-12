using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API.License;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public class LicenseService : ILicenseService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IAppSettingsService _appSettingsService;
        private readonly IApplicationUserService _applicationUserService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LicenseService> _logger;

        public LicenseService(IApplicationDbContext dbContext,
            IAppSettingsService appSettingsService,
            IApplicationUserService applicationUserService,
            IHttpClientFactory httpClientFactory,
            ILogger<LicenseService> logger)
        {
            _dbContext = dbContext;
            _appSettingsService = appSettingsService;
            _applicationUserService = applicationUserService;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        #region HttpClient

        private async Task<HttpClient> CreateHttpClient()
        {
            var licensing = await _appSettingsService.GetLicenseSettingsAsync();
            if (licensing == null)
            {
                throw new HESException(HESCode.ApiKeyEmpty);
            }

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(AppConstants.LicenseAddress);
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
            return await _dbContext.LicenseOrders.ToListAsync();
        }

        public async Task<List<LicenseOrder>> GetLicenseOrdersAsync(DataLoadingOptions<LicenseOrderFilter> dataLoadingOptions)
        {
            return await LicenseOrdersQuery(dataLoadingOptions).Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetLicenseOrdersCountAsync(DataLoadingOptions<LicenseOrderFilter> dataLoadingOptions)
        {
            return await LicenseOrdersQuery(dataLoadingOptions).CountAsync();
        }

        private IQueryable<LicenseOrder> LicenseOrdersQuery(DataLoadingOptions<LicenseOrderFilter> dataLoadingOptions)
        {
            var query = _dbContext.LicenseOrders
                .Include(x => x.HardwareVaultLicenses)
                .AsQueryable();

            // Filter
            if (dataLoadingOptions.Filter != null)
            {
                if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.Note))
                {
                    query = query.Where(x => x.Note.Contains(dataLoadingOptions.Filter.Note));
                }
                if (!string.IsNullOrWhiteSpace(dataLoadingOptions.Filter.ContactEmail))
                {
                    query = query.Where(w => w.ContactEmail.Contains(dataLoadingOptions.Filter.ContactEmail));
                }
                if (!dataLoadingOptions.Filter.ProlongLicense != null)
                {
                    query = query.Where(w => w.ProlongExistingLicenses == dataLoadingOptions.Filter.ProlongLicense);
                }
                if (dataLoadingOptions.Filter.OrderStatus != null)
                {
                    query = query.Where(x => x.OrderStatus == dataLoadingOptions.Filter.OrderStatus);
                }
                if (dataLoadingOptions.Filter.LicenseStartDateFrom != null)
                {
                    query = query.Where(w => w.StartDate >= dataLoadingOptions.Filter.LicenseStartDateFrom);
                }
                if (dataLoadingOptions.Filter.LicenseStartDateTo != null)
                {
                    query = query.Where(x => x.StartDate <= dataLoadingOptions.Filter.LicenseStartDateTo);
                }
                if (dataLoadingOptions.Filter.LicenseEndDateFrom != null)
                {
                    query = query.Where(w => w.EndDate >= dataLoadingOptions.Filter.LicenseEndDateFrom);
                }
                if (dataLoadingOptions.Filter.LicenseEndDateTo != null)
                {
                    query = query.Where(x => x.EndDate <= dataLoadingOptions.Filter.LicenseEndDateTo);
                }
                if (dataLoadingOptions.Filter.CreatedAtDateFrom != null)
                {
                    query = query.Where(w => w.CreatedAt >= dataLoadingOptions.Filter.CreatedAtDateFrom);
                }
                if (dataLoadingOptions.Filter.CreatedAtDateTo != null)
                {
                    query = query.Where(x => x.CreatedAt <= dataLoadingOptions.Filter.CreatedAtDateTo);
                }
            }

            // Search
            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.ContactEmail.Contains(dataLoadingOptions.SearchText) ||
                                    x.Note.Contains(dataLoadingOptions.SearchText));
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

            return query;
        }

        public async Task<LicenseOrder> GetLicenseOrderByIdAsync(string orderId)
        {
            return await _dbContext.LicenseOrders
                .Include(x => x.HardwareVaultLicenses)
                .FirstOrDefaultAsync(x => x.Id == orderId);
        }

        public async Task<LicenseOrder> CreateOrderAsync(LicenseOrder licenseOrder, List<HardwareVault> hardwareVaults)
        {
            if (licenseOrder == null)
            {
                throw new ArgumentNullException(nameof(licenseOrder));
            }

            EntityEntry<LicenseOrder> createdOrder;

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                createdOrder = _dbContext.LicenseOrders.Add(licenseOrder);
                await _dbContext.SaveChangesAsync();

                await AddOrUpdateHardwareVaultEmptyLicensesAsync(createdOrder.Entity.Id, hardwareVaults.Select(x => x.Id).ToList());
                transactionScope.Complete();
            }

            return createdOrder.Entity;
        }

        public async Task<LicenseOrder> EditOrderAsync(LicenseOrder licenseOrder, List<HardwareVault> hardwareVaults)
        {
            if (licenseOrder == null)
            {
                throw new ArgumentNullException(nameof(licenseOrder));
            }

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                _dbContext.LicenseOrders.Update(licenseOrder);
                await _dbContext.SaveChangesAsync();

                await AddOrUpdateHardwareVaultEmptyLicensesAsync(licenseOrder.Id, hardwareVaults.Select(x => x.Id).ToList());
                transactionScope.Complete();
            }

            return licenseOrder;
        }

        public async Task AddOrderRangeAsync(List<LicenseOrder> licenseOrders)
        {
            if (licenseOrders == null)
            {
                throw new ArgumentNullException(nameof(licenseOrders));
            }

            _dbContext.LicenseOrders.AddRange(licenseOrders);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteOrderAsync(string licenseOrderId)
        {
            if (string.IsNullOrWhiteSpace(licenseOrderId))
            {
                throw new ArgumentNullException(nameof(licenseOrderId));
            }

            var licenseOrder = await _dbContext.LicenseOrders.FindAsync(licenseOrderId);
            if (licenseOrder == null)
            {
                throw new HESException(HESCode.LicenseOrderNotFound);
            }

            _dbContext.LicenseOrders.Remove(licenseOrder);
            await _dbContext.SaveChangesAsync();
        }

        public async Task SendOrderAsync(LicenseOrder licenseOrder)
        {
            var vaultLicenses = await GetLicensesByOrderIdAsync(licenseOrder.Id);
            if (vaultLicenses == null)
            {
                throw new HESException(HESCode.LicenseForHardwareVaultNotFound);
            }

            var licensing = await _appSettingsService.GetLicenseSettingsAsync();
            if (licensing == null)
            {
                throw new HESException(HESCode.ApiKeyEmpty);
            }

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

            _dbContext.LicenseOrders.Update(licenseOrder);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateLicenseOrdersAsync()
        {
            var orders = await _dbContext.LicenseOrders
                .Where(x => x.OrderStatus == LicenseOrderStatus.Sent ||
                            x.OrderStatus == LicenseOrderStatus.Processing ||
                            x.OrderStatus == LicenseOrderStatus.WaitingForPayment)
                .ToListAsync();

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
                {
                    continue;
                }

                if (status == LicenseOrderStatus.Completed)
                {
                    await UpdateHardwareVaultsLicensesAsync(order.Id);
                }

                order.OrderStatus = status;

                _dbContext.LicenseOrders.Update(order);
                await _dbContext.SaveChangesAsync();

                await _applicationUserService.SendLicenseChangedAsync(order.CreatedAt, status);
            }
        }

        #endregion

        #region License

        public async Task<List<HardwareVaultLicense>> GetLicensesAsync()
        {
            return await _dbContext.HardwareVaultLicenses.ToListAsync();
        }

        public async Task<List<HardwareVaultLicense>> GetActiveLicensesAsync(string vaultId)
        {
            return await _dbContext.HardwareVaultLicenses
                .Where(x => x.EndDate >= DateTime.UtcNow.Date && x.HardwareVaultId == vaultId && x.Data != null)
                .ToListAsync();
        }

        public async Task<List<HardwareVaultLicense>> GetNewLicensesByHardwareVaultIdAsync(string vaultId)
        {
            return await _dbContext.HardwareVaultLicenses
                .Where(x => x.AppliedAt == null && x.HardwareVaultId == vaultId && x.Data != null)
                .ToListAsync();
        }

        public async Task<List<HardwareVaultLicense>> GetLicensesByOrderIdAsync(string orderId)
        {
            return await _dbContext.HardwareVaultLicenses
                .Where(d => d.LicenseOrderId == orderId)
                .ToListAsync();
        }

        public async Task AddOrUpdateHardwareVaultEmptyLicensesAsync(string orderId, List<string> vaultIds)
        {
            var existsHardwareVaultLicenses = await GetLicensesByOrderIdAsync(orderId);

            if (existsHardwareVaultLicenses != null)
            {
                _dbContext.HardwareVaultLicenses.RemoveRange(existsHardwareVaultLicenses);
            }

            var hardwareVaultLicenses = new List<HardwareVaultLicense>();

            foreach (var vaultId in vaultIds)
            {
                hardwareVaultLicenses.Add(new HardwareVaultLicense()
                {
                    LicenseOrderId = orderId,
                    HardwareVaultId = vaultId
                });
            }

            _dbContext.HardwareVaultLicenses.AddRange(hardwareVaultLicenses);
            await _dbContext.SaveChangesAsync();
        }

        public async Task AddHardwareVaultLicenseRangeAsync(List<HardwareVaultLicense> hardwareVaultLicenses)
        {
            if (hardwareVaultLicenses == null)
            {
                throw new ArgumentNullException(nameof(hardwareVaultLicenses));
            }

            _dbContext.HardwareVaultLicenses.AddRange(hardwareVaultLicenses);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateHardwareVaultsLicenseStatusAsync()
        {
            var hardwareVaultsChangedStatus = new List<HardwareVault>();

            var hardwareVaults = await _dbContext.HardwareVaults.Where(d => d.LicenseEndDate != null).ToListAsync();

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
                _dbContext.HardwareVaults.UpdateRange(hardwareVaults);
                await _dbContext.SaveChangesAsync();

                await _applicationUserService.SendHardwareVaultLicenseStatus(hardwareVaultsChangedStatus);
            }
        }

        public async Task ChangeLicenseAppliedAsync(string vaultId, string licenseId)
        {
            var hardwareVaultLicense = await _dbContext.HardwareVaultLicenses
                .Where(d => d.HardwareVaultId == vaultId && d.Id == licenseId)
                .FirstOrDefaultAsync();

            if (hardwareVaultLicense == null)
            {
                throw new HESException(HESCode.LicenseForHardwareVaultNotFound);
            }

            var vault = await _dbContext.HardwareVaults.FindAsync(vaultId);
            if (vault == null)
            {
                throw new HESException(HESCode.HardwareVaultNotFoundWithParam, new string[] { vaultId });
            }

            // Set license is applied
            hardwareVaultLicense.AppliedAt = DateTime.UtcNow;

            // Check if there are more licenses
            var newLicenses = await GetNewLicensesByHardwareVaultIdAsync(vaultId);
            if (newLicenses.Count == 0)
            {
                vault.HasNewLicense = false;
            }

            // Set license end date to vault
            if (vault.LicenseEndDate.HasValue)
            {
                vault.LicenseEndDate = vault.LicenseEndDate < hardwareVaultLicense.EndDate ? hardwareVaultLicense.EndDate : vault.LicenseEndDate;
            }
            else
            {
                vault.LicenseEndDate = hardwareVaultLicense.EndDate;
            }

            _dbContext.HardwareVaultLicenses.Update(hardwareVaultLicense);
            _dbContext.HardwareVaults.Update(vault);
            await _dbContext.SaveChangesAsync();
        }

        private async Task UpdateHardwareVaultsLicensesAsync(string orderId)
        {
            if (orderId == null)
            {
                throw new ArgumentNullException(nameof(orderId));
            }

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
                var vaults = await _dbContext.HardwareVaults.Where(x => vaultsIds.Contains(x.Id)).ToListAsync();

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

                _dbContext.HardwareVaultLicenses.UpdateRange(dummyLicenses);
                _dbContext.HardwareVaults.UpdateRange(vaults);
                await _dbContext.SaveChangesAsync();
            }
        }

        #endregion               
    }
}