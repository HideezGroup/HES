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
    public partial class EditProfile : HESComponentBase, IDisposable
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditProfile> Logger { get; set; }
        [Parameter] public string HardwareVaultProfileId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }

        public HardwareVaultProfile AccessProfile { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }
        public bool EntityBeingEdited { get; set; }
        public int InitPinExpirationValue { get; set; }
        public int InitPinLengthValue { get; set; }
        public int InitPinTryCountValue { get; set; }


        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();

                ModalDialogService.OnCancel += OnCancelAsync;

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
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task EditProfileAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await HardwareVaultService.EditProfileAsync(AccessProfile);
                    await ToastService.ShowToastAsync("Hardware vault profile updated.", ToastType.Success);            
                    await SynchronizationService.UpdateHardwareVaultProfiles(ExceptPageId);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task OnCancelAsync()
        {
            await HardwareVaultService.UnchangedProfileAsync(AccessProfile);
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
            ModalDialogService.OnCancel -= OnCancelAsync;

            if (!EntityBeingEdited)
                MemoryCache.Remove(AccessProfile.Id);
        }
    }
}