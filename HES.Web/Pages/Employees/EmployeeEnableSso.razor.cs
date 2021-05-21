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
    public partial class EmployeeEnableSso : HESModalBase
    {
        public IEmployeeService EmployeeService { get; set; }
        public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IEmailSenderService EmailSenderService { get; set; }
        [Inject] public ILogger<EmployeeEnableSso> Logger { get; set; }
        [Parameter] public Employee Employee { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public async Task EnableEmployeeSsoAsync()
        {
            try
            {
                await EmployeeService.EnableSsoAsync(Employee);
                var callback = await ApplicationUserService.GenerateEnableSsoCallBackUrlAsync(Employee.Email, NavigationManager.BaseUri);
                await EmailSenderService.SendEmployeeEnableSsoAsync(Employee.Email, callback);
                await ToastService.ShowToastAsync(Resources.Resource.EmployeeDetails_EnableSso_Toast, ToastType.Success);         
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
