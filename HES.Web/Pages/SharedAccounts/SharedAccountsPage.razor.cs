using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.SharedAccounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class SharedAccountsPage : HESComponentBase, IDisposable
    {
        public ISharedAccountService SharedAccountService { get; set; }
        public IMainTableService<SharedAccount, SharedAccountsFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<SharedAccountsPage> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                SharedAccountService = ScopedServices.GetRequiredService<ISharedAccountService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<SharedAccount, SharedAccountsFilter>>();

                SynchronizationService.UpdateSharedAccountsPage += UpdateSharedAccountsPage;

                await BreadcrumbsService.SetSharedAccounts();
                await MainTableService.InitializeAsync(SharedAccountService.GetSharedAccountsAsync, SharedAccountService.GetSharedAccountsCountAsync, ModalDialogService, StateHasChanged, nameof(SharedAccount.Name), ListSortDirection.Ascending);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateSharedAccountsPage(string exceptPageId, string userName)
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

        private async Task CreateSharedAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateSharedAccount));
                builder.AddAttribute(1, nameof(CreateSharedAccount.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Create Shared Account", body, ModalDialogSize.Large);
        }

        private async Task DeleteSharedAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteSharedAccount));
                builder.AddAttribute(1, nameof(DeleteSharedAccount.AccountId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteSharedAccount.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Shared Account", body, ModalDialogSize.Default);
        }

        private async Task EditSharedAccountOTPAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSharedAccountOtp));
                builder.AddAttribute(1, nameof(EditSharedAccountOtp.AccountId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditSharedAccountOtp.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Shared Account OTP", body, ModalDialogSize.Default);
        }

        private async Task EditSharedAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSharedAccount));
                builder.AddAttribute(1, nameof(EditSharedAccount.AccountId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditSharedAccount.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Shared Account", body, ModalDialogSize.Default);
        }

        private async Task EditSharedAccountPasswordAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSharedAccountPassword));
                builder.AddAttribute(1, nameof(EditSharedAccountPassword.AccountId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditSharedAccountPassword.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Shared Account Password", body, ModalDialogSize.Default);
        }

        public void Dispose()
        {
            SynchronizationService.UpdateSharedAccountsPage -= UpdateSharedAccountsPage;
            MainTableService.Dispose();
        }
    }
}