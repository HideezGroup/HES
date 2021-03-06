﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.ApplicationUsers;
using HES.Core.Models.Filters;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EmployeeDetailsPage : HESPageBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IAppSettingsService AppSettingsService { get; set; }
        public IDataTableService<Account, AccountFilter> DataTableService { get; set; }
        public ILdapService LdapService { get; set; }
        [Inject] public ILogger<EmployeeDetailsPage> Logger { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public Employee Employee { get; set; }
        public UserSsoInfo UserSsoInfo { get; set; }
        public string LdapHost { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<Account, AccountFilter>>();
                LdapService = ScopedServices.GetRequiredService<ILdapService>();

                PageSyncService.UpdateEmployeeDetailsPage += UpdateEmployeeDetailsPage;
                PageSyncService.UpdateHardwareVaultState += UpdateHardwareVaultState;

                await LoadEmployeeAsync();
                await LoadEmployeeSsoState();
                await LoadLdapSettingsAsync();
                await BreadcrumbsService.SetEmployeeDetails(Employee?.FullName);
                await DataTableService.InitializeAsync(EmployeeService.GetAccountsAsync, EmployeeService.GetAccountsCountAsync, StateHasChanged, nameof(Account.Name), entityId: EmployeeId);

                SetInitialized();
            }
            catch (Exception ex)
            {
                SetLoadFailed(ex.Message);
                Logger.LogError(ex.Message);
            }
        }

        #region Page update

        private async Task UpdateEmployeeDetailsPage(string exceptPageId, string employeeId)
        {
            if (Employee.Id != employeeId || PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await LoadEmployeeAsync();
                await DataTableService.LoadTableDataAsync();
                StateHasChanged();
            });
        }

        private async Task UpdateHardwareVaultState(string hardwareVaultId)
        {
            if (!Employee.HardwareVaults.Any(x => x.Id == hardwareVaultId))
                return;

            await InvokeAsync(async () =>
            {
                await LoadEmployeeAsync();
            });
        }

        #endregion

        private async Task LoadEmployeeAsync()
        {
            Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId, asNoTracking: true);
            if (Employee == null)
                throw new HESException(HESCode.EmployeeNotFound);

            StateHasChanged();
        }

        private async Task LoadEmployeeSsoState()
        {
            UserSsoInfo = await EmployeeService.GetUserSsoInfoAsync(Employee);
            StateHasChanged();
        }

        private async Task LoadLdapSettingsAsync()
        {
            var ldapSettings = await AppSettingsService.GetLdapSettingsAsync();

            if (ldapSettings != null)
            {
                LdapHost = ldapSettings.Host.Split(".")[0];
            }
        }

        private async Task<bool> VerifyAdUserAsync()
        {
            try
            {
                await LdapService.VerifyAdUserAsync(Employee);
                return true;
            }
            catch (HESException ex) when (ex.Code == HESCode.ActiveDirectoryUserNotFound)
            {
                await EmployeeService.RemoveFromHideezKeyOwnersAsync(Employee.Id);
                Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId, asNoTracking: true);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Notify);
                return false;
            }
            catch (LdapException ex) when (ex.ResultCode == LdapException.ServerDown)
            {
                await ToastService.ShowToastAsync(HESException.GetMessage(HESCode.TheLDAPServerIsUnavailable), ToastType.Error);
                return false;
            }
            catch (Exception ex)
            {
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                return false;
            }
        }

        #region Dialogs

        private async Task OpenDialogAddHardwareVaultAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddHardwareVault));
                builder.AddAttribute(1, nameof(AddHardwareVault.EmployeeId), EmployeeId);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_AddHardwareVault_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeAsync();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        private async Task OpenDialogRemoveHardwareVaultAsync(HardwareVault hardwareVault)
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteHardwareVault));
                builder.AddAttribute(1, nameof(DeleteHardwareVault.HardwareVaultId), hardwareVault.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_DeleteHardwareVault_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeAsync();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        public async Task OpenModalAddSoftwareVaultAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            //RenderFragment body = (builder) =>
            //{
            //    builder.OpenComponent(0, typeof(AddSoftwareVault));
            //    builder.AddAttribute(1, "Employee", Employee);
            //    builder.CloseComponent();
            //};

            //await ModalDialogService2.ShowAsync("Add software vault", body);
        }

        private async Task OpenDialogCreatePersonalAccountAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreatePersonalAccount));
                builder.AddAttribute(1, nameof(CreatePersonalAccount.EmployeeId), EmployeeId);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_CreatePersonalAccount_Title, body, ModalDialogSize.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeAsync();
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        private async Task OpenDialogAddSharedAccountAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSharedAccount));
                builder.AddAttribute(1, nameof(AddSharedAccount.EmployeeId), EmployeeId);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_AddSharedAccount_Title, body, ModalDialogSize.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeAsync();
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        private async Task OpenDialogSetAsWorkstationAccountAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SetAsWorkstationAccount));
                builder.AddAttribute(1, nameof(SetAsWorkstationAccount.AccountId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Set As Workstation Account", body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeAsync();
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        private async Task OpenDialogEditPersonalAccountAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditPersonalAccount));
                builder.AddAttribute(1, nameof(EditPersonalAccount.AccountId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_EditPersonalAccount_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeAsync();
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        private async Task OpenDialogEditPersonalAccountPasswordAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditPersonalAccountPwd));
                builder.AddAttribute(1, nameof(EditPersonalAccountPwd.AccountId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_EditPersonalAccountPwd_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeAsync();
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        private async Task OpenDialogEditPersonalAccountOtpAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditPersonalAccountOtp));
                builder.AddAttribute(1, nameof(EditPersonalAccountOtp.AccountId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_EditPersonalAccountOtp_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeAsync();
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        private async Task OpenDialogDeleteAccountAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteAccount));
                builder.AddAttribute(1, nameof(DeleteAccount.AccountId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_DeleteAccount_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeAsync();
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        private async Task OpenDialogResendInvitationAsync(SoftwareVaultInvitation softwareVaultInvitation)
        {
            if (!await VerifyAdUserAsync()) return;

            //RenderFragment body = (builder) =>
            //{
            //    builder.OpenComponent(0, typeof(SoftwareVaults.ResendSoftwareVaultInvitation));
            //    builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadEmployeeAsync));
            //    builder.AddAttribute(2, "SoftwareVaultInvitation", softwareVaultInvitation);
            //    builder.CloseComponent();
            //};

            //await ModalDialogService2.ShowAsync("Resend invitation", body);
        }

        private async Task OpenDialogDeleteInvitationAsync(SoftwareVaultInvitation softwareVaultInvitation)
        {
            if (!await VerifyAdUserAsync()) return;

            //RenderFragment body = (builder) =>
            //{
            //    builder.OpenComponent(0, typeof(SoftwareVaults.DeleteSoftwareVaultInvitation));
            //    builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadEmployeeAsync));
            //    builder.AddAttribute(2, "SoftwareVaultInvitation", softwareVaultInvitation);
            //    builder.CloseComponent();
            //};

            //await ModalDialogService2.ShowAsync("Delete invitation", body);
        }

        private async Task OpenDialogSoftwareVaultDetailsAsync(SoftwareVault softwareVault)
        {
            if (!await VerifyAdUserAsync()) return;

            //RenderFragment body = (builder) =>
            //{
            //    builder.OpenComponent(0, typeof(SoftwareVaultDetails));
            //    builder.AddAttribute(1, nameof(SoftwareVaultDetails.SoftwareVault), softwareVault);
            //    builder.CloseComponent();
            //};

            //await ModalDialogService.ShowAsync("Software vault details", body);
        }

        private async Task OpenDialogHardwareVaultDetailsAsync(HardwareVault hardwareVault)
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(HardwareVaultDetails));
                builder.AddAttribute(1, nameof(HardwareVaultDetails.HardwareVault), hardwareVault);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_HardwareVaultDetails_Title, body);
        }

        private async Task OpenDialogShowActivationCodeAsync(HardwareVault hardwareVault)
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(HardwareVaults.ShowActivationCode));
                builder.AddAttribute(1, nameof(HardwareVaults.ShowActivationCode.HardwareVaultId), hardwareVault.Id);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Activation code", body);
        }

        private async Task OpenModalEnableSsoAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EmployeeEnableSso));
                builder.AddAttribute(1, nameof(EmployeeEnableSso.Employee), Employee);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_EnableSso_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeSsoState();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        private async Task OpenModalDisableSsoAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EmployeeDisableSso));
                builder.AddAttribute(1, nameof(EmployeeDisableSso.Employee), Employee);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_DisableSso_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeSsoState();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        private async Task OpenModalEditSsoAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EmployeeEditSso));
                builder.AddAttribute(1, nameof(EmployeeEditSso.Employee), Employee);
                builder.AddAttribute(2, nameof(EmployeeEditSso.Info), UserSsoInfo);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.EmployeeDetails_EditSso_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadEmployeeSsoState();
                await PageSyncService.UpdateEmployeeDetails(PageId, EmployeeId);
            }
        }

        #endregion

        public void Dispose()
        {
            PageSyncService.UpdateEmployeeDetailsPage -= UpdateEmployeeDetailsPage;
            PageSyncService.UpdateHardwareVaultState -= UpdateHardwareVaultState;
        }
    }
}