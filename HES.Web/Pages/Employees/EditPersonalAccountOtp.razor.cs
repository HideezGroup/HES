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

namespace HES.Web.Pages.Employees
{
    public partial class EditPersonalAccountOtp : HESComponentBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EditPersonalAccountOtp> Logger { get; set; }
        [Parameter] public string AccountId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }

        public Account Account { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }
        public bool EntityBeingEdited { get; set; }

        private AccountOtp _accountOtp = new AccountOtp();


        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                RemoteDeviceConnectionsService = ScopedServices.GetRequiredService<IRemoteDeviceConnectionsService>();

                ModalDialogService.OnCancel += ModalDialogService_OnCancel;

                Account = await EmployeeService.GetAccountByIdAsync(AccountId);
                if (Account == null)
                    throw new Exception("Account not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(Account.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Account.Id, Account);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task EditAccountOtpAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await EmployeeService.EditPersonalAccountOtpAsync(Account, _accountOtp);
                    RemoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await EmployeeService.GetEmployeeVaultIdsAsync(Account.EmployeeId));
                    await ToastService.ShowToastAsync("Account OTP updated.", ToastType.Success);
                    await SynchronizationService.UpdateEmployeeDetails(ExceptPageId, Account.EmployeeId);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (IncorrectOtpException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(AccountOtp.OtpSecret), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task ModalDialogService_OnCancel()
        {
            await EmployeeService.UnchangedPersonalAccountAsync(Account);
        }

        public void Dispose()
        {
            ModalDialogService.OnCancel -= ModalDialogService_OnCancel;

            if (!EntityBeingEdited)
                MemoryCache.Remove(Account.Id);
        }
    }
}