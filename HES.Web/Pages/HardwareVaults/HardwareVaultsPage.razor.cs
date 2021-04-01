using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.HardwareVaults;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.HardwareVaults
{
    public partial class HardwareVaultsPage : HESPageBase, IDisposable
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        public IDataTableService<HardwareVault, HardwareVaultFilter> DataTableService { get; set; }
        [Inject] public ILogger<HardwareVaultsPage> Logger { get; set; }
        [Parameter] public string DashboardFilter { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<HardwareVault, HardwareVaultFilter>>();

                SynchronizationService.UpdateHardwareVaultsPage += UpdateHardwareVaultsPage;
                SynchronizationService.UpdateHardwareVaultState += UpdateHardwareVaultState;

                switch (DashboardFilter)
                {
                    case "LowBattery":
                        DataTableService.DataLoadingOptions.Filter.Battery = "low";
                        break;
                    case "VaultLocked":
                        DataTableService.DataLoadingOptions.Filter.Status = VaultStatus.Locked;
                        break;
                    case "VaultReady":
                        DataTableService.DataLoadingOptions.Filter.Status = VaultStatus.Ready;
                        break;
                    case "LicenseWarning":
                        DataTableService.DataLoadingOptions.Filter.LicenseStatus = VaultLicenseStatus.Warning;
                        break;
                    case "LicenseCritical":
                        DataTableService.DataLoadingOptions.Filter.LicenseStatus = VaultLicenseStatus.Critical;
                        break;
                    case "LicenseExpired":
                        DataTableService.DataLoadingOptions.Filter.LicenseStatus = VaultLicenseStatus.Expired;
                        break;
                }

                await BreadcrumbsService.SetHardwareVaults();
                await DataTableService.InitializeAsync(HardwareVaultService.GetVaultsAsync, HardwareVaultService.GetVaultsCountAsync, StateHasChanged, nameof(HardwareVault.Id));

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }


        private async Task UpdateHardwareVaultsPage(string exceptPageId, string userName)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync($"Page edited by {userName}.", ToastType.Notify);
                StateHasChanged();
            });
        }

        private async Task UpdateHardwareVaultState(string hardwareVaultId)
        {
            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync($"Hardware Vault {hardwareVaultId} state changed.", ToastType.Notify);
                StateHasChanged();
            });
        }

        private async Task ImportVaultsAsync()
        {
            try
            {
                await HardwareVaultService.ImportVaultsAsync();
                await DataTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync("Vaults imported.", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        private async Task EditRfidAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditRfid));
                builder.AddAttribute(1, nameof(EditRfid.HardwareVaultId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Edit RFID", body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateHardwareVaults(PageId);
            }
        }

        private async Task SuspendVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeStatus));
                builder.AddAttribute(1, nameof(ChangeStatus.HardwareVaultId), DataTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(ChangeStatus.VaultStatus), VaultStatus.Suspended);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Suspend", body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateHardwareVaults(PageId);
            }
        }

        private async Task ActivateVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeStatus));
                builder.AddAttribute(1, nameof(ChangeStatus.HardwareVaultId), DataTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(ChangeStatus.VaultStatus), VaultStatus.Active);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Activate", body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateHardwareVaults(PageId);
            }
        }

        private async Task CompromisedVaultAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeStatus));
                builder.AddAttribute(1, nameof(ChangeStatus.HardwareVaultId), DataTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(ChangeStatus.VaultStatus), VaultStatus.Compromised);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Compromised", body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateHardwareVaults(PageId);
            }
        }

        private async Task ShowActivationCodeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ShowActivationCode));
                builder.AddAttribute(1, nameof(ShowActivationCode.HardwareVaultId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            await ModalDialogService2.ShowAsync("Activation code", body);
        }

        private async Task ChangeVaultProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeProfile));
                builder.AddAttribute(1, nameof(ChangeProfile.HardwareVaultId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Profile", body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateHardwareVaults(PageId);
            }
        }

        public void Dispose()
        {
            SynchronizationService.UpdateHardwareVaultsPage -= UpdateHardwareVaultsPage;
            SynchronizationService.UpdateHardwareVaultState -= UpdateHardwareVaultState;
        }
    }
}