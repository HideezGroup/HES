using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Alarm
{
    public partial class DisableAlarm : HESComponentBase
    {
        public IRemoteWorkstationConnectionsService RemoteWorkstationConnections { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DisableAlarm> Logger { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public EventCallback CallBack { get; set; }

        public string UserConfirmPassword { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                RemoteWorkstationConnections = ScopedServices.GetRequiredService<IRemoteWorkstationConnectionsService>();
                ApplicationUser = await UserManager.GetUserAsync((await AuthenticationStateTask).User);
                if (ApplicationUser == null)
                    throw new Exception("Required relogin");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task DisableAlarmAsync()
        {
            try
            {
                var checkPassword = await UserManager.CheckPasswordAsync(ApplicationUser, UserConfirmPassword);

                if (!checkPassword)
                    throw new Exception("Invalid password");

                await RemoteWorkstationConnections.UnlockAllWorkstationsAsync(ApplicationUser.Email);
                await SynchronizationService.UpdateAlarm(ExceptPageId);
                await CallBack.InvokeAsync(this);
                await ToastService.ShowToastAsync("All workstations are unlocked.", ToastType.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }
    }
}