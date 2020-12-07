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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EditEmployee : HESComponentBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EditEmployee> Logger { get; set; }
        [Parameter] public string EmployeeId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }

        public Employee Employee { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }
        public List<Company> Companies { get; set; }
        public List<Department> Departments { get; set; }
        public List<Position> Positions { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                OrgStructureService = ScopedServices.GetRequiredService<IOrgStructureService>();

                ModalDialogService.OnCancel += ModalDialogService_OnCancel;

                Employee = await EmployeeService.GetEmployeeByIdAsync(EmployeeId);
                if (Employee == null)
                    throw new Exception("Employee not found.");

                EntityBeingEdited = MemoryCache.TryGetValue(Employee.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Employee.Id, Employee);

                Companies = await OrgStructureService.GetCompaniesAsync();

                if (Employee.DepartmentId == null)
                {
                    Departments = new List<Department>();
                }
                else
                {
                    Departments = await OrgStructureService.GetDepartmentsByCompanyIdAsync(Employee.Department.CompanyId);
                }

                Positions = await OrgStructureService.GetPositionsAsync();

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

        public async Task OnCompanyChangeAsync(ChangeEventArgs args)
        {
            var companyId = (string)args.Value;

            if (companyId == string.Empty)
                Employee.DepartmentId = null;

            Departments = await OrgStructureService.GetDepartmentsByCompanyIdAsync(companyId);
            Employee.DepartmentId = Departments.FirstOrDefault()?.Id;
        }

        private async Task EditAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await EmployeeService.EditEmployeeAsync(Employee);
                    await ToastService.ShowToastAsync("Employee updated.", ToastType.Success);           
                    await SynchronizationService.UpdateEmployees(ExceptPageId);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Employee.FirstName), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task ModalDialogService_OnCancel()
        {
            await EmployeeService.UnchangedEmployeeAsync(Employee);
        }

        public void Dispose()
        {
            ModalDialogService.OnCancel -= ModalDialogService_OnCancel;

            if (!EntityBeingEdited)
                MemoryCache.Remove(Employee.Id);
        }
    }
}