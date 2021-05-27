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
        public SplunkSettings SplunkSettings { get; set; }

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
            try
            {
                await InvokeAsync(async () =>
                {
                    await LoadDataSettingsAsync();
                    StateHasChanged();
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private async Task LoadDataSettingsAsync()
        {
            LicensingSettings = await AppSettingsService.GetLicenseSettingsAsync();
            LdapSettings = await AppSettingsService.GetLdapSettingsAsync();
            SplunkSettings = await AppSettingsService.GetSplunkSettingsAsync();
        }

        #region License

        private async Task OpenDialogLicensingSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddLicenseSettings));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Parameters_AddLicenseSettings_Title, body, ModalDialogSize.Default);
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Parameters_DeleteLicenseSettings_Title, body, ModalDialogSize.Default);
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Parameters_AddLdapSettings_Title, body, ModalDialogSize.Default);
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Parameters_DeleteLdapSettings_Title, body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadDataSettingsAsync();
                await PageSyncService.UpdateParameters(PageId);
            }
        }

        #endregion

        #region Splunk

        private async Task OpenDialogSplunkSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSplunkSettings));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Parameters_AddSplunkSettings_Title, body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadDataSettingsAsync();
                await PageSyncService.UpdateParameters(PageId);
            }
        }

        private async Task OpenDialogDeleteSplunkSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteSplunkSettings));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Parameters_DeleteSplunkSettings_Title, body, ModalDialogSize.Default);
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