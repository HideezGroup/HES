using HES.Core.Enums;
using HES.Core.Helpers;
using HES.Core.Models.ApplicationUsers;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.TwoFactor
{
    public partial class EnableAuthenticator : HESModalBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<EnableAuthenticator> Logger { get; set; }

        public VerificationCode VerificationCode { get; set; } = new VerificationCode();
        public SharedKeyInfo SharedKeyInfo { get; set; } = new SharedKeyInfo();
        public string[] RecoveryCodes { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await LoadSharedKeyAndQrCodeUriAsync();
                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await GenerateQrCodeAsync();
            }
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync()
        {
            var client = await HttpClientHelper.CreateClientAsync(NavigationManager, HttpClientFactory, JSRuntime, Logger);
            var response = client.GetAsync("api/Identity/LoadSharedKeyAndQrCodeUri").Result;

            if (!response.IsSuccessStatusCode)
                throw new Exception(await response.Content.ReadAsStringAsync());

            SharedKeyInfo = JsonConvert.DeserializeObject<SharedKeyInfo>(await response.Content.ReadAsStringAsync());
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
                var client = await HttpClientHelper.CreateClientAsync(NavigationManager, HttpClientFactory, JSRuntime, Logger);
                var response = await client.PostAsync("api/Identity/VerifyTwoFactor", (new StringContent(JsonConvert.SerializeObject(VerificationCode), Encoding.UTF8, "application/json")));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                var verifyTwoFactorTokenInfo = JsonConvert.DeserializeObject<VerifyTwoFactorInfo>(await response.Content.ReadAsStringAsync());

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