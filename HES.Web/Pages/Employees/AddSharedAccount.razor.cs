using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.SharedAccounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class AddSharedAccount : HESModalBase
    {
        public ISharedAccountService SheredAccountSevice { get; set; }
        public IEmployeeService EmployeeService { get; set; }
        public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public ILogger<AddSharedAccount> Logger { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public List<SharedAccount> SharedAccounts { get; set; }
        public SharedAccount SelectedSharedAccount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                SheredAccountSevice = ScopedServices.GetRequiredService<ISharedAccountService>();
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                RemoteDeviceConnectionsService = ScopedServices.GetRequiredService<IRemoteDeviceConnectionsService>();

                var count = await SheredAccountSevice.GetSharedAccountsCountAsync(new DataLoadingOptions<SharedAccountsFilter>());
                SharedAccounts = await SheredAccountSevice.GetSharedAccountsAsync(new DataLoadingOptions<SharedAccountsFilter>
                {
                    Take = count,
                    SortedColumn = nameof(Employee.FullName),
                    SortDirection = ListSortDirection.Ascending
                });
                SelectedSharedAccount = SharedAccounts.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task AddSharedAccoountAsync()
        {
            try
            {
                var account = await EmployeeService.AddSharedAccountAsync(EmployeeId, SelectedSharedAccount.Id);
                var employee = await EmployeeService.GetEmployeeByIdAsync(account.EmployeeId);
                RemoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(employee.HardwareVaults.Select(x => x.Id).ToArray());
                await ToastService.ShowToastAsync("Account added and will be recorded when the device is connected to the server.", ToastType.Success);
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