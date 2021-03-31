using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class LdapSettingsDialog : HESModalBase
    {
        [Inject] public ILdapService LdapService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<LdapSettingsDialog> Logger { get; set; }
        [Parameter] public string Host { get; set; }

        public LdapSettings LdapSettings { get; set; }
        public EditContext LdapSettingsContext { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var setting = await AppSettingsService.GetLdapSettingsAsync();

            if (setting == null)
                LdapSettings = new LdapSettings() { Host = Host };
            else
                LdapSettings = new LdapSettings()
                {
                    Host = Host,
                    MaxPasswordAge = setting.MaxPasswordAge
                };

            LdapSettingsContext = new EditContext(LdapSettings);
        }

        private async Task SaveSettingsAsync()
        {
            try
            {
                var isValid = LdapSettingsContext.Validate();

                if (!isValid)
                    return;

                await LdapService.ValidateCredentialsAsync(LdapSettings);
                await AppSettingsService.SetLdapSettingsAsync(LdapSettings);
                await ToastService.ShowToastAsync("Domain settings updated.", ToastType.Success);    
                await ModalDialogClose();
            }
            catch (LdapForNet.LdapInvalidCredentialsException)
            {
                ValidationErrorMessage.DisplayError(nameof(LdapSettings.Password), "Invalid password");
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