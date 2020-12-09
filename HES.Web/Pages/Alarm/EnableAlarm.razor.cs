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
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EnableAlarm> Logger { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public EventCallback CallBack { get; set; }

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

                await SynchronizationService.UpdateAlarm(ExceptPageId);
                await CallBack.InvokeAsync(this);
                await ToastService.ShowToastAsync("All workstations are locked.", ToastType.Success);
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