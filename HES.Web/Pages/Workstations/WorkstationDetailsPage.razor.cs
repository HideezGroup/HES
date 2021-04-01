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
    public partial class WorkstationDetailsPage : HESPageBase, IDisposable
    {
        public IWorkstationService WorkstationService { get; set; }
        public IDataTableService<WorkstationProximityVault, WorkstationDetailsFilter> DataTableService { get; set; }
        [Inject] public ILogger<WorkstationDetailsPage> Logger { get; set; }
        [Parameter] public string WorkstationId { get; set; }

        public Workstation Workstation { get; set; }   

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<WorkstationProximityVault, WorkstationDetailsFilter>>();
                SynchronizationService.UpdateWorkstationDetailsPage += UpdateWorkstationDetailsPage;
                await LoadWorkstationAsync();
                await BreadcrumbsService.SetWorkstationDetails(Workstation.Name);
                await DataTableService.InitializeAsync(WorkstationService.GetProximityVaultsAsync, WorkstationService.GetProximityVaultsCountAsync, StateHasChanged, nameof(WorkstationProximityVault.HardwareVaultId), entityId: WorkstationId);
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
                await DataTableService.LoadTableDataAsync();
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
                builder.AddAttribute(1, nameof(AddProximityVault.WorkstationId), WorkstationId);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Add Proximity Vault", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        private async Task OpenDialogDeleteHardwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteProximityVault));
                builder.AddAttribute(1, nameof(DeleteProximityVault.WorkstationProximityVault), DataTableService.SelectedEntity);
                builder.AddAttribute(2, nameof(DeleteProximityVault.WorkstationId), WorkstationId);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Delete Proximity Vault", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        public void Dispose()
        {
            SynchronizationService.UpdateWorkstationDetailsPage -= UpdateWorkstationDetailsPage;
        }
    }
}