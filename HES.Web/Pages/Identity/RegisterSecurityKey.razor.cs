using HES.Core.Entities;
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
        UserNotFound
    }

    public partial class RegisterSecurityKey : HESComponentBase
    {
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<RegisterSecurityKey> Logger { get; set; }

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

                var email = NavigationManager.GetQueryValue("email");

                User = await ApplicationUserService.GetUserByEmailAsync(email);

                if (User == null)
                {
                    RegistrationStep = SecurityKeyRegistrationStep.UserNotFound;
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