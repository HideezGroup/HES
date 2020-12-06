using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Employees;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class EmployeesPage : OwningComponentBase, IDisposable
    {
        public IEmployeeService EmployeeService { get; set; }
        public IMainTableService<Employee, EmployeeFilter> MainTableService { get; set; }
        [Inject] public ISynchronizationService SynchronizationService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EmployeesPage> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }
        public string PageId { get; set; }

        private HubConnection hubConnection;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                EmployeeService = ScopedServices.GetRequiredService<IEmployeeService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<Employee, EmployeeFilter>>();

                InitializePageUpdate();

                //await InitializeHubAsync();
                await BreadcrumbsService.SetEmployees();
                await MainTableService.InitializeAsync(EmployeeService.GetEmployeesAsync, EmployeeService.GetEmployeesCountAsync, ModalDialogService, StateHasChanged, nameof(Employee.FullName));

                Initialized = true;
            }
            catch (Exception ex)
            {
                LoadFailed = true;
                ErrorMessage = ex.Message;
                Logger.LogError(ex.Message);
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
                builder.AddAttribute(1, nameof(SyncEmployeesWithAD.ConnectionId), hubConnection?.ConnectionId);
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
                builder.AddAttribute(1, "ConnectionId", hubConnection?.ConnectionId);
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
                builder.AddAttribute(2, nameof(EditEmployee.ConnectionId), PageId);
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
                builder.AddAttribute(2, nameof(DeleteEmployee.ConnectionId), hubConnection?.ConnectionId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Employee", body);
        }

        private void InitializePageUpdate()
        {
            SynchronizationService.UpdateEmployeePage += UpdateEmployeePage;
            PageId = Guid.NewGuid().ToString();
        }

        private async Task UpdateEmployeePage(string pageId, string userName)
        {
            if (PageId != pageId)
            {
                await InvokeAsync(async () =>
                {
                    await MainTableService.LoadTableDataAsync();
                    await ToastService.ShowToastAsync($"Page edited by {userName}.", ToastType.Notify);
                    StateHasChanged();
                });
            }
        }

        private async Task InitializeHubAsync()
        {
            hubConnection = new HubConnectionBuilder()
            .WithUrl(NavigationManager.ToAbsoluteUri("/refreshHub"))
            .Build();

            hubConnection.On(RefreshPage.Employees, async () =>
            {
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync("Page updated by another admin.", ToastType.Notify);
            });

            await hubConnection.StartAsync();
        }

        public void Dispose()
        {
            //if (hubConnection?.State == HubConnectionState.Connected)
            //    hubConnection.DisposeAsync();

            SynchronizationService.UpdateEmployeePage -= UpdateEmployeePage;
            MainTableService.Dispose();
        }
    }
}