using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class AddLdapSettings : HESModalBase
    {
        public ILdapService LdapService { get; set; }
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddLdapSettings> Logger { get; set; }

        public LdapSettings LdapSettings { get; set; } = new LdapSettings();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button Button { get; set; }

        protected override void OnInitialized()
        {
            LdapService = ScopedServices.GetRequiredService<ILdapService>();
            AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                 {
                     await LdapService.ValidateCredentialsAsync(LdapSettings);
                     await AppSettingsService.SetLdapSettingsAsync(LdapSettings);
                     await ToastService.ShowToastAsync(Resources.Resource.Parameters_AddLdapSettings_Toast, ToastType.Success);
                     await ModalDialogClose();
                 });
            }
            catch (LdapException ex) when (ex.ResultCode == LdapException.InvalidCredentials)
            {
                ValidationErrorMessage.DisplayError(nameof(LdapSettings.Password), Resources.Resource.Parameters_AddLdapSettings_InvalidCredentials);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }
    }
}