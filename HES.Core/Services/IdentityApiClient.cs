using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Core.Models.Web.Identity;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.JSInterop;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Linq;
using System.Net;
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
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            var identity = new ClaimsIdentity(principal.Claims, CookieAuthenticationDefaults.AuthenticationScheme);
            principal = new ClaimsPrincipal(identity);
            _signInManager.Context.User = principal;
            var provider = (IHostEnvironmentAuthenticationStateProvider)_authenticationStateProvider;
            provider.SetAuthenticationState(Task.FromResult(new AuthenticationState(principal)));
        }
    }
}