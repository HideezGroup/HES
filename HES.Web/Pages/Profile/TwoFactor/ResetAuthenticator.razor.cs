using HES.Core.Constants;
using HES.Core.Enums;
using HES.Web.Components;
using HES.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.TwoFactor
{
    public partial class ResetAuthenticator : HESModalBase
    {
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<ResetAuthenticator> Logger { get; set; }

        protected override void OnInitialized()
        {
            SetInitialized();
        }

        private async Task ResetAuthenticatorKeyAsync()
        {
            try
            {              
                await JSRuntime.InvokeWebApiPostVoidAsync(Routes.ApiResetAuthenticatorKey);
                await ToastService.ShowToastAsync(Resources.Resource.Profile_Security_ResetAuthenticator_Toast, ToastType.Success);
                await ModalDialogClose();
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