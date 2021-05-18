using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Identity;
using HES.Web.Components;
using HES.Web.Pages.Profile.PersonalData;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile
{
    public partial class GeneralTab : HESPageBase
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public IIdentityApiClient IdentityApiClient { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
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
                if (User == null)
                {
                    throw new HESException(HESCode.UserNotFound);
                }

                UserProfileModel = new UserProfileModel
                {
                    UserId = User.Id,
                    FirstName = User.FirstName,
                    LastName = User.LastName,              
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

        private async Task DownloadPersonalDataAsync()
        {
            try
            {
                var personalData = new Dictionary<string, string>();
                var personalDataProps = typeof(ApplicationUser).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

                foreach (var p in personalDataProps)
                {
                    personalData.Add(p.Name, p.GetValue(User)?.ToString() ?? "null");
                }

                await JSRuntime.InvokeVoidAsync("downloadPersonalData", JsonConvert.SerializeObject(personalData));
                await ToastService.ShowToastAsync("Download started.", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        private async Task DeletePersonalDataAsync()
        {
            try
            {
                RenderFragment body = (builder) =>
                {
                    builder.OpenComponent(0, typeof(DeletePersonalData));
                    builder.AddAttribute(1, nameof(DeletePersonalData.ApplicationUser), User);
                    builder.CloseComponent();
                };

                await ModalDialogService.ShowAsync("Delete Personal Data", body);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }
    }
}