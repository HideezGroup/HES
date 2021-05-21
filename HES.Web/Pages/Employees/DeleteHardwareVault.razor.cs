using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
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
    public partial class DeleteHardwareVault : HESModalBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IHardwareVaultService HardwareVaultService { get; set; }
        public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public IPageSyncService SynchronizationService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteHardwareVault> Logger { get; set; }
        [Parameter] public string HardwareVaultId { get; set; }

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
                    throw new HESException(HESCode.HardwareVaultNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(HardwareVault.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(HardwareVault.Id, HardwareVault);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public async Task DeleteVaultAsync()
        {
            try
            {
                var employeeId = HardwareVault.EmployeeId;
                await EmployeeService.RemoveHardwareVaultAsync(HardwareVault.Id, Reason, IsNeedBackup);
                RemoteDeviceConnectionsService.StartUpdateHardwareVaultStatus(HardwareVault.Id);
                await SynchronizationService.HardwareVaultStateChanged(HardwareVault.Id);
                await ToastService.ShowToastAsync(Resources.Resource.EmployeeDetails_DeleteHardwareVault_Toast, ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(HardwareVault.Id);
        }
    }
}