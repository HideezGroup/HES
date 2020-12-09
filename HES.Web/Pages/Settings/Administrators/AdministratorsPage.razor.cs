using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Users;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Administrators
{
    public partial class AdministratorsPage : HESComponentBase, IDisposable
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        public IEmailSenderService EmailSenderService { get; set; }
        public IMainTableService<ApplicationUser, ApplicationUserFilter> MainTableService { get; set; }
        [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<AdministratorsPage> Logger { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        public AuthenticationState AuthenticationState { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();
                EmailSenderService = ScopedServices.GetRequiredService<IEmailSenderService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<ApplicationUser, ApplicationUserFilter>>();

                SynchronizationService.UpdateAdministratorsPage += UpdateAdministratorsPage;
                SynchronizationService.UpdateAdministratorStatePage += UpdateAdministratorStatePage;
            
                AuthenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                await BreadcrumbsService.SetAdministrators();
                await MainTableService.InitializeAsync(ApplicationUserService.GetAdministratorsAsync, ApplicationUserService.GetAdministratorsCountAsync, ModalDialogService, StateHasChanged, nameof(ApplicationUser.Email), ListSortDirection.Ascending);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateAdministratorsPage(string exceptPageId, string userName)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync($"Page edited by {userName}.", ToastType.Notify);
                StateHasChanged();
            });
        }

        private async Task UpdateAdministratorStatePage()
        {
            await InvokeAsync(async () =>
            {
                await MainTableService.LoadTableDataAsync();       
                StateHasChanged();
            });
        }

        private async Task InviteAdminAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(InviteAdmin));
                builder.AddAttribute(1, nameof(InviteAdmin.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Invite Administrator", body, ModalDialogSize.Default);
        }

        private async Task ResendInviteAsync()
        {
            try
            {
                var callBakcUrl = await ApplicationUserService.GetCallBackUrl(MainTableService.SelectedEntity.Email, NavigationManager.BaseUri);
                await EmailSenderService.SendUserInvitationAsync(MainTableService.SelectedEntity.Email, callBakcUrl);
                await ToastService.ShowToastAsync("Administrator invited.", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        private async Task DeleteAdminAsync()
        {
            if (MainTableService.Entities.Count == 1)
                return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteAdministrator));
                builder.AddAttribute(1, nameof(DeleteAdministrator.ApplicationUserId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteAdministrator.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Administrator", body, ModalDialogSize.Default);
        }

        public void Dispose()
        {
            SynchronizationService.UpdateAdministratorsPage -= UpdateAdministratorsPage;
            SynchronizationService.UpdateAdministratorStatePage -= UpdateAdministratorStatePage;
            MainTableService.Dispose();
        }
    }
}