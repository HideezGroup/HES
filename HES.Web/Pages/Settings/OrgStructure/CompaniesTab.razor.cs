using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class CompaniesTab : HESComponentBase, IDisposable
    {
        public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<CompaniesTab> Logger { get; set; }

        public List<Company> Companies { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                OrgStructureService = ScopedServices.GetRequiredService<IOrgStructureService>();
                SynchronizationService.UpdateOrgSructureCompaniesPage += UpdateOrgSructureCompaniesPage;
                await BreadcrumbsService.SetOrgStructure();
                await LoadCompaniesAsync();

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateOrgSructureCompaniesPage(string exceptPageId, string userName)
        {

            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await LoadCompaniesAsync();
                await ToastService.ShowToastAsync($"Page edited by {userName}.", ToastType.Notify);
                StateHasChanged();
            });

        }

        private async Task LoadCompaniesAsync()
        {
            Companies = await OrgStructureService.GetCompaniesAsync();
            StateHasChanged();
        }

        private async Task OpenDialogCreateCompanyAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateCompany));
                builder.AddAttribute(1, nameof(CreateCompany.ExceptPageId), PageId);
                builder.AddAttribute(2, nameof(CreateCompany.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create Company", body);
        }

        private async Task OpenDialogEditCompanyAsync(Company company)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditCompany));
                builder.AddAttribute(1, nameof(EditCompany.CompanyId), company.Id);
                builder.AddAttribute(2, nameof(EditCompany.ExceptPageId), PageId);
                builder.AddAttribute(3, nameof(EditCompany.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit Company", body);
        }

        private async Task OpenDialogDeleteCompanyAsync(Company company)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteCompany));
                builder.AddAttribute(1, nameof(DeleteCompany.CompanyId), company.Id);
                builder.AddAttribute(2, nameof(DeleteCompany.ExceptPageId), PageId);
                builder.AddAttribute(3, nameof(DeleteCompany.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete Company", body);
        }

        private async Task OpenDialogCreateDepartmentAsync(Company company)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateDepartment));
                builder.AddAttribute(1, nameof(CreateDepartment.CompanyId), company.Id);
                builder.AddAttribute(2, nameof(CreateDepartment.ExceptPageId), PageId);
                builder.AddAttribute(3, nameof(CreateDepartment.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create Department", body);
        }

        private async Task OpenDialogEditDepartmentAsync(Department department)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditDepartment));
                builder.AddAttribute(1, nameof(EditDepartment.DepartmentId), department.Id);
                builder.AddAttribute(2, nameof(EditDepartment.ExceptPageId), PageId);
                builder.AddAttribute(3, nameof(EditDepartment.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit Department", body);
        }

        private async Task OpenDialogDeleteDepartmentAsync(Department department)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteDepartment));
                builder.AddAttribute(1, nameof(DeleteDepartment.DepartmentId), department.Id);
                builder.AddAttribute(2, nameof(DeleteDepartment.ExceptPageId), PageId);
                builder.AddAttribute(3, nameof(DeleteDepartment.Refresh), EventCallback.Factory.Create(this, LoadCompaniesAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete Department", body);
        }

        public void Dispose()
        {
            SynchronizationService.UpdateOrgSructureCompaniesPage -= UpdateOrgSructureCompaniesPage;
        }
    }
}