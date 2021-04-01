using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class DeleteEmployee : HESModalBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteEmployee> Logger { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public Employee Employee { get; set; }
        public bool EmployeeHasVault { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();

                Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId);
                if (Employee == null)
                    throw new HESException(HESCode.EmployeeNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(Employee.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Employee.Id, Employee);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        protected override void OnParametersSet()
        {
            EmployeeHasVault = Employee.HardwareVaults.Count > 0 || Employee.SoftwareVaults.Count > 0;
        }

        public async Task DeleteEmployeeAsync()
        {
            try
            {
                await EmployeeService.DeleteEmployeeAsync(Employee.Id);
                await ToastService.ShowToastAsync("Employee removed.", ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Employee.Id);
        }
    }
}