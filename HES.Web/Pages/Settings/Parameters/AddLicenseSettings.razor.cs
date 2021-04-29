using HES.Core.Enums;
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
    public partial class AddLicenseSettings : HESModalBase
    {
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddLicenseSettings> Logger { get; set; }

        public LicensingSettings LicensingSettings { get; set; } = new LicensingSettings();
        public Button Button { get; set; }
        public string InputType { get; private set; }

        protected override void OnInitialized()
        {
            AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();
            InputType = "Password";
        }

        private async Task UpdateLicensingSettingsAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await AppSettingsService.SetLicenseSettingsAsync(LicensingSettings);
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