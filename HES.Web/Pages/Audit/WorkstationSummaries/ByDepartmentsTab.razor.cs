using HES.Core.Interfaces;
using HES.Core.Models.Audit;
using HES.Core.Models.Filters;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Audit.WorkstationSummaries
{
    public partial class ByDepartmentsTab : HESPageBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        public IDataTableService<SummaryByDepartments, SummaryFilter> DataTableService { get; set; }
        [Inject] public ILogger<ByDepartmentsTab> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<SummaryByDepartments, SummaryFilter>>();
                await DataTableService.InitializeAsync(WorkstationAuditService.GetSummaryByDepartmentsAsync, WorkstationAuditService.GetSummaryByDepartmentsCountAsync, StateHasChanged, nameof(SummaryByDepartments.Company), syncPropName: "Company");

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