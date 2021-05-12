﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Web.Components;
using HES.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Identity
{
    public enum SecurityKeyRegistrationStep
    {
        Start,
        Configuration,
        Done,
        Error,
        UserNotFound,
        AlreadyAdded,
        InvalidToken
    }

    public partial class RegisterSecurityKey : HESPageBase
    {
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<RegisterSecurityKey> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public IApplicationUserService ApplicationUserService { get; set; }
        public IFido2Service Fido2Service { get; set; }

        public SecurityKeyRegistrationStep RegistrationStep { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ApplicationUser User { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();
                Fido2Service = ScopedServices.GetRequiredService<IFido2Service>();

                var code = NavigationManager.GetQueryValue("code");
                var email = NavigationManager.GetQueryValue("email");

                // Check user exist
                User = await ApplicationUserService.GetUserByEmailAsync(email);
                if (User == null)
                {
                    RegistrationStep = SecurityKeyRegistrationStep.UserNotFound;
                }

                // Check key is already added
                var cred = await Fido2Service.GetCredentialsByUserEmail(User.Email);
                if (cred.Count > 0)
                {
                    RegistrationStep = SecurityKeyRegistrationStep.AlreadyAdded;
                }

                // Verify token
                var tokenIsValid = await ApplicationUserService.VerifyRegisterSecurityKeyTokenAsync(User, code);
                if (!tokenIsValid)
                {
                    RegistrationStep = SecurityKeyRegistrationStep.InvalidToken;
                }

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task RegisterSecurityKeyAsync()
        {
            try
            {
                RegistrationStep = SecurityKeyRegistrationStep.Configuration;
                await Fido2Service.AddSecurityKeyAsync(User.Email, JSRuntime);
                RegistrationStep = SecurityKeyRegistrationStep.Done;
            }
            catch (JSException ex)
            {
                Logger.LogWarning($"JS. {User.Email}. {ex.Message}");
                RegistrationStep = SecurityKeyRegistrationStep.Error;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                RegistrationStep = SecurityKeyRegistrationStep.Error;
            }
        }

        private async Task TryAgainAsync()
        {
            await RegisterSecurityKeyAsync();
        }
    }
}