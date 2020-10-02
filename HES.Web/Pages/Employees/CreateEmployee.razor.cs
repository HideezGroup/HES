using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Accounts;
using HES.Core.Models.Web.HardwareVaults;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Web.Pages.Employees
{
    public partial class CreateEmployee : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public ISharedAccountService SheredAccountSevice { get; set; }
        [Inject] public IEmailSenderService EmailSenderService { get; set; }
        [Inject] public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public ILogger<CreateEmployee> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public List<HardwareVault> HardwareVaults { get; set; }
        public List<Company> Companies { get; set; }
        public List<Department> Departments { get; set; }
        public List<Position> Positions { get; set; }
        public List<SharedAccount> SharedAccounts { get; set; }
        public ValidationErrorMessage EmployeeValidationErrorMessage { get; set; }
        public WorkstationAccountType WorkstationType { get; set; }
        public AccountType AccountType { get; set; }

        public string WarningMessage { get; set; }
        public bool Initialized { get; set; }

        public WizardStep WizardStep { get; set; }
        public Employee Employee { get; set; }
        public EditContext EmployeeContext { get; set; }
        public HardwareVault SelectedHardwareVault { get; set; }
        public WorkstationAccount WorkstationAccount { get; set; }
        public EditContext WorkstationAccountContext { get; set; }
        public WorkstationDomain WorkstationDomain { get; set; }
        public EditContext WorkstationDomainContext { get; set; }
        public string SharedAccountId { get; set; }
        public bool AccountSkiped { get; set; }
        public string InputType { get; private set; } = "Password";
        public string Code { get; set; }
        public string Email { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            Companies = await OrgStructureService.GetCompaniesAsync();
            Departments = new List<Department>();
            Positions = await OrgStructureService.GetPositionsAsync();
            SharedAccounts = await SheredAccountSevice.GetWorkstationSharedAccountsAsync();
            SharedAccountId = SharedAccounts.FirstOrDefault()?.Id;
            await LoadHardwareVaultsAsync();

            Employee = new Employee() { Id = Guid.NewGuid().ToString() };
            EmployeeContext = new EditContext(Employee);
            WorkstationAccount = new WorkstationAccount() { EmployeeId = Employee.Id };
            WorkstationAccountContext = new EditContext(WorkstationAccount);
            WorkstationDomain = new WorkstationDomain() { EmployeeId = Employee.Id };
            WorkstationDomainContext = new EditContext(WorkstationDomain);

            Initialized = true;
        }

        private async Task Next()
        {
            switch (WizardStep)
            {
                case WizardStep.Profile:
                    var employeeNameExist = await EmployeeService.CheckEmployeeNameExistAsync(Employee);
                    if (employeeNameExist)
                    {
                        EmployeeValidationErrorMessage.DisplayError(nameof(Core.Entities.Employee.FirstName), $"{Employee.FirstName} {Employee.LastName} already exists.");
                        return;
                    }
                    var employeeIsValid = EmployeeContext.Validate();
                    if (!employeeIsValid)
                        return;
                    WizardStep = WizardStep.HardwareVault;
                    break;
                case WizardStep.HardwareVault:
                    if (SelectedHardwareVault == null)
                    {
                        WarningMessage = "Please, select a vault.";
                        break;
                    }
                    WizardStep = WizardStep.WorkstationAccount;
                    break;
                case WizardStep.WorkstationAccount:
                    if (AccountType == AccountType.Personal)
                    {
                        if (WorkstationType == WorkstationAccountType.Domain)
                        {
                            var workstationDomainIsValid = WorkstationDomainContext.Validate();
                            if (!workstationDomainIsValid)
                                return;
                            WorkstationAccount.Type = WorkstationType;
                        }
                        else
                        {
                            var workstationAccountIsValid = WorkstationAccountContext.Validate();
                            if (!workstationAccountIsValid)
                                return;
                            WorkstationDomain.Type = WorkstationType;
                        }
                    }
                    WizardStep = WizardStep.Overview;
                    break;
                case WizardStep.Overview:
                    await CreateAsync();
                    if (SelectedHardwareVault == null)
                    {
                        await ToastService.ShowToastAsync("Employee created.", ToastType.Success);
                        await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Employees);
                        await ModalDialogService.CloseAsync();
                        break;
                    }
                    Code = await HardwareVaultService.GetVaultActivationCodeAsync(SelectedHardwareVault.Id);
                    Email = Employee.Email;
                    WizardStep = WizardStep.Activation;
                    break;
                case WizardStep.Activation:
                    await ToastService.ShowToastAsync("Employee created.", ToastType.Success);
                    await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Employees);
                    await ModalDialogService.CloseAsync();
                    break;
            }
        }

        private void Back()
        {
            switch (WizardStep)
            {
                case WizardStep.Profile:
                    break;
                case WizardStep.HardwareVault:
                    WizardStep = WizardStep.Profile;
                    break;
                case WizardStep.WorkstationAccount:
                    WizardStep = WizardStep.HardwareVault;
                    break;
                case WizardStep.Overview:
                    AccountSkiped = false;
                    WizardStep = WizardStep.WorkstationAccount;
                    break;
            }
        }

        #region Hardware Vault

        public int TotalRecords { get; set; }
        public string SearchText { get; set; } = string.Empty;

        private async Task LoadHardwareVaultsAsync()
        {
            var filter = new HardwareVaultFilter() { Status = VaultStatus.Ready };
            TotalRecords = await HardwareVaultService.GetVaultsCountAsync(new DataLoadingOptions<HardwareVaultFilter>
            {
                SearchText = SearchText,
                Filter = filter
            });

            HardwareVaults = await HardwareVaultService.GetVaultsAsync(new DataLoadingOptions<HardwareVaultFilter>
            {
                Take = TotalRecords,
                SortedColumn = nameof(HardwareVault.Id),
                SortDirection = ListSortDirection.Ascending,
                SearchText = SearchText,
                Filter = filter
            });

            SelectedHardwareVault = null;
            StateHasChanged();
        }

        private async Task SelectedItemChangedAsync(HardwareVault hardwareVault)
        {
            await InvokeAsync(() =>
            {
                SelectedHardwareVault = hardwareVault;
                StateHasChanged();
            });
        }

        private async Task SearchTextChangedAsync(string searchText)
        {
            SearchText = searchText;
            await LoadHardwareVaultsAsync();
        }

        #endregion

        private void SkipAccount()
        {
            AccountSkiped = true;
            WizardStep = WizardStep.Overview;
        }

        private void SkipVault()
        {
            SelectedHardwareVault = null;
            WizardStep = WizardStep.WorkstationAccount;
        }

        private async Task CompanyChangedAsync(ChangeEventArgs args)
        {
            Departments = await OrgStructureService.GetDepartmentsByCompanyIdAsync(args.Value.ToString());
            Employee.DepartmentId = Departments.FirstOrDefault()?.Id;
        }

        private void AccountTypeChanged(AccountType accountType)
        {
            AccountType = accountType;
        }

        private async Task CreateAsync()
        {
            try
            {
                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    await EmployeeService.CreateEmployeeAsync(Employee);

                    if (SelectedHardwareVault != null)
                        await EmployeeService.AddHardwareVaultAsync(Employee.Id, SelectedHardwareVault.Id);

                    if (!AccountSkiped)
                    {
                        if (AccountType == AccountType.Personal)
                        {
                            switch (WorkstationType)
                            {
                                case WorkstationAccountType.Local:
                                case WorkstationAccountType.AzureAD:
                                case WorkstationAccountType.Microsoft:
                                    await EmployeeService.CreateWorkstationAccountAsync(WorkstationAccount);
                                    break;
                                case WorkstationAccountType.Domain:
                                    await EmployeeService.CreateWorkstationAccountAsync(WorkstationDomain);
                                    break;
                            }
                        }
                        else
                        {
                            await EmployeeService.AddSharedAccountAsync(Employee.Id, SharedAccountId);
                        }
                    }

                    transactionScope.Complete();
                }

                if (SelectedHardwareVault != null)
                    RemoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(SelectedHardwareVault.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task SendEmailAsync()
        {
            if (string.IsNullOrWhiteSpace(Email))
                return;

            await EmailSenderService.SendHardwareVaultActivationCodeAsync(new Employee() { FirstName = Employee.FirstName, LastName = Employee.LastName, Email = Email }, Code);
        }

        private async Task CopyToClipboardAsync()
        {
            await JsRuntime.InvokeVoidAsync("copyToClipboard");
        }
    }
}