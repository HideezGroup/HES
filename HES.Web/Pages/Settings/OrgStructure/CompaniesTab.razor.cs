﻿using HES.Core.Entities;
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
    public partial class CompaniesTab : HESPageBase, IDisposable
    {
        public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
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
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Create Company", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadCompaniesAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        private async Task OpenDialogEditCompanyAsync(Company company)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditCompany));
                builder.AddAttribute(1, nameof(EditCompany.CompanyId), company.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Edit Company", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadCompaniesAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        private async Task OpenDialogDeleteCompanyAsync(Company company)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteCompany));
                builder.AddAttribute(1, nameof(DeleteCompany.CompanyId), company.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Delete Company", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadCompaniesAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        private async Task OpenDialogCreateDepartmentAsync(Company company)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateDepartment));
                builder.AddAttribute(1, nameof(CreateDepartment.CompanyId), company.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Create Departmen", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadCompaniesAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        private async Task OpenDialogEditDepartmentAsync(Department department)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditDepartment));
                builder.AddAttribute(1, nameof(EditDepartment.DepartmentId), department.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Edit Departmen", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadCompaniesAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        private async Task OpenDialogDeleteDepartmentAsync(Department department)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteDepartment));
                builder.AddAttribute(1, nameof(DeleteDepartment.DepartmentId), department.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Delete Departmen", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadCompaniesAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        public void Dispose()
        {
            SynchronizationService.UpdateOrgSructureCompaniesPage -= UpdateOrgSructureCompaniesPage;
        }
    }
}