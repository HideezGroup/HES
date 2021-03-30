using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EmployeeDisableSso : HESModalBase
    {
        public IEmployeeService EmployeeService { get; set; }
        [Inject] public IEmailSenderService EmailSenderService { get; set; }
        [Inject] public ILogger<EmployeeDisableSso> Logger { get; set; }
        [Parameter] public Employee Employee { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public async Task DisableEmployeeSsoAsync()
        {
            try
            {
                await EmployeeService.DisableSsoAsync(Employee);
                await EmailSenderService.SendEmployeeDisableSsoAsync(Employee.Email);
                await ToastService.ShowToastAsync($"SSO disabled.", ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }
    }
}