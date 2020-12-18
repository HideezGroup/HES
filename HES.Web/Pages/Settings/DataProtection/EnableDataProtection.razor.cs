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
    public partial class EnableDataProtection : HESComponentBase
    {
        public class NewPasswordModel
        {
            [Required]
            [Display(Name = "Password")]
            [DataType(DataType.Password)]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        [Inject] public IDataProtectionService DataProtectionService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EnableDataProtection> Logger { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public NewPasswordModel NewPassword { get; set; } = new NewPasswordModel();
        public ButtonSpinner ButtonSpinner { get; set; }

        private async Task EnableDataProtectionAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                    await DataProtectionService.EnableProtectionAsync(NewPassword.Password);
                    await Refresh.InvokeAsync(this);
                    await ToastService.ShowToastAsync("Data protection enabled.", ToastType.Success);
                    await SynchronizationService.UpdateDataProtection(ExceptPageId);
                    Logger.LogInformation($"Data protection enabled by {authState.User.Identity.Name}");
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
            finally
            {
                await ModalDialogService.CloseAsync();
            }
        }
    }
}