using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class DeleteProfile : HESModalBase, IDisposable
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteProfile> Logger { get; set; }
        [Parameter] public string HardwareVaultProfileId { get; set; }

        public HardwareVaultProfile AccessProfile { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();

                AccessProfile = await HardwareVaultService.GetProfileByIdAsync(HardwareVaultProfileId);
                if (AccessProfile == null)
                    throw new HESException(HESCode.HardwareVaultProfileNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(AccessProfile.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(AccessProfile.Id, AccessProfile);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task DeleteProfileAsync()
        {
            try
            {
                await HardwareVaultService.DeleteProfileAsync(AccessProfile.Id);
                await ToastService.ShowToastAsync(Resources.Resource.HardwareVaultAccessProfile_DeleteProfile_Toast, ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(AccessProfile.Id);
        }
    }
}