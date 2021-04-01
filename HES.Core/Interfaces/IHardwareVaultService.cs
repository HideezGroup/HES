using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models.Web;
using HES.Core.Models.Web.DataTableComponent;
using HES.Core.Models.Web.HardwareVaults;
using Hideez.SDK.Communication.Device;
using Hideez.SDK.Communication.HES.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IHardwareVaultService : IDisposable
    {
        IQueryable<HardwareVault> VaultQuery();
        Task<HardwareVault> GetVaultByIdAsync(string id);
        Task<List<HardwareVault>> GetVaultsWithoutLicenseAsync();
        Task<List<HardwareVault>> GetVaultsWithLicenseAsync();
        Task<List<HardwareVault>> GetVaultsAsync(DataLoadingOptions<HardwareVaultFilter> options);
        Task<int> GetVaultsCountAsync(DataLoadingOptions<HardwareVaultFilter> options);
        Task ImportVaultsAsync();
        Task UpdateRfidAsync(HardwareVault vault);
        Task UpdateNeedSyncAsync(HardwareVault vault, bool needSync);
        Task UpdateNeedSyncAsync(List<HardwareVault> vaults, bool needSync);
        Task UpdateTimestampAsync(HardwareVault vault, uint timestamp);
        Task<HardwareVault> UpdateVaultAsync(HardwareVault vault);
        Task UnchangedVaultAsync(HardwareVault vault);
        Task SetReadyStatusAsync(HardwareVault vault);
        Task SetActiveStatusAsync(HardwareVault vault);
        Task SetLockedStatusAsync(HardwareVault vault);
        Task SetDeactivatedStatusAsync(HardwareVault vault);
        Task UpdateVaultInfoAsync(HwVaultInfoFromClientDto dto);
        Task<HardwareVaultActivation> CreateVaultActivationAsync(string vaultId);
        Task ChangeVaultActivationStatusAsync(string vaultId, HardwareVaultActivationStatus status);
        Task SetVaultStatusAppliedAsync(HardwareVault hardwareVault);
        Task<string> GetVaultActivationCodeAsync(string vaultId);
        Task ActivateVaultAsync(string vaultId);
        Task SuspendVaultAsync(string vaultId, string description);
        Task VaultCompromisedAsync(string vaultId, VaultStatusReason reason, string description);
        IQueryable<HardwareVaultProfile> ProfileQuery();
        Task<List<HardwareVaultProfile>> GetProfilesAsync();
        Task<List<HardwareVaultProfile>> GetHardwareVaultProfilesAsync(DataLoadingOptions<HardwareVaultProfileFilter> dataLoadingOptions);
        Task<int> GetHardwareVaultProfileCountAsync(DataLoadingOptions<HardwareVaultProfileFilter> dataLoadingOptions);
        Task<HardwareVaultProfile> GetProfileByIdAsync(string profileId);
        Task<List<string>> GetVaultIdsByProfileTaskAsync();
        Task<HardwareVaultProfile> CreateProfileAsync(HardwareVaultProfile hardwareVaultProfile);
        Task EditProfileAsync(HardwareVaultProfile hardwareVaultProfile);
        Task DeleteProfileAsync(string id);
        Task ChangeVaultProfileAsync(string vaultId, string profileId);
        Task<AccessParams> GetAccessParamsAsync(string vaultId);
        Task UnchangedProfileAsync(HardwareVaultProfile profile);
    }
}