using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
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
    public partial class EmployeeDisableSso : HESComponentBase
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IEmailSenderService EmailSenderService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public ILogger<EmployeeDisableSso> Logger { get; set; }
        public IEmployeeService EmployeeService { get; set; }

        [Parameter] public EventCallback Refresh { get; set; }
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

        public async Task DisableEmployeeSsoAsync()
        {
            try
            {
                await EmployeeService.DisableSsoAsync(Employee);
                await EmailSenderService.SendEmployeeDisableSsoAsync(Employee.Email);
                await SynchronizationService.UpdateEmployeeDetails(ExceptPageId, Employee.Id);
                await ToastService.ShowToastAsync($"SSO for employee {Employee.Email} disabled.", ToastType.Success);
                await Refresh.InvokeAsync();
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