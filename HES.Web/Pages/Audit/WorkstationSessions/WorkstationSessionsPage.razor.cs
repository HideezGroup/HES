using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSessions
{
    public partial class WorkstationSessionsPage : HESComponentBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        public IMainTableService<WorkstationSession, WorkstationSessionFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public ILogger<WorkstationSessionsPage> Logger { get; set; }
        [Parameter] public string DashboardFilter { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<WorkstationSession, WorkstationSessionFilter>>();

                switch (DashboardFilter)
                {
                    case "NonHideezUnlock":
                        MainTableService.DataLoadingOptions.Filter.UnlockedBy = Hideez.SDK.Communication.SessionSwitchSubject.NonHideez;
                        break;
                    case "LongOpenSession":
                        MainTableService.DataLoadingOptions.Filter.Query = WorkstationAuditService.SessionQuery().Where(x => x.StartDate <= DateTime.UtcNow.AddHours(-12) && x.EndDate == null);
                        break;
                    case "OpenedSessions":
                        MainTableService.DataLoadingOptions.Filter.Query = WorkstationAuditService.SessionQuery().Where(x => x.EndDate == null);
                        break;
                }

                await BreadcrumbsService.SetAuditWorkstationSessions();
                await MainTableService.InitializeAsync(WorkstationAuditService.GetWorkstationSessionsAsync, WorkstationAuditService.GetWorkstationSessionsCountAsync, ModalDialogService, StateHasChanged, nameof(WorkstationSession.StartDate), ListSortDirection.Descending);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        public void Dispose()
        {
            MainTableService.Dispose();
        }
    }
}