using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class LicenseSettingsDialog : HESModalBase
    {
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<LicenseSettingsDialog> Logger { get; set; }
        [Parameter] public LicensingSettings LicensingSettings { get; set; }
        public Button Button { get; set; }

        public string InputType { get; private set; }

        protected override void OnInitialized()
        {
            InputType = "Password";
        }

        private async Task UpdateLicensingSettingsAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await AppSettingsService.SetLicensingSettingsAsync(LicensingSettings);
                    await ToastService.ShowToastAsync("License settings updated.", ToastType.Success);
                    await ModalDialogClose();
                });
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