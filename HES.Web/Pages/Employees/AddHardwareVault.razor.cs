using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Filters;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Web.Pages.Employees
{
    public partial class AddHardwareVault : HESModalBase
    {
        public IEmployeeService EmployeeService { get; set; }
        public IHardwareVaultService HardwareVaultService { get; set; }
        public ILdapService LdapService { get; set; }
        [Inject] public IPageSyncService SynchronizationService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddHardwareVault> Logger { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public List<HardwareVault> HardwareVaults { get; set; }
        public HardwareVault SelectedHardwareVault { get; set; }
        public string WarningMessage { get; set; }
        public int TotalRecords { get; set; }
        public string SearchText { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();
                LdapService = ScopedServices.GetRequiredService<ILdapService>();

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
        private async Task LoadDataAsync()
        {
            var filter = new HardwareVaultFilter() { Status = VaultStatus.Ready };
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

                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await EmployeeService.AddHardwareVaultAsync(EmployeeId, SelectedHardwareVault.Id);

                    var ldapSettings = await AppSettingsService.GetSettingsAsync<LdapSettings>(ServerConstants.Domain);
                    if (ldapSettings?.Password != null)
                    {
                        var employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId);
                        if (employee.ActiveDirectoryGuid != null)
                        {
                            await LdapService.AddUserToHideezKeyOwnersAsync(ldapSettings, employee.ActiveDirectoryGuid);
                        }
                    }

                    transactionScope.Complete();
                }

                await ToastService.ShowToastAsync("Vault added", ToastType.Success);
                await SynchronizationService.HardwareVaultStateChanged(SelectedHardwareVault.Id);
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