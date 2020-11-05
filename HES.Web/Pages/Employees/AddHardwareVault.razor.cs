using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.HardwareVaults;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Web.Pages.Employees
{
    public partial class AddHardwareVault : OwningComponentBase
    {
        IEmployeeService EmployeeService { get; set; }
        IHardwareVaultService HardwareVaultService { get; set; }
        ILdapService LdapService { get; set; }
        [Inject] IAppSettingsService AppSettingsService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<AddHardwareVault> Logger { get; set; }
        [Inject] IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public string EmployeeId { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public List<HardwareVault> HardwareVaults { get; set; }
        public HardwareVault SelectedHardwareVault { get; set; }
        public string WarningMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
            HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();
            LdapService = ScopedServices.GetRequiredService<ILdapService>();

            SearchText = string.Empty;
            await LoadDataAsync();
        }

        public int TotalRecords { get; set; }
        public string SearchText { get; set; }

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

                    var ldapSettings = await AppSettingsService.GetLdapSettingsAsync();
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

                await Refresh.InvokeAsync(this);
                await ToastService.ShowToastAsync("Vault added", ToastType.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.EmployeesDetails, EmployeeId);
                await HubContext.Clients.All.SendAsync(RefreshPage.HardwareVaultStateChanged, SelectedHardwareVault.Id);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                await ModalDialogService.CloseAsync();
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        private async Task CloseAsync()
        {
            await ModalDialogService.CloseAsync();
        }
    }
}