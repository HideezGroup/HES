using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Workstations;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class WorkstationDetailsPage : HESComponentBase, IDisposable
    {
        public IWorkstationService WorkstationService { get; set; }
        public IMainTableService<WorkstationProximityVault, WorkstationDetailsFilter> MainTableService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<WorkstationDetailsPage> Logger { get; set; }
        [Parameter] public string WorkstationId { get; set; }

        public Workstation Workstation { get; set; }   

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<WorkstationProximityVault, WorkstationDetailsFilter>>();
                SynchronizationService.UpdateWorkstationDetailsPage += UpdateWorkstationDetailsPage;
                await LoadWorkstationAsync();
                await BreadcrumbsService.SetWorkstationDetails(Workstation.Name);
                await MainTableService.InitializeAsync(WorkstationService.GetProximityVaultsAsync, WorkstationService.GetProximityVaultsCountAsync, ModalDialogService, StateHasChanged, nameof(WorkstationProximityVault.HardwareVaultId), entityId: WorkstationId);
                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateWorkstationDetailsPage(string exceptPageId, string workstationId, string userName)
        {
            if (Workstation.Id != workstationId || PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {     
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync($"Page edited by {userName}.", ToastType.Notify);
                StateHasChanged();
            });
        }

        private async Task LoadWorkstationAsync()
        {
            Workstation = await WorkstationService.GetWorkstationByIdAsync(WorkstationId);
            if (Workstation == null)
                throw new Exception("Workstation not found.");
        }

        private async Task OpenDialogAddHardwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddProximityVault));
                builder.AddAttribute(1, "WorkstationId", WorkstationId);
                builder.AddAttribute(2, "ExceptPageId", PageId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add Proximity Vault", body);
        }

        private async Task OpenDialogDeleteHardwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteProximityVault));
                builder.AddAttribute(1, "WorkstationProximityVault", MainTableService.SelectedEntity);
                builder.AddAttribute(2, "WorkstationId", WorkstationId);
                builder.AddAttribute(3, "ExceptPageId", PageId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete Proximity Vault", body);
        }

        public void Dispose()
        {
            SynchronizationService.UpdateWorkstationDetailsPage -= UpdateWorkstationDetailsPage;
            MainTableService.Dispose();
        }
    }
}
