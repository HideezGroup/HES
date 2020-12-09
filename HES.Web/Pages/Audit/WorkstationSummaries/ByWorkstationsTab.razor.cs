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
    public partial class ByWorkstationsTab : HESComponentBase, IDisposable
    {
        public IWorkstationAuditService WorkstationAuditService { get; set; }
        public IMainTableService<SummaryByWorkstations, SummaryFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public ILogger<ByWorkstationsTab> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationAuditService = ScopedServices.GetRequiredService<IWorkstationAuditService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<SummaryByWorkstations, SummaryFilter>>();
                await MainTableService.InitializeAsync(WorkstationAuditService.GetSummaryByWorkstationsAsync, WorkstationAuditService.GetSummaryByWorkstationsCountAsync, ModalDialogService, StateHasChanged, nameof(SummaryByWorkstations.Workstation), syncPropName: "Workstation");

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