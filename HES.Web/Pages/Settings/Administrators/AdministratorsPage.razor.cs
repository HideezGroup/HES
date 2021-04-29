using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Filters;
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
    public partial class AdministratorsPage : HESPageBase, IDisposable
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        public IEmailSenderService EmailSenderService { get; set; }
        public IDataTableService<ApplicationUser, ApplicationUserFilter> DataTableService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<AdministratorsPage> Logger { get; set; }
        public AuthenticationState AuthenticationState { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();
                EmailSenderService = ScopedServices.GetRequiredService<IEmailSenderService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<ApplicationUser, ApplicationUserFilter>>();

                PageSyncService.UpdateAdministratorsPage += UpdateAdministratorsPage;
                PageSyncService.UpdateAdministratorStatePage += UpdateAdministratorStatePage;

                AuthenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
                await BreadcrumbsService.SetAdministrators();
                await DataTableService.InitializeAsync(ApplicationUserService.GetAdministratorsAsync, ApplicationUserService.GetAdministratorsCountAsync, StateHasChanged, nameof(ApplicationUser.Email), ListSortDirection.Ascending);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateAdministratorsPage(string exceptPageId)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
                StateHasChanged();
            });
        }

        private async Task UpdateAdministratorStatePage()
        {
            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
                StateHasChanged();
            });
        }

        private async Task InviteAdminAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(InviteAdmin));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Invite Administrator", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateAdministrators(PageId);
            }
        }

        private async Task ResendInviteAsync()
        {
            try
            {
                var callBakcUrl = await ApplicationUserService.GenerateInviteCallBackUrl(DataTableService.SelectedEntity.Email, NavigationManager.BaseUri);
                await EmailSenderService.SendUserInvitationAsync(DataTableService.SelectedEntity.Email, callBakcUrl);
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
            if (DataTableService.Entities.Count == 1)
                return;

            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteAdministrator));
                builder.AddAttribute(1, nameof(DeleteAdministrator.ApplicationUserId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Delete Administrator", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateAdministrators(PageId);
            }
        }

        public void Dispose()
        {
            PageSyncService.UpdateAdministratorsPage -= UpdateAdministratorsPage;
            PageSyncService.UpdateAdministratorStatePage -= UpdateAdministratorStatePage;
        }
    }
}