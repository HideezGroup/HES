using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Identity;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.DataProtection
{
    public partial class DisableDataProtection : HESModalBase
    {
        [Inject] public IDataProtectionService DataProtectionService { get; set; }
        [Inject] public ILogger<DisableDataProtection> Logger { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        public DataProtectionPasswordModel CurrentPassword { get; set; } = new DataProtectionPasswordModel();
        public Button Button { get; set; }

        private async Task DisableDataProtectionAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                    await DataProtectionService.DisableProtectionAsync(CurrentPassword.Password);
                    await ToastService.ShowToastAsync(Resources.Resource.DataProtection_DisableDataProtection_Toast, ToastType.Success);
                    Logger.LogInformation($"Data protection disabled by {authState.User.Identity.Name}");
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