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
    public partial class EmployeeEnableSso : HESComponentBase
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IEmailSenderService EmailSenderService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public ILogger<EmployeeEnableSso> Logger { get; set; }
        public IEmployeeService EmployeeService { get; set; }

        [Parameter] public Employee Employee { get; set; }
        [Parameter] public string ExceptPageId { get; set; }


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
                await ModalDialogService.CancelAsync();
            }
        }

        public async Task EnableEmployeeSsoAsync()
        {
            try
            {
                await EmployeeService.EnableSsoAsync(Employee);
                var callBack = await ApplicationUserService.GetEnableSsoCallBackUrl(Employee.Email, NavigationManager.BaseUri);
                await EmailSenderService.SendEmployeeEnableSsoAsync(Employee.Email, callBack);
                await SynchronizationService.UpdateEmployees(ExceptPageId);
                await ToastService.ShowToastAsync($"SSO for employee {Employee.Email} enabled.", ToastType.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }
    }
}
