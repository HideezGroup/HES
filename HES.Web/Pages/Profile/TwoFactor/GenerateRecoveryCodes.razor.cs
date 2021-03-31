using HES.Core.Enums;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.TwoFactor
{
    public partial class GenerateRecoveryCodes : HESModalBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public ILogger<GenerateRecoveryCodes> Logger { get; set; }
        public string[] RecoveryCodes { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task GenerateRecoveryCodesAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/GenerateNewTwoFactorRecoveryCodes", new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                var recoveryCodes = JsonConvert.DeserializeObject<List<string>>(await response.Content.ReadAsStringAsync());

                RecoveryCodes = recoveryCodes.ToArray();               
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