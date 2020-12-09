using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.AppSettings;
using HES.Core.Models.Web.Workstations;
using HES.Core.Services;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Alarm
{
    public partial class AlarmPage : HESComponentBase, IDisposable
    {
        public IWorkstationService WorkstationService { get; set; }
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<AlarmPage> Logger { get; set; }

        public AlarmState AlarmState { get; set; }
        public int WorkstationOnline { get; set; }
        public int WorkstationCount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
                AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();

                SynchronizationService.UpdateAlarmPage += UpdateAlarmPage;

                await BreadcrumbsService.SetAlarm();
                await GetAlarmStateAsync();
                WorkstationOnline = RemoteWorkstationConnectionsService.WorkstationsOnlineCount();
                WorkstationCount = await WorkstationService.GetWorkstationsCountAsync(new DataLoadingOptions<WorkstationFilter>());
                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private async Task UpdateAlarmPage(string exceptPageId, string userName)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await GetAlarmStateAsync();
                await ToastService.ShowToastAsync($"Page edited by {userName}.", ToastType.Notify);
                StateHasChanged();
            });
        }

        private async Task GetAlarmStateAsync()
        {
            AlarmState = await AppSettingsService.GetAlarmStateAsync();
        }

        private async Task EnableAlarmAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EnableAlarm));
                builder.AddAttribute(1, nameof(EnableAlarm.CallBack), EventCallback.Factory.Create(this, GetAlarmStateAsync));
                builder.AddAttribute(2, nameof(EnableAlarm.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Turn on alarm", body);
        }

        private async Task DisableAlarmAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DisableAlarm));
                builder.AddAttribute(1, nameof(DisableAlarm.CallBack), EventCallback.Factory.Create(this, GetAlarmStateAsync));
                builder.AddAttribute(2, nameof(DisableAlarm.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Turn off alarm", body);
        }

        public void Dispose()
        {
            SynchronizationService.UpdateAlarmPage -= UpdateAlarmPage;
        }
    }
}