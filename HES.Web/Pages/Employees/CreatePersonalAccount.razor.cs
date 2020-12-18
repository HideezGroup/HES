using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class CreatePersonalAccount : HESComponentBase
    {
        public IEmployeeService EmployeeService { get; set; }
        public ITemplateService TemplateService { get; set; }
        public ILdapService LdapService { get; set; }
        public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<CreatePersonalAccount> Logger { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public string EmployeeId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }

        public Employee Employee { get; set; }
        public AccountAddModel PersonalAccount { get; set; }
        public List<Template> Templates { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                TemplateService = ScopedServices.GetRequiredService<ITemplateService>();
                LdapService = ScopedServices.GetRequiredService<ILdapService>();
                RemoteDeviceConnectionsService = ScopedServices.GetRequiredService<IRemoteDeviceConnectionsService>();

                Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId);
                Templates = await TemplateService.GetTemplatesAsync();
                PersonalAccount = new AccountAddModel() { EmployeeId = EmployeeId };

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task CreateAccountAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await EmployeeService.CreatePersonalAccountAsync(PersonalAccount);
                    RemoteDeviceConnectionsService.StartUpdateHardwareVaultAccounts(await EmployeeService.GetEmployeeVaultIdsAsync(EmployeeId));
                    await Refresh.InvokeAsync(this);
                    await ToastService.ShowToastAsync("Account created.", ToastType.Success);
                    await SynchronizationService.UpdateEmployeeDetails(ExceptPageId, EmployeeId);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(PersonalAccount.Name), ex.Message);
            }
            catch (IncorrectUrlException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(PersonalAccount.Urls), ex.Message);
            }
            catch (IncorrectOtpException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(PersonalAccount.OtpSecret), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private void TemplateSelected(ChangeEventArgs e)
        {
            var template = Templates.FirstOrDefault(x => x.Id == e.Value.ToString());
            if (template != null)
            {
                PersonalAccount.Name = template.Name;
                PersonalAccount.Urls = template.Urls;
                PersonalAccount.Apps = template.Apps;
            }
        }

        private async Task CloseAsync()
        {
            await ModalDialogService.CloseAsync();
        }
    }
}