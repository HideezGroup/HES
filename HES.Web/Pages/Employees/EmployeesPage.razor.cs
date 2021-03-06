﻿using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Filters;
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
        public IDataTableService<Employee, EmployeeFilter> DataTableService { get; set; }
        [Inject] public ILogger<EmployeesPage> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<Employee, EmployeeFilter>>();

                PageSyncService.UpdateEmployeePage += UpdateEmployeePage;

                await BreadcrumbsService.SetEmployees();
                await DataTableService.InitializeAsync(EmployeeService.GetEmployeesAsync, EmployeeService.GetEmployeesCountAsync, StateHasChanged, nameof(Employee.FullName));

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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Employees_SyncEmployeesWithAD_Title, body, ModalDialogSize.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateEmployees(PageId);
            }
        }

        private async Task EmployeeDetailsAsync()
        {
            await InvokeAsync(() =>
            {
                NavigationManager.NavigateTo($"{Routes.EmployeesDetails}{DataTableService.SelectedEntity.Id}");
            });
        }

        private async Task CreateEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateEmployee));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Employees_CreateEmployee_Title, body, ModalDialogSize.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateEmployees(PageId);
            }
        }

        private async Task EditEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditEmployee));
                builder.AddAttribute(1, nameof(EditEmployee.EmployeeId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Employees_EditEmployee_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateEmployees(PageId);
            }
        }

        private async Task DeleteEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteEmployee));
                builder.AddAttribute(1, nameof(DeleteEmployee.EmployeeId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Employees_DeleteEmployee_Title, body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateEmployees(PageId);
            }
        }

        private async Task UpdateEmployeePage(string exceptPageId)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
                StateHasChanged();
            });
        }

        public void Dispose()
        {
            PageSyncService.UpdateEmployeePage -= UpdateEmployeePage;
        }
    }
}