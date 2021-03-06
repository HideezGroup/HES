﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Filters;
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

                PageSyncService.UpdateHardwareVaultsPage += UpdateHardwareVaultsPage;
                PageSyncService.UpdateHardwareVaultState += UpdateHardwareVaultState;

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


        private async Task UpdateHardwareVaultsPage(string exceptPageId)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
                StateHasChanged();
            });
        }

        private async Task UpdateHardwareVaultState(string hardwareVaultId)
        {
            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync(string.Format(Resources.Resource.Message_HardwareVaultStateChanged, hardwareVaultId), ToastType.Notify);
                StateHasChanged();
            });
        }

        private async Task ImportVaultsAsync()
        {
            try
            {
                await HardwareVaultService.ImportVaultsAsync();
                await DataTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync(Resources.Resource.HardwareVaults_ImportVaults_Toast, ToastType.Success);
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.HardwareVaults_EditRfid_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateHardwareVaults(PageId);
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.HardwareVaults_ChangeStatus_Title_Suspend, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateHardwareVaults(PageId);
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.HardwareVaults_ChangeStatus_Title_Activate, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateHardwareVaults(PageId);
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.HardwareVaults_ChangeStatus_Title_Compromised, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateHardwareVaults(PageId);
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

            await ModalDialogService.ShowAsync(Resources.Resource.HardwareVaults_ShowActivationCode_Title, body);
        }

        private async Task ChangeVaultProfileAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeProfile));
                builder.AddAttribute(1, nameof(ChangeProfile.HardwareVaultId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.HardwareVaults_ChangeProfile_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateHardwareVaults(PageId);
            }
        }

        public void Dispose()
        {
            PageSyncService.UpdateHardwareVaultsPage -= UpdateHardwareVaultsPage;
            PageSyncService.UpdateHardwareVaultState -= UpdateHardwareVaultState;
        }
    }
}