using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Employees;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EmployeesPage : HESPageBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IMainTableService<Employee, EmployeeFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; } // TODO remove
        [Inject] public ILogger<EmployeesPage> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<Employee, EmployeeFilter>>();

                SynchronizationService.UpdateEmployeePage += UpdateEmployeePage;

                await BreadcrumbsService.SetEmployees();
                await MainTableService.InitializeAsync(EmployeeService.GetEmployeesAsync, EmployeeService.GetEmployeesCountAsync, ModalDialogService, StateHasChanged, nameof(Employee.FullName));
                
                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task SyncEmployeesWithAdAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SyncEmployeesWithAD));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Sync with Active Directory", body, ModalDialogSize2.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await MainTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateEmployees(PageId);
            }
        }

        private async Task EmployeeDetailsAsync()
        {
            await InvokeAsync(() =>
            {
                NavigationManager.NavigateTo($"/Employees/Details/{MainTableService.SelectedEntity.Id}");
            });
        }

        private async Task CreateEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateEmployee));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Create Employee", body, ModalDialogSize2.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await MainTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateEmployees(PageId);
            }
        }

        private async Task EditEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditEmployee));
                builder.AddAttribute(1, nameof(EditEmployee.EmployeeId), MainTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Edit Employee", body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await MainTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateEmployees(PageId);
            }
        }

        private async Task DeleteEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteEmployee));
                builder.AddAttribute(1, nameof(DeleteEmployee.EmployeeId), MainTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Delete Employee", body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await MainTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateEmployees(PageId);
            }
        }

        private async Task UpdateEmployeePage(string exceptPageId, string userName)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync($"Page edited by {userName}.", ToastType.Notify);
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            SynchronizationService.UpdateEmployeePage -= UpdateEmployeePage;
            MainTableService.Dispose();
        }
    }
}