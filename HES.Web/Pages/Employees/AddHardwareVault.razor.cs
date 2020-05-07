﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.HardwareVault;
using HES.Core.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class AddHardwareVault : ComponentBase
    {
        [Inject] IEmployeeService EmployeeService { get; set; }
        [Inject] IRemoteWorkstationConnectionsService RemoteWorkstationConnectionsService { get; set; }
        [Inject] IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] ILogger<AddHardwareVault> Logger { get; set; }


        [Parameter] public Func<HardwareVaultFilter, Task> AddVault { get; set; }


        public List<HardwareVault> HardwareVaults { get; set; }
        public HardwareVault SelectedHardwareVault { get; set; }


        protected override async Task OnInitializedAsync()
        {
            SearchText = string.Empty;
            await LoadTableDataAsync();
        }

        public int TotalRecords { get; set; }
        public string SearchText { get; set; }

        private async Task LoadTableDataAsync()
        {
            TotalRecords = await HardwareVaultService.GetVaultsCountAsync(SearchText, new HardwareVaultFilter());
            HardwareVaults = await HardwareVaultService.GetVaultsAsync(0, TotalRecords, nameof(HardwareVault.Id), ListSortDirection.Ascending, SearchText, new HardwareVaultFilter());
            
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
            await LoadTableDataAsync();
        }

        private async Task AddVaultAsync()
        {
            try
            {
                await EmployeeService.AddHardwareVaultAsync("", new string[] { });
                RemoteWorkstationConnectionsService.StartUpdateRemoteDevice(new string[] { });
                await ModalDialogService.CloseAsync();
                ToastService.ShowToast("Vault added successfully", ToastLevel.Error);
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
