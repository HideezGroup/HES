﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class AddSharedAccount : ComponentBase
    {
        [Inject] ISharedAccountService SheredAccountSevice { get; set; }
        [Inject] IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] IEmployeeService EmployeeService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] ILogger<AddSharedAccount> Logger { get; set; }

        public List<SharedAccount> SharedAccounts { get; set; }
        public SharedAccount SelectedSharedAccount { get; set; }

        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            SharedAccounts = await SheredAccountSevice.GetSharedAccountsAsync();
            SelectedSharedAccount = SharedAccounts.First();
        }

        private async Task AddSharedAccoountAsync()
        {
            try
            {
                var account = await EmployeeService.AddSharedAccountAsync(EmployeeId, SelectedSharedAccount.Id);
                var employee = await EmployeeService.GetEmployeeByIdAsync(account.EmployeeId);
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(employee.HardwareVaults.Select(x => x.Id).ToArray());
                await ModalDialogService.CloseAsync();
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Account added and will be recorded when the device is connected to the server.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                await ModalDialogService.CloseAsync();
                Logger.LogError(ex.Message, ex);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
        }

        private async Task CloseAsync()
        {
            await ModalDialogService.CloseAsync();
        }
    }
}
