using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class CreateAccessProfile : HESModalBase
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public ILogger<CreateAccessProfile> Logger { get; set; }

        public HardwareVaultProfile AccessProfile { get; set; }
        public Button Button { get; set; }
        public int InitPinExpirationValue { get; set; }
        public int InitPinLengthValue { get; set; }
        public int InitPinTryCountValue { get; set; }


        protected override async Task OnInitializedAsync()
        {
            HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();

            AccessProfile = await HardwareVaultService.GetDefaultProfile();
            AccessProfile.Id = null;
            AccessProfile.Name = null;

            InitPinExpirationValue = AccessProfile.PinExpirationConverted;
            InitPinLengthValue = AccessProfile.PinLength;
            InitPinTryCountValue = AccessProfile.PinTryCount;
        }

        private async Task CreateProfileAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await HardwareVaultService.CreateProfileAsync(AccessProfile);
                    await ToastService.ShowToastAsync("Hardware vault profile created.", ToastType.Success); 
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
    }
}