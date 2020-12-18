using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class EditSharedAccountOtp : HESComponentBase, IDisposable
    {
        public ISharedAccountService SharedAccountService { get; set; }
        public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditSharedAccountOtp> Logger { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public string AccountId { get; set; }

        public SharedAccount Account { get; set; }
        public AccountOtp AccountOtp { get; set; } = new AccountOtp();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                SharedAccountService = ScopedServices.GetRequiredService<ISharedAccountService>();
                RemoteDeviceConnectionsService = ScopedServices.GetRequiredService<IRemoteDeviceConnectionsService>();

                ModalDialogService.OnCancel += OnCancelAsync;

                Account = await SharedAccountService.GetSharedAccountByIdAsync(AccountId);

                if (Account == null)
                    throw new Exception("Account not found");

                EntityBeingEdited = MemoryCache.TryGetValue(Account.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Account.Id, Account);

                SetInitialized();
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

        private async Task EditAccoountOtpAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    var vaults = await SharedAccountService.EditSharedAccountOtpAsync(Account, AccountOtp);
                    RemoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(vaults);
                    await SynchronizationService.UpdateSharedAccounts(ExceptPageId);
                    await ToastService.ShowToastAsync("Account OTP updated.", ToastType.Success);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (IncorrectOtpException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.OtpSecret), ex.Message);
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