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
    public partial class EnableAlarm : HESModalBase
    {
        public IRemoteWorkstationConnectionsService RemoteWorkstationConnections { get; set; }
        [Inject] public ILogger<EnableAlarm> Logger { get; set; }
        [Parameter] public string CurrentUserEmail { get; set; }

        protected override void OnInitialized()
        {
            RemoteWorkstationConnections = ScopedServices.GetRequiredService<IRemoteWorkstationConnectionsService>();
        }

        private async Task EnableAlarmAsync()
        {
            try
            {
                await RemoteWorkstationConnections.LockAllWorkstationsAsync(CurrentUserEmail);
                await ToastService.ShowToastAsync("All workstations are locked.", ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }
    }
}