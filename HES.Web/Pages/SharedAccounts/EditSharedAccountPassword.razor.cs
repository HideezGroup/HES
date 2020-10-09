using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class EditSharedAccountPassword : OwningComponentBase, IDisposable
    {
        public ISharedAccountService SharedAccountService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public ILogger<EditSharedAccountPassword> Logger { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }
        [Parameter] public string AccountId { get; set; }

        public SharedAccount Account { get; set; }
        public AccountPassword AccountPassword { get; set; } = new AccountPassword();
        public bool EntityBeingEdited { get; set; }
        public bool Initialized { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                SharedAccountService = ScopedServices.GetRequiredService<ISharedAccountService>();

                Account = await SharedAccountService.GetSharedAccountByIdAsync(AccountId);

                if (Account == null)
                    throw new Exception("Account not found");

                ModalDialogService.OnCancel += OnCancelAsync;
                EntityBeingEdited = MemoryCache.TryGetValue(Account.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Account.Id, Account);

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task OnCancelAsync()
        {
            await SharedAccountService.UnchangedAsync(Account);
        }

        private async Task EditAccoountPasswordAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    var vaults = await SharedAccountService.EditSharedAccountPwdAsync(Account, AccountPassword);
                    RemoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(vaults);
                    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.SharedAccounts);
                    await ToastService.ShowToastAsync("Account password updated.", ToastType.Success);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public void Dispose()
        {
            ModalDialogService.OnCancel -= OnCancelAsync;

            if (!EntityBeingEdited)
                MemoryCache.Remove(Account.Id);
        }
    }
}