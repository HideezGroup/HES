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
    public partial class ByEmployeesTab : HESComponentBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        public IMainTableService<SummaryByEmployees, SummaryFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public ILogger<ByEmployeesTab> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<SummaryByEmployees, SummaryFilter>>();
                await MainTableService.InitializeAsync(WorkstationAuditService.GetSummaryByEmployeesAsync, WorkstationAuditService.GetSummaryByEmployeesCountAsync, ModalDialogService, StateHasChanged, nameof(SummaryByEmployees.Employee), syncPropName: "Employee");

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