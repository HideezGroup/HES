using HES.Core.Entities;
using HES.Core.Enums;
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
    public partial class EditProfile : HESModalBase, IDisposable
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditProfile> Logger { get; set; }
        [Parameter] public string HardwareVaultProfileId { get; set; }

        public HardwareVaultProfile AccessProfile { get; set; }
        public Button Button { get; set; }
        public bool EntityBeingEdited { get; set; }
        public int InitPinExpirationValue { get; set; }
        public int InitPinLengthValue { get; set; }
        public int InitPinTryCountValue { get; set; }


        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();

                AccessProfile = await HardwareVaultService.GetProfileByIdAsync(HardwareVaultProfileId);
                if (AccessProfile == null)
                    throw new Exception("Hardware Vault Profile not found.");

                InitPinExpirationValue = AccessProfile.PinExpirationConverted;
                InitPinLengthValue = AccessProfile.PinLength;
                InitPinTryCountValue = AccessProfile.PinTryCount;

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
        protected override async Task ModalDialogCancel()
        {
            HardwareVaultService.UnchangedProfile(AccessProfile);
            await base.ModalDialogCancel();
        }

        private async Task EditProfileAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await HardwareVaultService.EditProfileAsync(AccessProfile);
                    await ToastService.ShowToastAsync("Hardware vault profile updated.", ToastType.Success);            
                    await ModalDialogClose();
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private void OnInputPinExpiration(ChangeEventArgs args)
        {
            AccessProfile.PinExpirationConverted = Convert.ToInt32((string)args.Value);
        }

        private void OnInputPinLength(ChangeEventArgs args)
        {
            AccessProfile.PinLength = Convert.ToInt32((string)args.Value);
        }

        private void OnInputPinTryCount(ChangeEventArgs args)
        {
            AccessProfile.PinTryCount = Convert.ToInt32((string)args.Value);
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(AccessProfile.Id);
        }
    }
}