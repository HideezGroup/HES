using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.HardwareVaults
{
    public partial class ChangeProfile : HESComponentBase, IDisposable
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public ILogger<ChangeProfile> Logger { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Parameter] public string HardwareVaultId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }

        public HardwareVault HardwareVault { get; set; }
        public SelectList VaultProfiles { get; set; }
        public string SelectedVaultProfileId { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();
                RemoteDeviceConnectionsService = ScopedServices.GetRequiredService<IRemoteDeviceConnectionsService>();

                HardwareVault = await HardwareVaultService.GetVaultByIdAsync(HardwareVaultId);
                if (HardwareVault == null)
                    throw new Exception("HardwareVault not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(HardwareVault.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(HardwareVault.Id, HardwareVault);

                VaultProfiles = new SelectList(await HardwareVaultService.GetProfilesAsync(), nameof(HardwareVaultProfile.Id), nameof(HardwareVaultProfile.Name));
                SelectedVaultProfileId = VaultProfiles.First().Value;

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task ChangeProfileAsync()
        {
            try
            {
                await HardwareVaultService.ChangeVaultProfileAsync(HardwareVault.Id, SelectedVaultProfileId);
                RemoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(HardwareVault.Id);
                await ToastService.ShowToastAsync("Vault profile updated", ToastType.Success);
                await SynchronizationService.UpdateHardwareVaults(ExceptPageId);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(HardwareVault.Id);
        }
    }
}