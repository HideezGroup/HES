using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Core.Models.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class IdentityApiClient : IIdentityApiClient
    {
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsRuntime;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IdentityApiClient> _logger;

        public IdentityApiClient(AuthenticationStateProvider authenticationStateProvider,
                                 SignInManager<ApplicationUser> signInManager,
                                 NavigationManager navigationManager,
                                 IJSRuntime jsRuntime,
                                 IHttpClientFactory httpClientFactory,
                                 ILogger<IdentityApiClient> logger)
        {
            _authenticationStateProvider = authenticationStateProvider;
            _signInManager = signInManager;
            _navigationManager = navigationManager;
            _jsRuntime = jsRuntime;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<AuthorizationResponse> LoginWithPasswordAsync(PasswordSignInModel parameters)
        {
            var client = await CreateClientAsync();
            var stringContent = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync("api/Identity/LoginWithPassword", stringContent);
            var authorizationResponse = JsonConvert.DeserializeObject<AuthorizationResponse>(await httpResponse.Content.ReadAsStringAsync());

            await TrySetCookieAsync(httpResponse);

            if (authorizationResponse.Succeeded)
            {
                await SetAuthenticatedAsync(authorizationResponse.User);
            }

            return authorizationResponse;
        }

        public async Task<AuthorizationResponse> LoginWithFido2Async(SecurityKeySignInModel parameters)
        {
            var client = await CreateClientAsync();
            var stringContent = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
            var httpResponse = await client.PostAsync("api/Identity/LoginWithFido2", stringContent);
            var authorizationResponse = JsonConvert.DeserializeObject<AuthorizationResponse>(await httpResponse.Content.ReadAsStringAsync());

            await TrySetCookieAsync(httpResponse);

            if (authorizationResponse.Succeeded)
            {
                await SetAuthenticatedAsync(authorizationResponse.User);
            }

            return authorizationResponse;
        }

        public async Task<AuthorizationResponse> LogoutAsync()
        {
            var client = await CreateClientAsync();
            var httpResponse = await client.PostAsync("api/Identity/Logout", new StringContent(string.Empty));
            var authorizationResponse = JsonConvert.DeserializeObject<AuthorizationResponse>(await httpResponse.Content.ReadAsStringAsync());

            List<string> cookies = null;
            if (client.DefaultRequestHeaders.TryGetValues("Cookie", out IEnumerable<string> cookieEntries))
            {
                cookies = cookieEntries.ToList();
            }

            if (httpResponse.IsSuccessStatusCode && cookies != null && cookies.Any())
            {
                foreach (var cookie in cookies[0].Split(';'))
                {
                    var cookieParts = cookie.Split('=');
                    if (cookieParts[0] == ".AspNetCore.Identity.Application")
                    {
                        await _jsRuntime.InvokeVoidAsync("removeCookie", cookieParts[0]);
                    }
                }
            }

            if (authorizationResponse.Succeeded)
            {
                await SetAuthenticatedAsync(authorizationResponse.User);
            }

            return authorizationResponse;
        }

        public async Task RefreshSignInAsync()
        {
            var client = await CreateClientAsync();
            var httpResponse = await client.PostAsync("api/Identity/RefreshSignIn", new StringContent(string.Empty));
            await TrySetCookieAsync(httpResponse);
        }

        private async Task TrySetCookieAsync(HttpResponseMessage response)
        {
            if (response.Headers.TryGetValues("Set-Cookie", out var cookieEntries))
            {
                foreach (var cookie in cookieEntries)
                {
                    await _jsRuntime.InvokeVoidAsync("setCookie", cookie.Replace("httponly", string.Empty));
                }
            }
        }

        private async Task SetAuthenticatedAsync(ApplicationUser user)
        {
            if (user == null)
            {
                var principal = new ClaimsPrincipal(new ClaimsIdentity());
                _signInManager.Context.User = principal;
                var provider = (IHostEnvironmentAuthenticationStateProvider)_authenticationStateProvider;
                provider.SetAuthenticationState(Task.FromResult(new AuthenticationState(principal)));
            }
            else
            {
                var principal = await _signInManager.CreateUserPrincipalAsync(user);
                var identity = new ClaimsIdentity(principal.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
                principal = new ClaimsPrincipal(identity);
                _signInManager.Context.User = principal;
                var provider = (IHostEnvironmentAuthenticationStateProvider)_authenticationStateProvider;
                provider.SetAuthenticationState(Task.FromResult(new AuthenticationState(principal)));
            }
        }

        private async Task<HttpClient> CreateClientAsync()
        {
            string cookie = null;
            try
            {
                cookie = await _jsRuntime.InvokeAsync<string>("getCookie");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }

            var client = _httpClientFactory.CreateClient("HES");
            client.BaseAddress = new Uri(_navigationManager.BaseUri);

            if (!string.IsNullOrWhiteSpace(cookie))
            {
                client.DefaultRequestHeaders.Add("Cookie", cookie);
            }

            return client;
        }
    }
}