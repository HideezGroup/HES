using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Core.Helpers
{
    public class HttpClientHelper
    {
        public static async Task<HttpClient> CreateClientAsync<T>(NavigationManager navigationManager, IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime, ILogger<T> logger)
        {
            string cookie = null;
            try
            {
                cookie = await jsRuntime.InvokeAsync<string>("getCookie");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }

            var client = httpClientFactory.CreateClient("HES");
            client.BaseAddress = new Uri(navigationManager.BaseUri);

            if (!string.IsNullOrWhiteSpace(cookie))
            {
                client.DefaultRequestHeaders.Add("Cookie", cookie);
            }

            return client;
        }
    }
}