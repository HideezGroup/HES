using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Identity;
using HES.Web.Components;
using HES.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Identity
{
    public enum KeyRegistrationStep
    {
        SecurityKeyRegistration,
        SecurityKeyRegistrationFinish,
        SecurityKeyRegistrationError,
    }

    public partial class RegisterSecurityKey : HESComponentBase
    {
        [Inject] public IIdentityApiClient IdentityApiClient { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<RegisterSecurityKey> Logger { get; set; }

        public IApplicationUserService ApplicationUserService { get; set; }
        public IFido2Service Fido2Service { get; set; }

        public KeyRegistrationStep RegistrationStep { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }

        private string _email;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                _email = NavigationManager.GetQueryValue("email");

                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();
                Fido2Service = ScopedServices.GetRequiredService<IFido2Service>();


                var user = await ApplicationUserService.GetUserByEmailAsync(_email);
                if (user == null)
                {
                    ValidationErrorMessage.DisplayError(nameof(UserEmailModel.Email), HESException.GetMessage(HESCode.UserNotFound));
                    return;
                }

                RegistrationStep = KeyRegistrationStep.SecurityKeyRegistration;

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task SignInWithSecurityKeyAsync()
        {
            try
            {
                await Fido2Service.MakeAssertionRawResponse(_email, JSRuntime);
                RegistrationStep = KeyRegistrationStep.SecurityKeyRegistrationFinish;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                RegistrationStep = KeyRegistrationStep.SecurityKeyRegistrationError;
            }
        }

        private async Task TryAgainAsync()
        {
            await SignInWithSecurityKeyAsync();
        }
    }
}
