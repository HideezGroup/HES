using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.TwoFactor
{
    public partial class Disable2fa : HESComponentBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<Disable2fa> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

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
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task DisableTwoFactorAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/DisableTwoFactor", new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                await Refresh.InvokeAsync();
                await ToastService.ShowToastAsync("2fa has been disabled. You can reenable 2fa when you setup an authenticator app", ToastType.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }         
        }
    }
}