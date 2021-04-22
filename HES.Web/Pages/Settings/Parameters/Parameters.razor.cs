﻿using HES.Core.Constants;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class Parameters : HESPageBase, IDisposable
    {
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<Parameters> Logger { get; set; }

        public LicensingSettings LicensingSettings { get; set; }
        public string DomainHost { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();
                PageSyncService.UpdateParametersPage += UpdateParametersPage;

                await BreadcrumbsService.SetParameters();
                await LoadDataSettingsAsync();

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateParametersPage(string exceptPageId)
        {
            await InvokeAsync(async () =>
            {
                await LoadDataSettingsAsync();
                StateHasChanged();
            });
        }

        private async Task LoadDataSettingsAsync()
        {
            LicensingSettings = await LoadLicensingSettingsAsync();
            DomainHost = await LoadDomainSettingsAsync();
        }

        private async Task<LicensingSettings> LoadLicensingSettingsAsync()
        {
            var settings = await AppSettingsService.GetSettingsAsync<LicensingSettings>(ServerConstants.Licensing);

            if (settings == null)
            {
                return new LicensingSettings();
            }

            return settings;
        }

        private async Task OpenDialogLicensingSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(LicenseSettingsDialog));
                builder.AddAttribute(1, nameof(LicenseSettingsDialog.LicensingSettings), LicensingSettings);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("License Settings", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadDataSettingsAsync();
                await PageSyncService.UpdateParameters(PageId);
            }
        }

        private async Task<string> LoadDomainSettingsAsync()
        {
            var settings = await AppSettingsService.GetSettingsAsync<LdapSettings>(ServerConstants.Domain);
            return settings?.Host;
        }

        private async Task OpenDialogLdapSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(LdapSettingsDialog));
                builder.AddAttribute(1, nameof(LdapSettingsDialog.Host), DomainHost);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Domain Settings", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadDataSettingsAsync();
                await PageSyncService.UpdateParameters(PageId);
            }
        }

        private async Task OpenDialogDeleteLdapCredentialsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteLdapCredentials));
                builder.CloseComponent();
            };


            var instance = await ModalDialogService.ShowAsync("Delete", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadDataSettingsAsync();
                await PageSyncService.UpdateParameters(PageId);
            }
        }

        public void Dispose()
        {
            PageSyncService.UpdateParametersPage -= UpdateParametersPage;
        }
    }
}