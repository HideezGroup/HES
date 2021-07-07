using HES.Core.Constants;
using HES.Core.Enums;
using HES.Web.Components;
using HES.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.TwoFactor
{
    public partial class GenerateRecoveryCodes : HESModalBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<GenerateRecoveryCodes> Logger { get; set; }
        public string[] RecoveryCodes { get; set; }

        protected override void OnInitialized()
        {
            SetInitialized();
        }

        private async Task GenerateRecoveryCodesAsync()
        {
            try
            {
                RecoveryCodes = await JSRuntime.InvokeWebApiPostAsync<string[]>(Routes.ApiGenerateNewTwoFactorRecoveryCodes, string.Empty);
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