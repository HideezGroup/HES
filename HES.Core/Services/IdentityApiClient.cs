using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Core.Models.Web.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using Newtonsoft.Json;
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
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IJSRuntime _jsRuntime;

        public IdentityApiClient(HttpClient httpClient,
                                 AuthenticationStateProvider authenticationStateProvider,
                                 SignInManager<ApplicationUser> signInManager,
                                 IJSRuntime jsRuntime)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _signInManager = signInManager;
            _jsRuntime = jsRuntime;
        }

        public async Task<AuthorizationResponse> LoginWithPasswordAsync(PasswordSignInModel parameters)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
            var httpResponse = await _httpClient.PostAsync("api/Identity/LoginWithPassword", stringContent);
            var authorizationResponse = JsonConvert.DeserializeObject<AuthorizationResponse>(await httpResponse.Content.ReadAsStringAsync());

            await TrySetCookieAsync(httpResponse);

            if (authorizationResponse.Succeeded)
                await SetAuthenticatedAsync(authorizationResponse.User);

            return authorizationResponse;
        }

        public async Task<AuthorizationResponse> LoginWithFido2Async(SecurityKeySignInModel parameters)
        {
            var stringContent = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
            var httpResponse = await _httpClient.PostAsync("api/Identity/LoginWithFido2", stringContent);
            var authorizationResponse = JsonConvert.DeserializeObject<AuthorizationResponse>(await httpResponse.Content.ReadAsStringAsync());

            await TrySetCookieAsync(httpResponse);

            if (authorizationResponse.Succeeded)
                await SetAuthenticatedAsync(authorizationResponse.User);

            return authorizationResponse;
        }

        public async Task<AuthorizationResponse> LogoutAsync()
        {
            List<string> cookies = null;
            if (_httpClient.DefaultRequestHeaders.TryGetValues("Cookie", out IEnumerable<string> cookieEntries))
                cookies = cookieEntries.ToList();

            var httpResponse = await _httpClient.PostAsync("api/Identity/Logout", new StringContent(string.Empty));
            var authorizationResponse = JsonConvert.DeserializeObject<AuthorizationResponse>(await httpResponse.Content.ReadAsStringAsync());

            if (httpResponse.IsSuccessStatusCode && cookies != null && cookies.Any())
            {
                _httpClient.DefaultRequestHeaders.Remove("Cookie");

                foreach (var cookie in cookies[0].Split(';'))
                {
                    var cookieParts = cookie.Split('=');
                    if (cookieParts[0] == ".AspNetCore.Identity.Application")
                        await _jsRuntime.InvokeVoidAsync("removeCookie", cookieParts[0]);
                }
            }

            if (authorizationResponse.Succeeded)
                await SetAuthenticatedAsync(authorizationResponse.User);

            return authorizationResponse;
        }

        public async Task RefreshSignInAsync()
        {
            var httpResponse = await _httpClient.PostAsync("api/Identity/RefreshSignIn", new StringContent(string.Empty));
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
    }
}