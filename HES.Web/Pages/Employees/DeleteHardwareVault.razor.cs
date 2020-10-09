using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class DeleteHardwareVault : OwningComponentBase, IDisposable
    {
        IEmployeeService EmployeeService { get; set; }
        [Inject] IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] ILogger<DeleteHardwareVault> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string HardwareVaultId { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        public HardwareVault HardwareVault { get; set; }
        public VaultStatusReason Reason { get; set; } = VaultStatusReason.Withdrawal;
        public bool IsNeedBackup { get; set; }
        public bool EntityBeingEdited { get; set; }
        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();

                HardwareVault = await HardwareVaultService.GetVaultByIdAsync(HardwareVaultId);
                if (HardwareVault == null)
                    throw new Exception("HardwareVault not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(HardwareVault.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(HardwareVault.Id, HardwareVault);

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
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
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.EmployeesDetails, employeeId);
                await HubContext.Clients.All.SendAsync(RefreshPage.HardwareVaultStateChanged, HardwareVault.Id);
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