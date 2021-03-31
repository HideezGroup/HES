using HES.Core.Enums;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.TwoFactor
{
    public partial class ResetAuthenticator : HESModalBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public ILogger<ResetAuthenticator> Logger { get; set; }

        protected override void OnInitialized()
        {
            SetInitialized();
        }

        private async Task ResetAuthenticatorKeyAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/ResetAuthenticatorKey", new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                await ToastService.ShowToastAsync("Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.", ToastType.Success);
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