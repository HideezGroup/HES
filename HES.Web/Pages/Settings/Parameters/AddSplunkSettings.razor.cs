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
    public partial class AddSplunkSettings : HESModalBase
    {
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddSplunkSettings> Logger { get; set; }

        public SplunkSettings SplunkSettings { get; set; } = new SplunkSettings();
        public Button Button { get; set; }

        protected override void OnInitialized()
        {
            AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();
        }

        private async Task UpdateSettingsAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await AppSettingsService.SetSplunkSettingsAsync(SplunkSettings);
                    await ToastService.ShowToastAsync(Resources.Resource.Parameters_AddSplunkSettings_Toast, ToastType.Success);
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