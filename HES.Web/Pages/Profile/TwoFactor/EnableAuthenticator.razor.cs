using HES.Core.Constants;
using HES.Core.Enums;
using HES.Core.Models.ApplicationUsers;
using HES.Web.Components;
using HES.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.TwoFactor
{
    public partial class EnableAuthenticator : HESModalBase
    {
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<EnableAuthenticator> Logger { get; set; }

        public VerificationCode VerificationCode { get; set; } = new VerificationCode();
        public SharedKeyInfo SharedKeyInfo { get; set; } = new SharedKeyInfo();
        public string[] RecoveryCodes { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            try
            {
                if (firstRender)
                {
                    await LoadSharedKeyAndQrCodeUriAsync();
                    SetInitialized();
                    StateHasChanged();
                    await GenerateQrCodeAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync()
        {
            SharedKeyInfo = await JSRuntime.InvokeWebApiGetAsync<SharedKeyInfo>(Routes.ApiLoadSharedKeyAndQrCodeUri);
        }

        private async Task GenerateQrCodeAsync()
        {
            try
            {
                await JSRuntime.InvokeVoidAsync("generateQr", SharedKeyInfo.AuthenticatorUri);
            }
            catch (Exception ex)
            {
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        private async Task VerifyTwoFactorAsync()
        {
            try
            {               
                var verifyTwoFactorTokenInfo = await JSRuntime.InvokeWebApiPostAsync<VerifyTwoFactorInfo>(Routes.ApiVerifyTwoFactor, VerificationCode);

                if (!verifyTwoFactorTokenInfo.IsTwoFactorTokenValid)
                {
                    await ToastService.ShowToastAsync(Resources.Resource.Profile_Security_EnableAuthenticator_Toast_InvalidCode, ToastType.Error);
                    return;
                }

                await ToastService.ShowToastAsync(Resources.Resource.Profile_Security_EnableAuthenticator_Toast_AuthenticatorVerified, ToastType.Success);

                if (verifyTwoFactorTokenInfo.RecoveryCodes != null)
                {
                    RecoveryCodes = verifyTwoFactorTokenInfo.RecoveryCodes.ToArray();
                }
                else
                {
                    await ModalDialogClose();
                }
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