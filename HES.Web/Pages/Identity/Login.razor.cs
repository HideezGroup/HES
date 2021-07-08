using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Core.Models.Identity;
using HES.Web.Components;
using HES.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Identity
{
    public enum AuthenticationStep
    {
        EmailValidation,
        EnterPassword,
        ForgotPassword,
        SecurityKeyAuthentication,
        SecurityKeyError
    }

    public partial class Login : HESPageBase
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        public IFido2Service Fido2Service { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<Login> Logger { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public AuthenticationStep AuthenticationStep { get; set; }
        public PasswordSignInModel PasswordSignInModel { get; set; } = new PasswordSignInModel();
        public SecurityKeySignInModel SecurityKeySignInModel { get; set; } = new SecurityKeySignInModel();
        public UserEmailModel UserEmailModel { get; set; } = new UserEmailModel();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button ButtonSpinner { get; set; }
        public bool HasSecurityKey { get; set; }
        public string ReturnUrl { get; set; }
        public bool SetFocus { get; set; }
        public bool SendingDisabled { get; set; }
        public int TimeToRepeat { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();
                Fido2Service = ScopedServices.GetRequiredService<IFido2Service>();

                if (await GetCurrentUserIsAuthenticatedAsync())
                {
                    NavigationManager.NavigateTo(Routes.Logout, true);
                    return;
                }

                ReturnUrl = NavigationManager.GetQueryValue("returnUrl");
                SwitchFocus();

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
            try
            {
                if (SetFocus)
                {
                    switch (AuthenticationStep)
                    {
                        case AuthenticationStep.EmailValidation:
                            await JSRuntime.InvokeVoidAsync("setFocus", "email");
                            SwitchFocus();
                            break;
                        case AuthenticationStep.EnterPassword:
                            await JSRuntime.InvokeVoidAsync("setFocus", "password");
                            SwitchFocus();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"JS. {ex.Message}");
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

                    var isAdmin = await UserManager.IsInRoleAsync(user, ApplicationRoles.Admin);
                    if (!isAdmin)
                    {
                        ValidationErrorMessage.DisplayError(nameof(UserEmailModel.Email), HESException.GetMessage(HESCode.InvalidLoginAttempt));
                        return;
                    }

                    PasswordSignInModel.Email = UserEmailModel.Email;
                    HasSecurityKey = (await Fido2Service.GetCredentialsByUserEmail(UserEmailModel.Email)).Count > 0;
                    SwitchFocus();
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
                var response = await JSRuntime.InvokeWebApiPostAsync<AuthorizationResponse>(Routes.ApiLoginWithFido2, SecurityKeySignInModel);
                response.ThrowIfFailed();

                NavigationManager.NavigateToLocal(ReturnUrl ?? Routes.Dashboard, true);
            }
            catch (JSException ex)
            {
                Logger.LogWarning($"JS. {UserEmailModel.Email}. {ex.Message}");
                AuthenticationStep = AuthenticationStep.SecurityKeyError;
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
                    var response = await JSRuntime.InvokeWebApiPostAsync<AuthorizationResponse>(Routes.ApiLoginWithPassword, PasswordSignInModel);
                    response.ThrowIfFailed();

                    if (response.Succeeded)
                    {
                        NavigationManager.NavigateToLocal(ReturnUrl ?? Routes.Dashboard, true);
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

        private void SetForgotPasswordStep()
        {
            AuthenticationStep = AuthenticationStep.ForgotPassword;
        }

        private async Task ResetPasswordAsync()
        {
            try
            {
                await ApplicationUserService.SendUserResetPasswordAsync(UserEmailModel.Email, NavigationManager.BaseUri);
                SetTimerToResend();
            }
            catch (HESException ex)
            {
                SetErrorMessage(ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetErrorMessage(ex.Message);
            }
        }

        private async void SetTimerToResend()
        {
            SendingDisabled = true;
            TimeToRepeat = 60;
            while (TimeToRepeat > 0)
            {
                TimeToRepeat--;
                StateHasChanged();
                await Task.Delay(1000);
            }
            SendingDisabled = false;
            StateHasChanged();
        }

        private void SwitchFocus()
        {
            SetFocus = !SetFocus;
        }
    }
}