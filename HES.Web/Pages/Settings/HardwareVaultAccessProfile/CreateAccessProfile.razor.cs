using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class CreateAccessProfile : OwningComponentBase
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<CreateAccessProfile> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public HardwareVaultProfile AccessProfile { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }
        public int InitPinExpirationValue { get; set; }
        public int InitPinLengthValue { get; set; }
        public int InitPinTryCountValue { get; set; }


        protected override async Task OnInitializedAsync()
        {
            HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();

            AccessProfile = await HardwareVaultService.ProfileQuery().AsNoTracking().FirstOrDefaultAsync(x => x.Id == "default");
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
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await HardwareVaultService.CreateProfileAsync(AccessProfile);
                    await ToastService.ShowToastAsync("Hardware vault profile created.", ToastType.Success);
                    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.HardwareVaultProfiles);
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