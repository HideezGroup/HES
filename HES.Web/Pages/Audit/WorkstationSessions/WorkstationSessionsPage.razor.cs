using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Filters;
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
    public partial class WorkstationSessionsPage : HESPageBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        public IDataTableService<WorkstationSession, WorkstationSessionFilter> DataTableService { get; set; }
        [Inject] public ILogger<WorkstationSessionsPage> Logger { get; set; }
        [Parameter] public string DashboardFilter { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<WorkstationSession, WorkstationSessionFilter>>();

                switch (DashboardFilter)
                {
                    case "NonHideezUnlock":
                        DataTableService.DataLoadingOptions.Filter.UnlockedBy = Hideez.SDK.Communication.SessionSwitchSubject.NonHideez;
                        break;
                    case "LongOpenSession":
                        DataTableService.DataLoadingOptions.Filter.Query = WorkstationAuditService.SessionQuery().Where(x => x.StartDate <= DateTime.UtcNow.AddHours(-12) && x.EndDate == null);
                        break;
                    case "OpenedSessions":
                        DataTableService.DataLoadingOptions.Filter.Query = WorkstationAuditService.SessionQuery().Where(x => x.EndDate == null);
                        break;
                }

                await BreadcrumbsService.SetAuditWorkstationSessions();
                await DataTableService.InitializeAsync(WorkstationAuditService.GetWorkstationSessionsAsync, WorkstationAuditService.GetWorkstationSessionsCountAsync, StateHasChanged, nameof(WorkstationSession.StartDate), ListSortDirection.Descending);

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

        }
    }
}