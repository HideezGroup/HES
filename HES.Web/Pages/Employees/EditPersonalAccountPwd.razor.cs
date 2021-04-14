﻿using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Accounts;
using HES.Core.Models.AppSettings;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Web.Pages.Employees
{
    public partial class EditPersonalAccountPwd : HESModalBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public ILdapService LdapService { get; set; }
        public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditPersonalAccountPwd> Logger { get; set; }
        [Parameter] public string AccountId { get; set; }

        public AccountPassword AccountPassword { get; set; } = new AccountPassword();
        public Employee Employee { get; set; }
        public Account Account { get; set; }
        public LdapSettings LdapSettings { get; set; }
        public Button ButtonSpinner { get; set; }
        public bool EntityBeingEdited { get; set; }



        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                LdapService = ScopedServices.GetRequiredService<ILdapService>();
                RemoteDeviceConnectionsService = ScopedServices.GetRequiredService<IRemoteDeviceConnectionsService>();

                Account = await EmployeeService.GetAccountByIdAsync(AccountId);
                if (Account == null)
                    throw new HESException(HESCode.AccountNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(Account.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Account.Id, Account);

                Employee = await EmployeeService.GetEmployeeByIdAsync(Account.EmployeeId);
                LdapSettings = await AppSettingsService.GetSettingsAsync<LdapSettings>(ServerConstants.Domain);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task EditAccountPasswordAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        await EmployeeService.EditPersonalAccountPwdAsync(Account, AccountPassword);

                        if (AccountPassword.UpdateActiveDirectoryPassword)
                            await LdapService.SetUserPasswordAsync(Account.EmployeeId, AccountPassword.Password, LdapSettings);

                        transactionScope.Complete();
                    }

                    RemoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await EmployeeService.GetEmployeeVaultIdsAsync(Account.EmployeeId));
                    await ToastService.ShowToastAsync("Account password updated.", ToastType.Success);
                    await ModalDialogClose();
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogClose();
            }
        }

        protected override async Task ModalDialogCancel()
        {
            EmployeeService.UnchangedPersonalAccount(Account);
            await base.ModalDialogCancel();
        }

        public void Dispose()
        {         
            if (!EntityBeingEdited)
                MemoryCache.Remove(Account.Id);
        }
    }
}