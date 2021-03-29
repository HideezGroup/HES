using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Alarm
{
    public partial class EnableAlarm : HESComponentBase
    {
        public IRemoteWorkstationConnectionsService RemoteWorkstationConnections { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EnableAlarm> Logger { get; set; }
        [CascadingParameter] public ModalDialogInstance ModalDialogInstance { get; set; }

        private async Task EnableAlarmAsync()
        {
            try
            {
                RemoteWorkstationConnections = ScopedServices.GetRequiredService<IRemoteWorkstationConnectionsService>();

                string userEmail = null;
                try
                {
                    userEmail = (await AuthenticationStateTask).User.Identity.Name;
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.Message);
                }
                finally
                {
                    await RemoteWorkstationConnections.LockAllWorkstationsAsync(userEmail);
                }

                await ToastService.ShowToastAsync("All workstations are locked.", ToastType.Success);
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