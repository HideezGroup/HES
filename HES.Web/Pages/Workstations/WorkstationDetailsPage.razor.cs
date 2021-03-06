﻿using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Filters;
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
        public IDataTableService<WorkstationHardwareVaultPair, WorkstationDetailsFilter> DataTableService { get; set; }
        [Inject] public ILogger<WorkstationDetailsPage> Logger { get; set; }
        [Parameter] public string WorkstationId { get; set; }

        public Workstation Workstation { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<WorkstationHardwareVaultPair, WorkstationDetailsFilter>>();
                PageSyncService.UpdateWorkstationDetailsPage += UpdateWorkstationDetailsPage;
                await LoadWorkstationAsync();
                await BreadcrumbsService.SetWorkstationDetails(Workstation.Name);
                await DataTableService.InitializeAsync(WorkstationService.GetWorkstationHardwareVaultPairsAsync, WorkstationService.GetWorkstationHardwareVaultPairsCountAsync, StateHasChanged, nameof(WorkstationHardwareVaultPair.HardwareVaultId), entityId: WorkstationId);
                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateWorkstationDetailsPage(string exceptPageId, string workstationId)
        {
            if (Workstation.Id != workstationId || PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
                StateHasChanged();
            });
        }

        private async Task LoadWorkstationAsync()
        {
            Workstation = await WorkstationService.GetWorkstationByIdAsync(WorkstationId);
            if (Workstation == null)
                throw new HESException(HESCode.WorkstationNotFound);
        }

        private async Task OpenDialogAddHardwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddProximityVault));
                builder.AddAttribute(1, nameof(AddProximityVault.WorkstationId), WorkstationId);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Workstations_AddProximityVault_Title, body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateWorkstationDetails(PageId, WorkstationId);
            }
        }

        private async Task OpenDialogDeleteHardwareVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteProximityVault));
                builder.AddAttribute(1, nameof(DeleteProximityVault.WorkstationProximityVault), DataTableService.SelectedEntity);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Workstations_DeleteProximityVault_Title, body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateWorkstationDetails(PageId, WorkstationId);
            }
        }

        public void Dispose()
        {
            PageSyncService.UpdateWorkstationDetailsPage -= UpdateWorkstationDetailsPage;
        }
    }
}