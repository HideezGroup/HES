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
    public partial class EditPersonalAccount : HESModalBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditPersonalAccount> Logger { get; set; }
        [Parameter] public string AccountId { get; set; }

        public Account Account { get; set; }
        public AccountEditModel PersonalAccount { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button ButtonSpinner { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                RemoteDeviceConnectionsService = ScopedServices.GetRequiredService<IRemoteDeviceConnectionsService>();

                Account = await EmployeeService.GetAccountByIdAsync(AccountId);
                if (Account == null)
                    throw new HESException(HESCode.AccountNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(Account.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Account.Id, Account);

                PersonalAccount = new AccountEditModel().Initialize(Account);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task EditAccountAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await EmployeeService.EditPersonalAccountAsync(PersonalAccount);
                    RemoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await EmployeeService.GetEmployeeVaultIdsAsync(PersonalAccount.EmployeeId));
                    await ToastService.ShowToastAsync("Account updated.", ToastType.Success);
                    await ModalDialogClose();
                });
            }
            catch (HESException ex) when (ex.Code == HESCode.AccountExist)
            {
                ValidationErrorMessage.DisplayError(nameof(Account.Name), ex.Message);
            }
            catch (HESException ex) when (ex.Code == HESCode.IncorrectUrl)
            {
                ValidationErrorMessage.DisplayError(nameof(Account.Urls), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        protected override async Task ModalDialogCancel()
        {
            await EmployeeService.UnchangedPersonalAccountAsync(Account);
            await base.ModalDialogCancel();
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Account.Id);
        }
    }
}