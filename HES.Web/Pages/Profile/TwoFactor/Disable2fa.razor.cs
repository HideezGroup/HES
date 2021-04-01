using HES.Core.Enums;
using HES.Core.Helpers;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.TwoFactor
{
    public partial class Disable2fa : HESModalBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IHttpClientFactory HttpClientFactory { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<Disable2fa> Logger { get; set; }

        protected override void OnInitialized()
        {
            SetInitialized();
        }

        private async Task DisableTwoFactorAsync()
        {
            try
            {
                var client = await HttpClientHelper.CreateClientAsync(NavigationManager, HttpClientFactory, JSRuntime, Logger);
                var response = await client.PostAsync("api/Identity/DisableTwoFactor", new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                await ToastService.ShowToastAsync("2fa has been disabled. You can reenable 2fa when you setup an authenticator app", ToastType.Success);
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