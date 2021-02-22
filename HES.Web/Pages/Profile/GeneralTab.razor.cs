using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Identity;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile
{
    public partial class GeneralTab : HESComponentBase
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public IIdentityApiClient IdentityApiClient { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<ProfilePage> Logger { get; set; }

        public ApplicationUser User { get; set; }
        public UserProfileModel UserProfileModel { get; set; }
        public ChangeEmailModel ChangeEmailModel { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button ButtonUpdateProfile { get; set; }
        public Button ButtonChangeEmail { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();

                var email = (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User.Identity.Name;
                User = await ApplicationUserService.GetUserByEmailAsync(email);

                UserProfileModel = new UserProfileModel
                {
                    UserId = User.Id,
                    FullName = User.FirstName,
                    PhoneNumber = User.PhoneNumber
                };

                ChangeEmailModel = new ChangeEmailModel
                {
                    CurrentEmail = User.Email
                };

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateProfileInfoAsync()
        {
            try
            {
                await ButtonUpdateProfile.SpinAsync(async () =>
                {
                    await ApplicationUserService.UpdateProfileInfoAsync(UserProfileModel);
                    await IdentityApiClient.RefreshSignInAsync();
                    await ToastService.ShowToastAsync("Your profile has been updated.", ToastType.Success);
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        private async Task ChangeEmailAsync()
        {
            try
            {
                await ButtonChangeEmail.SpinAsync(async () =>
                {
                    await ApplicationUserService.ChangeEmailAsync(ChangeEmailModel);
                    await ToastService.ShowToastAsync("Email confirmation sent.", ToastType.Success);
                });
            }
            catch (HESException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(ChangeEmailModel.NewEmail), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }
    }
}