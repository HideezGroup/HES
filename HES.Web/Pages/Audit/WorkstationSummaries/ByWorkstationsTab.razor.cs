using HES.Core.Interfaces;
using HES.Core.Models.Audit;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class ByWorkstationsTab : HESPageBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        public IDataTableService<SummaryByWorkstations, SummaryFilter> DataTableService { get; set; }
        [Inject] public ILogger<ByWorkstationsTab> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<SummaryByWorkstations, SummaryFilter>>();

                await DataTableService.InitializeAsync(WorkstationAuditService.GetSummaryByWorkstationsAsync, WorkstationAuditService.GetSummaryByWorkstationsCountAsync, StateHasChanged, nameof(SummaryByWorkstations.Workstation), syncPropName: "Workstation");

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