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
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DisableAlarm> Logger { get; set; }
        [CascadingParameter] public ModalDialogInstance ModalDialogInstance { get; set; }

        public string UserConfirmPassword { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                RemoteWorkstationConnections = ScopedServices.GetRequiredService<IRemoteWorkstationConnectionsService>();
                ApplicationUser = await UserManager.FindByEmailAsync(await GetCurrentUserEmailAsync());
                if (ApplicationUser == null)
                    throw new Exception("Required relogin");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogInstance.CloseAsync(ModalResult.Cancel);
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
                await ToastService.ShowToastAsync("All workstations are unlocked.", ToastType.Success);
                await ModalDialogInstance.CloseAsync(ModalResult.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogInstance.CloseAsync(ModalResult.Cancel);
            }
        }
    }
}