using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Identity;
using HES.Web.Components;
using HES.Web.Pages.Profile.SecurityKeys;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile
{
    public partial class SecurityTab : HESComponentBase
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        public IFido2Service FidoService { get; set; }
        [Inject] public IIdentityApiClient IdentityApiClient { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<SecurityTab> Logger { get; set; }

        public List<FidoStoredCredential> StoredCredentials { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public ChangePasswordModel ChangePasswordModel { get; set; }
        public Button ButtonChangePassword { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();
                FidoService = ScopedServices.GetRequiredService<IFido2Service>();

                CurrentUser = await ApplicationUserService.GetUserByEmailAsync(await GetCurrentUserEmail());
                ChangePasswordModel = new ChangePasswordModel() { UserId = CurrentUser.Id };
                await LoadStoredCredentialsAsync();

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

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

        private async Task LoadStoredCredentialsAsync()
        {
            StoredCredentials = await FidoService.GetCredentialsByUserEmail(CurrentUser.Email);
        }

        private async Task AddSecurityKeyAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSecurityKey));
                builder.AddAttribute(1, "Refresh", EventCallback.Factory.Create(this, LoadStoredCredentialsAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add new security key", body);
        }

        private async Task RemoveSecurityKeyAsync(string id)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteSecurityKey));
                builder.AddAttribute(1, nameof(DeleteSecurityKey.SecurityKeyId), id);
                builder.AddAttribute(2, "Refresh", EventCallback.Factory.Create(this, LoadStoredCredentialsAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Remove security key", body);
        }

        private async Task EditSecurityKeyAsync(string id)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSecurityKey));
                builder.AddAttribute(1, nameof(EditSecurityKey.SecurityKeyId), id);
                builder.AddAttribute(2, "Refresh", EventCallback.Factory.Create(this, LoadStoredCredentialsAsync));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Change security key name", body);
        }
    }
}