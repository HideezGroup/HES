using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Core.Models.ApplicationUsers;
using HES.Core.Models.Identity;
using HES.Web.Components;
using HES.Web.Extensions;
using HES.Web.Pages.Profile.SecurityKeys;
using HES.Web.Pages.Profile.TwoFactor;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile
{
    public partial class SecurityTab : HESPageBase
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        public IFido2Service FidoService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<SecurityTab> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public List<FidoStoredCredential> StoredCredentials { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public UserChangePasswordModel ChangePasswordModel { get; set; }
        public Button ButtonChangePassword { get; set; }
        public TwoFactorInfo TwoFactorInfo { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();
                FidoService = ScopedServices.GetRequiredService<IFido2Service>();

                CurrentUser = await ApplicationUserService.GetUserByEmailAsync(await GetCurrentUserEmailAsync());
                if (CurrentUser == null)
                {
                    NavigationManager.NavigateTo(Routes.Login, true);
                    return;
                }

                // Password
                ChangePasswordModel = new UserChangePasswordModel() { UserId = CurrentUser.Id };

                // Security Key
                await LoadStoredCredentialsAsync();

                // Continue in OnAfterRenderAsync
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
                if (firstRender)
                {
                    // 2FA
                    await GetTwoFactorInfoAsync();
                    SetInitialized();
                    StateHasChanged();
                }
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
                    var response = await JSRuntime.InvokeWebApiPostAsync<IdentityResponse>(Routes.ApiUpdateAccountPassword, ChangePasswordModel);
                    response.ThrowIfFailed();
                    await ToastService.ShowToastAsync(Resources.Resource.Profile_Security_ChangePassword_Toast, ToastType.Success);
                    ChangePasswordModel = new UserChangePasswordModel() { UserId = CurrentUser.Id };
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Profile_Security_AddSecurityKey_Title, body);
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Profile_Security_DeleteSecurityKey_Title, body);
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Profile_Security_EditSecurityKey_Title, body);
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
            TwoFactorInfo = await JSRuntime.InvokeWebApiGetAsync<TwoFactorInfo>(Routes.ApiGetTwoFactorInfo);
        }

        private async Task EnableAuthenticatorAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EnableAuthenticator));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Profile_Security_EnableAuthenticator_Title, body, ModalDialogSize.Large);
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Profile_Security_ResetAuthenticator_Title, body, ModalDialogSize.Large);
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

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.Profile_Security_Disable2fa_Title, body, ModalDialogSize.Large);
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

            await ModalDialogService.ShowAsync(Resources.Resource.Profile_Security_GenerateRecoveryCodes_Title, body, ModalDialogSize.Large);
        }

        private async Task ForgetBrowserAsync()
        {
            try
            {
                await JSRuntime.InvokeWebApiPostVoidAsync(Routes.ForgetTwoFactorClient);
                await ToastService.ShowToastAsync(Resources.Resource.Profile_Security_ForgetBrowser_Toast, ToastType.Success);
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