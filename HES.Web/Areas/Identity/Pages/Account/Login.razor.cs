using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Identity;
using HES.Infrastructure;
using HES.Web.Components;
using HES.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account
{
    public enum AuthenticationStep
    {
        EmailValidation,
        EnterPassword,
        SecurityKeyAuthentication,
        SecurityKeyError
    }

    public partial class Login : HESComponentBase
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        public IFido2Service Fido2Service { get; set; }
        [Inject] public IIdentityApiClient IdentityApiClient { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<Login> Logger { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }

        public AuthenticationStep AuthenticationStep { get; set; }
        public PasswordSignInModel PasswordSignInModel { get; set; } = new PasswordSignInModel();
        public SecurityKeySignInModel SecurityKeySignInModel { get; set; } = new SecurityKeySignInModel();
        public UserEmailModel UserEmailModel { get; set; } = new UserEmailModel();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button ButtonSpinner { get; set; }
        public bool HasSecurityKey { get; set; }
        public string ReturnUrl { get; set; }

        protected override void OnInitialized()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();
                Fido2Service = ScopedServices.GetRequiredService<IFido2Service>();

                ReturnUrl = NavigationManager.GetQueryValue("returnUrl");

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            switch (AuthenticationStep)
            {
                case AuthenticationStep.EmailValidation:
                    await JSRuntime.InvokeVoidAsync("setFocus", "email");
                    break;
                case AuthenticationStep.EnterPassword:
                    await JSRuntime.InvokeVoidAsync("setFocus", "password");
                    break;
            }
        }

        private async Task NextAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    var user = await ApplicationUserService.GetUserByEmailAsync(UserEmailModel.Email);
                    if (user == null)
                    {
                        ValidationErrorMessage.DisplayError(nameof(UserEmailModel.Email), HESException.GetMessage(HESCode.UserNotFound));
                        return;
                    }

                    var isAdmin = await UserManager.IsInRoleAsync(user, ApplicationRoles.AdminRole);
                    if (!isAdmin)
                    {
                        ValidationErrorMessage.DisplayError(nameof(UserEmailModel.Email), HESException.GetMessage(HESCode.InvalidLoginAttempt));
                        return;
                    }

                    PasswordSignInModel.Email = UserEmailModel.Email;
                    HasSecurityKey = (await Fido2Service.GetCredentialsByUserEmail(UserEmailModel.Email)).Count > 0;
                    AuthenticationStep = AuthenticationStep.EnterPassword;
                });
            }
            catch (HESException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(UserEmailModel.Email), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetErrorMessage(ex.Message);
            }
        }

        private async Task SignInWithSecurityKeyAsync()
        {
            try
            {
                AuthenticationStep = AuthenticationStep.SecurityKeyAuthentication;

                SecurityKeySignInModel.RememberMe = PasswordSignInModel.RememberMe;
                SecurityKeySignInModel.AuthenticatorAssertionRawResponse = await Fido2Service.MakeAssertionRawResponse(UserEmailModel.Email, JSRuntime);
                var response = await IdentityApiClient.LoginWithFido2Async(SecurityKeySignInModel);
                response.ThrowIfFailed();

                NavigationManager.NavigateTo(ReturnUrl ?? Routes.Dashboard, true);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                AuthenticationStep = AuthenticationStep.SecurityKeyError;
            }
        }

        private async Task LoginWithPasswordAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    var response = await IdentityApiClient.LoginWithPasswordAsync(PasswordSignInModel);
                    response.ThrowIfFailed();

                    if (response.Succeeded)
                    {
                        NavigationManager.NavigateTo(Routes.Dashboard, true);
                        return;
                    }

                    if (response.RequiresTwoFactor)
                    {
                        NavigationManager.NavigateTo($"{Routes.LoginWith2Fa}?returnUrl={ReturnUrl}", true);
                        return;
                    }

                    if (response.IsLockedOut)
                    {
                        NavigationManager.NavigateTo(Routes.Lockout, true);
                        return;
                    }
                });
            }
            catch (HESException ex) when (ex.Code == HESCode.InvalidLoginAttempt)
            {
                ValidationErrorMessage.DisplayError(nameof(PasswordSignInModel.Password), HESException.GetMessage(ex.Code));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetErrorMessage(ex.Message);
            }
        }

        private async Task TryAgainAsync()
        {
            await SignInWithSecurityKeyAsync();
        }

        private void BackToEmailValidation()
        {
            AuthenticationStep = AuthenticationStep.EmailValidation;
        }
    }
}