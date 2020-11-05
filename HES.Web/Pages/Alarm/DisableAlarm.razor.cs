using System;
using HES.Core.Enums;
using HES.Core.Entities;
using HES.Core.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using HES.Core.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace HES.Web.Pages.Alarm
{
    public partial class DisableAlarm : OwningComponentBase
    {
        public IRemoteWorkstationConnectionsService RemoteWorkstationConnections { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DisableAlarm> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public EventCallback CallBack { get; set; }

        public string UserConfirmPassword { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                RemoteWorkstationConnections = ScopedServices.GetRequiredService<IRemoteWorkstationConnectionsService>();

                var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                ApplicationUser = await UserManager.GetUserAsync(state.User);
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
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Alarm);
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