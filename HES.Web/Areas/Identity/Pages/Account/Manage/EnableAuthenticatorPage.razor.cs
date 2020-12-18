using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppUsers;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class EnableAuthenticatorPage : ComponentBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<EnableAuthenticatorPage> Logger { get; set; }

        public VerificationCode VerificationCode { get; set; } = new VerificationCode();
        public SharedKeyInfo SharedKeyInfo { get; set; } = new SharedKeyInfo();
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await BreadcrumbsService.SetEnableAuthenticator();
                await LoadSharedKeyAndQrCodeUriAsync();
                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                LoadFailed = true;
                ErrorMessage = ex.Message;
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
            var response = HttpClient.GetAsync("api/Identity/LoadSharedKeyAndQrCodeUri").Result;

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
                var response = await HttpClient.PostAsync("api/Identity/VerifyTwoFactor", (new StringContent(JsonConvert.SerializeObject(VerificationCode), Encoding.UTF8, "application/json")));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                var verifyTwoFactorTokenInfo = JsonConvert.DeserializeObject<VerifyTwoFactorInfo>(await response.Content.ReadAsStringAsync());

                if (!verifyTwoFactorTokenInfo.IsTwoFactorTokenValid)
                {
                    await ToastService.ShowToastAsync("Verification code is invalid.", ToastType.Error);             
                    return;
                }

                await ToastService.ShowToastAsync("Your authenticator app has been verified.", ToastType.Success);

                if (verifyTwoFactorTokenInfo.RecoveryCodes != null)
                {
                    NavigationManager.NavigateTo($"/Identity/Account/Manage/ShowRecoveryCodes/{string.Join(" & ", verifyTwoFactorTokenInfo.RecoveryCodes)}", false);
                }
                else
                {
                    NavigationManager.NavigateTo("/Identity/Account/Manage/TwoFactorAuthentication", false);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }
    }
}