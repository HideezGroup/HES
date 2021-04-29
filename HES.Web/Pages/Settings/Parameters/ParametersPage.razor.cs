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
    public partial class ParametersPage : HESPageBase, IDisposable
    {
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<ParametersPage> Logger { get; set; }

        public LicensingSettings LicensingSettings { get; set; }
        public LdapSettings LdapSettings { get; set; }

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
            LicensingSettings = await AppSettingsService.GetLicenseSettingsAsync();
            LdapSettings = await AppSettingsService.GetLdapSettingsAsync();
        }

        #region License

        private async Task OpenDialogLicensingSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddLicenseSettings));
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

        private async Task OpenDialogDeleteLicenseSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteLicenseSettings));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Delete Settings", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadDataSettingsAsync();
                await PageSyncService.UpdateParameters(PageId);
            }
        }

        #endregion

        #region Ldap

        private async Task OpenDialogLdapSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddLdapSettings));
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

        private async Task OpenDialogDeleteLdapSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteLdapSettings));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Delete Settings", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadDataSettingsAsync();
                await PageSyncService.UpdateParameters(PageId);
            }
        }

        #endregion

        public void Dispose()
        {
            PageSyncService.UpdateParametersPage -= UpdateParametersPage;
        }
    }
}