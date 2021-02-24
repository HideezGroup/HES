using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppUsers;
using HES.Web.Areas.Identity.Pages.Account.Manage.SecurityKeys;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage
{
    public partial class ProfilePage : OwningComponentBase
    {
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<ProfilePage> Logger { get; set; }
        public IFido2Service FidoService { get; set; }

        public List<FidoStoredCredential> StoredCredentials { get; set; }


        public ApplicationUser ApplicationUser { get; set; }
        public ProfileInfo ProfileInfo { get; set; }
        public ProfilePassword ProfilePassword { get; set; }
        public bool Initialized { get; set; }
        public bool LoadFailed { get; set; }
        public string ErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await BreadcrumbsService.SetProfile();

                var response = await HttpClient.GetAsync("api/Identity/GetUser");

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                FidoService = ScopedServices.GetRequiredService<IFido2Service>();
                ApplicationUser = JsonConvert.DeserializeObject<ApplicationUser>(await response.Content.ReadAsStringAsync());

                ProfileInfo = new ProfileInfo
                {
                    Email = ApplicationUser.Email,
                    PhoneNumber = ApplicationUser.PhoneNumber
                };

                ProfilePassword = new ProfilePassword();

                await LoadStoredCredentialsAsync();

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ErrorMessage = ex.Message;
                LoadFailed = true;
            }
        }

        private async Task ModalDialogService_OnClose()
        {
            await LoadStoredCredentialsAsync();
            ModalDialogService.OnClose -= ModalDialogService_OnClose;
        }

        private async Task LoadStoredCredentialsAsync()
        {
            StoredCredentials = await FidoService.GetCredentialsByUserEmail(ProfileInfo.Email);
            StateHasChanged();
        }

        private async Task AddSecurityKeyAsync()
        {
            ModalDialogService.OnClose += ModalDialogService_OnClose;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddSecurityKey));
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add Security Key", body);
        }

        private async Task EditSecurityKeyAsync(string credentialId)
        {
            ModalDialogService.OnClose += ModalDialogService_OnClose;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSecurityKey));
                builder.AddAttribute(1, nameof(EditSecurityKey.SecurityKeyId), credentialId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Change security key name", body);
        }

        private async Task RemoveSecurityKeyAsync(string credentialId)
        {
            ModalDialogService.OnClose += ModalDialogService_OnClose;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteSecurityKey));
                builder.AddAttribute(1, nameof(DeleteSecurityKey.SecurityKeyId), credentialId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Remove Security Key", body);
        }

        private async Task SaveProfileAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/UpdateProfileInfo", new StringContent(JsonConvert.SerializeObject(ProfileInfo), Encoding.UTF8, "application/json"));
                
                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());
   
                await ToastService.ShowToastAsync("Your profile has been updated.", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        private async Task SendVerificationEmailAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/SendVerificationEmail", new StringContent(string.Empty));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                await ToastService.ShowToastAsync("Verification email sent.", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        public async Task ChangePasswordAsync()
        {
            try
            {
                var response = await HttpClient.PostAsync("api/Identity/UpdateProfilePassword", new StringContent(JsonConvert.SerializeObject(ProfilePassword), Encoding.UTF8, "application/json"));

                if (!response.IsSuccessStatusCode)
                    throw new Exception(await response.Content.ReadAsStringAsync());

                await ToastService.ShowToastAsync("Your password has been changed.", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }
    }
}