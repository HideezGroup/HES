using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppUsers;
using HES.Core.Models.Web.Identity;
using HES.Web.Components;
using HES.Web.Pages.Profile.SecurityKeys;
using HES.Web.Pages.Profile.TwoFactor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile
{
    public partial class SecurityTab : HESPageBase
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        public IFido2Service FidoService { get; set; }
        [Inject] public IIdentityApiClient IdentityApiClient { get; set; }
        [Inject] public ILogger<SecurityTab> Logger { get; set; }
        [Inject] public HttpClient HttpClient { get; set; }

        public List<FidoStoredCredential> StoredCredentials { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public ChangePasswordModel ChangePasswordModel { get; set; }
        public Button ButtonChangePassword { get; set; }
        public TwoFactorInfo TwoFactorInfo { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();
                FidoService = ScopedServices.GetRequiredService<IFido2Service>();

                CurrentUser = await ApplicationUserService.GetUserByEmailAsync(await GetCurrentUserEmailAsync());

                // Password
                ChangePasswordModel = new ChangePasswordModel() { UserId = CurrentUser.Id };

                // Security Key
                await LoadStoredCredentialsAsync();

                // 2FA
                await GetTwoFactorInfoAsync();

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        #region Password

        private async Task ChangePasswordAsync()
        {
            try
            {
                await ButtonChangePassword.SpinAsync(async () =>
                {
                    await ApplicationUserService.UpdateAccountPasswordAsync(ChangePasswordModel);
                    await IdentityApiClient.RefreshSignInAsync();
                    await ToastService.ShowToastAsync("Password updated.", ToastType.Success);

                    ChangePasswordModel = new ChangePasswordModel() { UserId = CurrentUser.Id };
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        #endregion

        #region SecurityKey

        private async Task LoadStoredCredentialsAsync()
        {
            StoredCredentials = await FidoService.GetCredentialsByUserEmail(CurrentUser.Email);
        }

        private async Task AddSecurityKeyAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSecurityKey));
                builder.AddAttribute(1, nameof(AddSecurityKey.CurrentUser), CurrentUser);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Add new security key", body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadStoredCredentialsAsync();
            }
        }

        private async Task DeleteSecurityKeyAsync(string id)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteSecurityKey));
                builder.AddAttribute(1, nameof(DeleteSecurityKey.SecurityKeyId), id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Remove security key", body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadStoredCredentialsAsync();
            }
        }

        private async Task EditSecurityKeyAsync(string id)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSecurityKey));
                builder.AddAttribute(1, nameof(EditSecurityKey.SecurityKeyId), id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Change security key name", body);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadStoredCredentialsAsync();
            }
        }

        #endregion

        #region 2FA

        private async Task GetTwoFactorInfoAsync()
        {
            TwoFactorInfo = await ApplicationUserService.GetTwoFactorInfoAsync(HttpClient);
        }

        private async Task EnableAuthenticatorAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EnableAuthenticator));
                builder.CloseComponent();
            };
                   
            var instance = await ModalDialogService2.ShowAsync("Enable Authenticator", body, ModalDialogSize2.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await GetTwoFactorInfoAsync();
            }
        }

        private async Task ResetAuthenticatorAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ResetAuthenticator));
                builder.CloseComponent();
            };
     
            var instance = await ModalDialogService2.ShowAsync("Reset Authenticator", body, ModalDialogSize2.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await GetTwoFactorInfoAsync();
            }
        }

        private async Task Disable2FaAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(Disable2fa));
                builder.CloseComponent();
            };
        
            var instance = await ModalDialogService2.ShowAsync("Disable two-factor authentication (2FA)", body, ModalDialogSize2.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await GetTwoFactorInfoAsync();
            }
        }

        private async Task GenerateRecoveryCodesAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(GenerateRecoveryCodes));
                builder.CloseComponent();
            };

            await ModalDialogService2.ShowAsync("Generate two-factor authentication (2FA) recovery codes", body, ModalDialogSize2.Large);        
        }

        private async Task ForgetBrowserAsync()
        {
            try
            {
                await ApplicationUserService.ForgetBrowserAsync(HttpClient);
                await ToastService.ShowToastAsync("The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code.", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        #endregion
    }
}