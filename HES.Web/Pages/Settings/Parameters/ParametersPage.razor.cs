using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        public List<SamlRelyingParty> SamlRelyingParties { get; set; }

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
            SamlRelyingParties = await AppSettingsService.GetSaml2RelyingPartiesAsync();
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

        #region Splunk

        private async Task OpenDialogSplunkSettingsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSplunkSettings));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Splunk Settings", body, ModalDialogSize.Default);
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

            var instance = await ModalDialogService.ShowAsync("Delete Settings", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadDataSettingsAsync();
                await PageSyncService.UpdateParameters(PageId);
            }
        }

        #endregion

        #region Saml

        private async Task OpenDialogAddSaml2RelyingPartyAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSaml2RelyingParty));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Add Service Provider", body, ModalDialogSize.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadDataSettingsAsync();
                await PageSyncService.UpdateParameters(PageId);
            }
        }

        private async Task OpenDialogEditSaml2RelyingPartyAsync(string relyingPartyId)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSaml2RelyingParty));
                builder.AddAttribute(1, nameof(EditSaml2RelyingParty.RelyingPartyId), relyingPartyId);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Edit Service Provider", body, ModalDialogSize.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadDataSettingsAsync();
                await PageSyncService.UpdateParameters(PageId);
            }
        }

        private async Task OpenDialogDeleteSaml2RelyingPartyAsync(string relyingPartyId)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteSaml2RelyingParty));
                builder.AddAttribute(1, nameof(DeleteSaml2RelyingParty.RelyingPartyId), relyingPartyId);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Delete Service Provider", body, ModalDialogSize.Default);
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