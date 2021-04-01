using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationEvents
{
    public partial class WorkstationEventsPage : HESPageBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        public IDataTableService<WorkstationEvent, WorkstationEventFilter> DataTableService { get; set; }
        [Inject] public ILogger<WorkstationEventsPage> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<WorkstationEvent, WorkstationEventFilter>>();

                await BreadcrumbsService.SetAuditWorkstationEvents();
                await DataTableService.InitializeAsync(WorkstationAuditService.GetWorkstationEventsAsync, WorkstationAuditService.GetWorkstationEventsCountAsync, StateHasChanged, nameof(WorkstationEvent.Date), ListSortDirection.Descending);

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