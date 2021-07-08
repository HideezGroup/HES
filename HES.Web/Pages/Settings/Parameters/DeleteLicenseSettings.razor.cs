using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class DeleteLicenseSettings : HESModalBase
    {
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<DeleteLicenseSettings> Logger { get; set; }

        protected override void OnInitialized()
        {
            AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();
        }

        private async Task DeleteAsync()
        {
            try
            {
                await AppSettingsService.RemoveLicenseSettingsAsync();
                await ToastService.ShowToastAsync(Resources.Resource.Parameters_DeleteLicenseSettings_Toast, ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }
    }
}