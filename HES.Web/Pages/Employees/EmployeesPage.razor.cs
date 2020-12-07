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
    public partial class EmployeesPage : HESComponentBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IMainTableService<Employee, EmployeeFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
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


        //private async Task ImportEmployeesFromAdAsync()
        //{
        //    RenderFragment body = (builder) =>
        //    {
        //        builder.OpenComponent(0, typeof(AddEmployee));
        //        builder.AddAttribute(1, "ConnectionId", hubConnection?.ConnectionId);
        //        builder.CloseComponent();
        //    };

        //    await MainTableService.ShowModalAsync("Import from AD", body);
        //}

        private async Task SyncEmployeesWithAdAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SyncEmployeesWithAD));
                builder.AddAttribute(1, nameof(SyncEmployeesWithAD.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Sync with Active Directory", body, ModalDialogSize.Large);
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
                builder.AddAttribute(1, nameof(CreateEmployee.ExceptPageId), PageId);
                builder.CloseComponent();
            };
            await MainTableService.ShowModalAsync("Create Employee", body, ModalDialogSize.Large);
        }

        private async Task EditEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditEmployee));
                builder.AddAttribute(1, nameof(EditEmployee.EmployeeId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditEmployee.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Employee", body);
        }

        private async Task DeleteEmployeeAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteEmployee));
                builder.AddAttribute(1, nameof(DeleteEmployee.EmployeeId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteEmployee.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Employee", body);
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