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
    public partial class ByEmployeesTab : HESComponentBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        public IDataTableService<SummaryByEmployees, SummaryFilter> DataTableService { get; set; }
        [Inject] public ILogger<ByEmployeesTab> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<SummaryByEmployees, SummaryFilter>>();
                await DataTableService.InitializeAsync(WorkstationAuditService.GetSummaryByEmployeesAsync, WorkstationAuditService.GetSummaryByEmployeesCountAsync, StateHasChanged, nameof(SummaryByEmployees.Employee), syncPropName: "Employee");

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