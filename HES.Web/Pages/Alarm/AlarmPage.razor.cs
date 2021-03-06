﻿using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Filters;
using HES.Core.Services;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Alarm
{
    public partial class AlarmPage : HESPageBase, IDisposable
    {
        public IWorkstationService WorkstationService { get; set; }
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AlarmPage> Logger { get; set; }

        public AlarmState AlarmState { get; set; }
        public int WorkstationOnline { get; set; }
        public int WorkstationCount { get; set; }
        public string CurrentUserEmail { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
                AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();
                PageSyncService.UpdateAlarmPage += UpdateAlarmPage;

                await BreadcrumbsService.SetAlarm();
                await GetAlarmStateAsync();

                CurrentUserEmail = await GetCurrentUserEmailAsync();
                WorkstationOnline = RemoteWorkstationConnectionsService.GetWorkstationsOnlineCount();
                WorkstationCount = await WorkstationService.GetWorkstationsCountAsync(new DataLoadingOptions<WorkstationFilter>());

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private async Task UpdateAlarmPage(string exceptPageId)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await GetAlarmStateAsync();
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
                builder.AddAttribute(1, nameof(EnableAlarm.CurrentUserEmail), CurrentUserEmail);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Alarm_EnableAlarm_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await GetAlarmStateAsync();
                await PageSyncService.UpdateAlarm(PageId);
            }
        }

        private async Task DisableAlarmAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DisableAlarm));
                builder.AddAttribute(1, nameof(DisableAlarm.CurrentUserEmail), CurrentUserEmail);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Alarm_DisableAlarm_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await GetAlarmStateAsync();
                await PageSyncService.UpdateAlarm(PageId);
            }
        }

        public void Dispose()
        {
            PageSyncService.UpdateAlarmPage -= UpdateAlarmPage;
        }
    }
}