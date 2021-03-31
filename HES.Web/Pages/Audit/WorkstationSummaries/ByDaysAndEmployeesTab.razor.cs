using HES.Core.Interfaces;
using HES.Core.Models.Web.Audit;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class ByDaysAndEmployeesTab : HESPageBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        public IDataTableService<SummaryByDayAndEmployee, SummaryFilter> DataTableService { get; set; }
        [Inject] public ILogger<ByDaysAndEmployeesTab> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<SummaryByDayAndEmployee, SummaryFilter>>();
                await DataTableService.InitializeAsync(WorkstationAuditService.GetSummaryByDayAndEmployeesAsync, WorkstationAuditService.GetSummaryByDayAndEmployeesCountAsync, StateHasChanged, nameof(SummaryByDayAndEmployee.Date), syncPropName: "Date");

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