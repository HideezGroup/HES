using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Identity;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.PersonalData
{
    public partial class DeletePersonalData : HESComponentBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<DeletePersonalData> Logger { get; set; }
        [Parameter] public ApplicationUser ApplicationUser { get; set; }

        public UserPasswordModel PasswordModel { get; set; } = new UserPasswordModel();
        public Button ButtonSpinner { get; set; }

        protected override void OnInitialized()
        {
            SetInitialized();
        }

        private async Task DeletePersonalDataAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    List<string> cookies = null;
                    if (HttpClient.DefaultRequestHeaders.TryGetValues("Cookie", out IEnumerable<string> cookieEntries))
                        cookies = cookieEntries.ToList();

                    var response = await HttpClient.PostAsync("api/Identity/DeletePersonalData", (new StringContent(JsonConvert.SerializeObject(PasswordModel), Encoding.UTF8, "application/json")));

                    if (!response.IsSuccessStatusCode)
                        throw new Exception(await response.Content.ReadAsStringAsync());

                    if (cookies != null && cookies.Any())
                    {
                        HttpClient.DefaultRequestHeaders.Remove("Cookie");

                        foreach (var cookie in cookies[0].Split(';'))
                        {
                            var cookieParts = cookie.Split('=');
                            await JSRuntime.InvokeVoidAsync("removeCookie", cookieParts[0]);
                        }
                    }

                    NavigationManager.NavigateTo(Routes.Login, true);
                });
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