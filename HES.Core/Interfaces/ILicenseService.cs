using HES.Core.Entities;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.LicenseOrders;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILicenseService : IDisposable
    {
        Task<List<LicenseOrder>> GetLicenseOrdersAsync();
        Task<List<LicenseOrder>> GetLicenseOrdersAsync(DataLoadingOptions<LicenseOrderFilter> dataLoadingOptions);
        Task<int> GetLicenseOrdersCountAsync(DataLoadingOptions<LicenseOrderFilter> dataLoadingOptions);
        Task<LicenseOrder> GetLicenseOrderByIdAsync(string orderId);
        Task<LicenseOrder> CreateOrderAsync(LicenseOrder licenseOrder, List<HardwareVault> hardwareVaults);
        Task<LicenseOrder> EditOrderAsync(LicenseOrder licenseOrder, List<HardwareVault> hardwareVaults);
        Task<List<LicenseOrder>> AddOrderRangeAsync(List<LicenseOrder> licenseOrders);
        Task DeleteOrderAsync(LicenseOrder licenseOrder);
        Task SendOrderAsync(LicenseOrder licenseOrder);
        Task UpdateLicenseOrdersAsync();
        Task<List<HardwareVaultLicense>> GetLicensesAsync();
        Task<List<HardwareVaultLicense>> GetActiveLicensesAsync(string vaultId);
        Task<List<HardwareVaultLicense>> GetNewLicensesByHardwareVaultIdAsync(string vaultId);
        Task<List<HardwareVaultLicense>> GetLicensesByOrderIdAsync(string orderId);
        Task<List<HardwareVaultLicense>> AddOrUpdateHardwareVaultEmptyLicensesAsync(string orderId, List<string> vaultIds);
        Task<List<HardwareVaultLicense>> AddHardwareVaultLicenseRangeAsync(List<HardwareVaultLicense> hardwareVaultLicenses);
        Task UpdateHardwareVaultsLicenseStatusAsync();
        Task ChangeLicenseAppliedAsync(string vaultId, string licenseId);
    }
}