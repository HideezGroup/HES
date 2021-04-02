using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.HardwareVaults;
using HES.Core.Models.Workstations;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Workstations
{
    public partial class AddProximityVault : HESModalBase
    {
        IWorkstationService WorkstationService { get; set; }
        IHardwareVaultService HardwareVaultService { get; set; }
        IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] ILogger<AddProximityVault> Logger { get; set; }
        [Parameter] public string WorkstationId { get; set; }

        public List<HardwareVault> HardwareVaults { get; set; }
        public HardwareVault SelectedHardwareVault { get; set; }
        public string WarningMessage { get; set; }
        public bool AlreadyAdded { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                WorkstationService = ScopedServices.GetRequiredService<IWorkstationService>();
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();
                RemoteWorkstationConnectionsService = ScopedServices.GetRequiredService<IRemoteWorkstationConnectionsService>();

                SearchText = string.Empty;
                await LoadDataAsync();

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public int TotalRecords { get; set; }
        public string SearchText { get; set; }

        private async Task LoadDataAsync()
        {
            var filter = new HardwareVaultFilter() { Status = VaultStatus.Active };
            TotalRecords = await HardwareVaultService.GetVaultsCountAsync(new DataLoadingOptions<HardwareVaultFilter>
            {
                SearchText = SearchText,
                Filter = filter
            });

            HardwareVaults = await HardwareVaultService.GetVaultsAsync(new DataLoadingOptions<HardwareVaultFilter>
            {
                Take = TotalRecords,
                SortedColumn = nameof(HardwareVault.Id),
                SortDirection = ListSortDirection.Ascending,
                SearchText = SearchText,
                Filter = filter
            });

            var count = await WorkstationService.GetProximityVaultsCountAsync(new DataLoadingOptions<WorkstationDetailsFilter>() { EntityId = WorkstationId });
            var proximityVaultFilter = new DataLoadingOptions<WorkstationDetailsFilter>()
            {
                Take = count,
                SortedColumn = nameof(WorkstationProximityVault.HardwareVaultId),
                SortDirection = ListSortDirection.Ascending,
                EntityId = WorkstationId
            };
            var proximityVaults = await WorkstationService.GetProximityVaultsAsync(proximityVaultFilter);
            AlreadyAdded = proximityVaults.Count > 0;

            HardwareVaults = HardwareVaults.Where(x => !proximityVaults.Select(s => s.HardwareVaultId).Contains(x.Id)).ToList();
            SelectedHardwareVault = null;
            StateHasChanged();
        }

        private async Task SelectedItemChangedAsync(HardwareVault hardwareVault)
        {
            await InvokeAsync(() =>
            {
                SelectedHardwareVault = hardwareVault;
                StateHasChanged();
            });
        }

        private async Task SearchTextChangedAsync(string searchText)
        {
            SearchText = searchText;
            await LoadDataAsync();
        }

        private async Task AddVaultAsync()
        {
            try
            {
                if (SelectedHardwareVault == null)
                {
                    WarningMessage = "Please, select a vault.";
                    return;
                }

                await WorkstationService.AddProximityVaultAsync(WorkstationId, SelectedHardwareVault.Id);
                await RemoteWorkstationConnectionsService.UpdateProximitySettingsAsync(WorkstationId, await WorkstationService.GetProximitySettingsAsync(WorkstationId));
                await ToastService.ShowToastAsync("Vault added", ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }
    }
}