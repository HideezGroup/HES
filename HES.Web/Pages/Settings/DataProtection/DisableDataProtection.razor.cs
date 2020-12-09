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
    public partial class DisableDataProtection : HESComponentBase
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
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<DisableDataProtection> Logger { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public CurrentPasswordModel CurrentPassword { get; set; } = new CurrentPasswordModel();
        public ButtonSpinner ButtonSpinner { get; set; }

        private async Task DisableDataProtectionAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                    await DataProtectionService.DisableProtectionAsync(CurrentPassword.Password);
                    await Refresh.InvokeAsync(this);
                    await ToastService.ShowToastAsync("Data protection disabled.", ToastType.Success);
                    await SynchronizationService.UpdateDataProtection(ExceptPageId);
                    Logger.LogInformation($"Data protection disabled by {authState.User.Identity.Name}");
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
