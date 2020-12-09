using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class DeleteHardwareVault : HESComponentBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IHardwareVaultService HardwareVaultService { get; set; }
        public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteHardwareVault> Logger { get; set; }
        [Parameter] public string HardwareVaultId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public HardwareVault HardwareVault { get; set; }
        public VaultStatusReason Reason { get; set; } = VaultStatusReason.Withdrawal;
        public bool IsNeedBackup { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();
                RemoteDeviceConnectionsService = ScopedServices.GetRequiredService<IRemoteDeviceConnectionsService>();

                HardwareVault = await HardwareVaultService.GetVaultByIdAsync(HardwareVaultId);
                if (HardwareVault == null)
                    throw new Exception("HardwareVault not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(HardwareVault.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(HardwareVault.Id, HardwareVault);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public async Task DeleteVaultAsync()
        {
            try
            {
                var employeeId = HardwareVault.EmployeeId;
                await EmployeeService.RemoveHardwareVaultAsync(HardwareVault.Id, Reason, IsNeedBackup);
                await Refresh.InvokeAsync(this);
                RemoteDeviceConnectionsService.StartUpdateHardwareVaultStatus(HardwareVault.Id);
                await SynchronizationService.UpdateEmployeeDetails(ExceptPageId, employeeId);
                await SynchronizationService.HardwareVaultStateChanged(HardwareVault.Id);
                await ToastService.ShowToastAsync("Vault removed.", ToastType.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public async Task CancelAsync()
        {
            await ModalDialogService.CloseAsync();
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(HardwareVault.Id);
        }
    }
}