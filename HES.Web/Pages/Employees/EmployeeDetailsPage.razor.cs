using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EmployeeDetailsPage : OwningComponentBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IAppSettingsService AppSettingsService { get; set; }
        public IMainTableService<Account, AccountFilter> MainTableService { get; set; }
        public ILdapService LdapService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EmployeeDetailsPage> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public Employee Employee { get; set; }
        public string LdapHost { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }
        public bool AdUserNotFound { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<Account, AccountFilter>>();
                LdapService = ScopedServices.GetRequiredService<ILdapService>();

                await InitializeHubAsync();
                await LoadEmployeeAsync();
                await BreadcrumbsService.SetEmployeeDetails(Employee?.FullName);
                await LoadLdapSettingsAsync();
                await MainTableService.InitializeAsync(EmployeeService.GetAccountsAsync, EmployeeService.GetAccountsCountAsync, ModalDialogService, StateHasChanged, nameof(Account.Name), entityId: EmployeeId);

                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
            }
        }

        private async Task LoadEmployeeAsync()
        {
            Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId, true);
            if (Employee == null)
                throw new Exception("Employee not found.");

            if (!await VerifyAdUserAsync())
                Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId, asNoTracking: true);

            StateHasChanged();
        }

        private async Task LoadLdapSettingsAsync()
        {
            var ldapSettings = await AppSettingsService.GetLdapSettingsAsync();

            if (ldapSettings?.Password != null)
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
                await EmployeeService.ToLocalUserAsync(Employee.Id);
                Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId, asNoTracking: true);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Notify);
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
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadEmployeeAsync));
                builder.AddAttribute(2, "EmployeeId", EmployeeId);
                builder.AddAttribute(3, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add hardware vault", body);
        }

        private async Task OpenDialogRemoveHardwareVaultAsync(HardwareVault hardwareVault)
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteHardwareVault));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadEmployeeAsync));
                builder.AddAttribute(2, "HardwareVaultId", hardwareVault.Id);
                builder.AddAttribute(3, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete hardware vault", body);
        }

        public async Task OpenModalAddSoftwareVaultAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSoftwareVault));
                builder.AddAttribute(1, "Employee", Employee);
                builder.AddAttribute(2, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add software vault", body);
        }

        private async Task OpenDialogCreatePersonalAccountAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreatePersonalAccount));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, MainTableService.LoadTableDataAsync));
                builder.AddAttribute(2, "EmployeeId", EmployeeId);
                builder.AddAttribute(3, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Create personal account", body, ModalDialogSize.Large);
        }

        private async Task OpenDialogAddSharedAccountAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSharedAccount));
                builder.AddAttribute(1, "EmployeeId", EmployeeId);
                builder.AddAttribute(2, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Add shared account", body, ModalDialogSize.Large);
        }

        private async Task OpenDialogSetAsWorkstationAccountAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SetAsWorkstationAccount));
                builder.AddAttribute(1, "AccountId", MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Account", body);
        }

        private async Task OpenDialogEditPersonalAccountAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditPersonalAccount));
                builder.AddAttribute(1, nameof(EditPersonalAccount.AccountId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditPersonalAccount.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit account", body);
        }

        private async Task OpenDialogEditPersonalAccountPasswordAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditPersonalAccountPwd));
                builder.AddAttribute(1, "AccountId", MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit account password", body);
        }

        private async Task OpenDialogEditPersonalAccountOtpAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditPersonalAccountOtp));
                builder.AddAttribute(1, "AccountId", MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, "ConnectionId", hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit account otp", body);
        }

        private async Task OpenDialogGenerateAdPasswordAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(GenerateAdPassword));
                builder.AddAttribute(1, nameof(GenerateAdPassword.AccountId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(GenerateAdPassword.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Generate AD Password", body);
        }

        private async Task OpenDialogDeleteAccountAsync()
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteAccount));
                builder.AddAttribute(1, nameof(DeleteAccount.AccountId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteAccount.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete Account", body);
        }

        private async Task OpenDialogResendInvitationAsync(SoftwareVaultInvitation softwareVaultInvitation)
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SoftwareVaults.ResendSoftwareVaultInvitation));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadEmployeeAsync));
                builder.AddAttribute(2, "SoftwareVaultInvitation", softwareVaultInvitation);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Resend invitation", body);
        }

        private async Task OpenDialogDeleteInvitationAsync(SoftwareVaultInvitation softwareVaultInvitation)
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SoftwareVaults.DeleteSoftwareVaultInvitation));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadEmployeeAsync));
                builder.AddAttribute(2, "SoftwareVaultInvitation", softwareVaultInvitation);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete invitation", body);
        }

        private async Task OpenDialogSoftwareVaultDetailsAsync(SoftwareVault softwareVault)
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SoftwareVaultDetails));
                builder.AddAttribute(1, "SoftwareVault", softwareVault);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Software vault details", body);
        }

        private async Task OpenDialogHardwareVaultDetailsAsync(HardwareVault hardwareVault)
        {
            if (!await VerifyAdUserAsync()) return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(HardwareVaultDetails));
                builder.AddAttribute(1, "HardwareVault", hardwareVault);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Hardware vault details", body);
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

        #endregion

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On<string>(RefreshPage.EmployeesDetails, async (employeeId) =>
             {
                 if (employeeId != EmployeeId)
                     return;

                 await LoadEmployeeAsync();
                 await MainTableService.LoadTableDataAsync();
                 await ToastService.ShowToastAsync("Page updated by another admin.", ToastType.Notify);
             });

            hubConnection.On<string>(RefreshPage.EmployeesDetailsVaultSynced, async (employeeId) =>
            {
                if (employeeId != EmployeeId)
                    return;

                await LoadEmployeeAsync();
                await ToastService.ShowToastAsync("Hardware vault sync completed.", ToastType.Notify);
            });

            hubConnection.On<string>(RefreshPage.EmployeesDetailsVaultState, async (employeeId) =>
            {
                if (employeeId != EmployeeId)
                    return;

                await LoadEmployeeAsync();
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            if (hubConnection?.State == HubConnectionState.Connected)
                hubConnection.DisposeAsync();

            MainTableService.Dispose();
        }
    }
}