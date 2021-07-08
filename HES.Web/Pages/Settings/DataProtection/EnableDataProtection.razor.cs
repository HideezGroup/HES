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
    public partial class EnableDataProtection : HESModalBase
    {
        [Inject] public IDataProtectionService DataProtectionService { get; set; }
        [Inject] public ILogger<EnableDataProtection> Logger { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        public DataProtectionNewPasswordModel NewPassword { get; set; } = new DataProtectionNewPasswordModel();
        public Button Button { get; set; }

        private async Task EnableDataProtectionAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                    await DataProtectionService.EnableProtectionAsync(NewPassword.Password);
                    await ToastService.ShowToastAsync(Resources.Resource.DataProtection_EnableDataProtection_Toast, ToastType.Success);
                    Logger.LogInformation($"Data protection enabled by {authState.User.Identity.Name}");
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