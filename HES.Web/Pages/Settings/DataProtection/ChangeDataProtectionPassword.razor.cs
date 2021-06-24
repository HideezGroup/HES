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
    public partial class ChangeDataProtectionPassword : HESModalBase
    {
        [Inject] public IDataProtectionService DataProtectionService { get; set; }
        [Inject] public ILogger<DisableDataProtection> Logger { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        public DataProtectionChangePasswordModel CurrentPassword { get; set; } = new DataProtectionChangePasswordModel();
        public Button Button { get; set; }

        private async Task ChangeDataProtectionPasswordAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                    await DataProtectionService.ChangeProtectionPasswordAsync(CurrentPassword.OldPassword, CurrentPassword.NewPassword);
                    await ToastService.ShowToastAsync(Resources.Resource.DataProtection_ChangeDataProtectionPassword_Toast, ToastType.Success);
                    Logger.LogInformation($"Data protection password updated by {authState.User.Identity.Name}");
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