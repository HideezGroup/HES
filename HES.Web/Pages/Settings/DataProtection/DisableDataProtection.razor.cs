using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.DataProtection
{
    public partial class DisableDataProtection : HESModalBase
    {
        public class CurrentPasswordModel
        {
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }
        }

        [Inject] public IDataProtectionService DataProtectionService { get; set; }
        [Inject] public ILogger<DisableDataProtection> Logger { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

        public CurrentPasswordModel CurrentPassword { get; set; } = new CurrentPasswordModel();
        public Button Button { get; set; }

        private async Task DisableDataProtectionAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                    await DataProtectionService.DisableProtectionAsync(CurrentPassword.Password);
                    await ToastService.ShowToastAsync("Data protection disabled.", ToastType.Success);
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
