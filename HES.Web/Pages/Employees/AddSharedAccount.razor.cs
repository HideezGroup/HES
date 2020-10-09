using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.SharedAccounts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class AddSharedAccount : ComponentBase
    {
        [Inject] ISharedAccountService SheredAccountSevice { get; set; }
        [Inject] public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] IEmployeeService EmployeeService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] ILogger<AddSharedAccount> Logger { get; set; }
        [Inject] IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string EmployeeId { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public List<SharedAccount> SharedAccounts { get; set; }
        public SharedAccount SelectedSharedAccount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var count = await SheredAccountSevice.GetSharedAccountsCountAsync(new DataLoadingOptions<SharedAccountsFilter>());
            SharedAccounts = await SheredAccountSevice.GetSharedAccountsAsync(new DataLoadingOptions<SharedAccountsFilter>
            {
                Take = count,
                SortedColumn = nameof(Employee.FullName),
                SortDirection = ListSortDirection.Ascending
            });
            SelectedSharedAccount = SharedAccounts.FirstOrDefault();
        }

        private async Task AddSharedAccoountAsync()
        {
            try
            {
                var account = await EmployeeService.AddSharedAccountAsync(EmployeeId, SelectedSharedAccount.Id);
                var employee = await EmployeeService.GetEmployeeByIdAsync(account.EmployeeId);
                RemoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(employee.HardwareVaults.Select(x => x.Id).ToArray());
                await ToastService.ShowToastAsync("Account added and will be recorded when the device is connected to the server.", ToastType.Success);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.EmployeesDetails, EmployeeId);
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
