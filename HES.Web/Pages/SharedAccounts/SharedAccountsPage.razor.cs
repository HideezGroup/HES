﻿using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Filters;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class SharedAccountsPage : HESPageBase, IDisposable
    {
        public ISharedAccountService SharedAccountService { get; set; }
        public IDataTableService<SharedAccount, SharedAccountsFilter> DataTableService { get; set; }
        [Inject] public ILogger<SharedAccountsPage> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                SharedAccountService = ScopedServices.GetRequiredService<ISharedAccountService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<SharedAccount, SharedAccountsFilter>>();

                PageSyncService.UpdateSharedAccountsPage += UpdateSharedAccountsPage;

                await BreadcrumbsService.SetSharedAccounts();
                await DataTableService.InitializeAsync(SharedAccountService.GetSharedAccountsAsync, SharedAccountService.GetSharedAccountsCountAsync, StateHasChanged, nameof(SharedAccount.Name), ListSortDirection.Ascending);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateSharedAccountsPage(string exceptPageId)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
                StateHasChanged();
            });

        }

        private async Task CreateSharedAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateSharedAccount));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.SharedAccounts_CreateSharedAccount_Title, body, ModalDialogSize.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateSharedAccounts(PageId);
            }
        }

        private async Task DeleteSharedAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteSharedAccount));
                builder.AddAttribute(1, nameof(DeleteSharedAccount.AccountId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.SharedAccounts_DeleteSharedAccount_Title, body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateSharedAccounts(PageId);
            }
        }

        private async Task EditSharedAccountOTPAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSharedAccountOtp));
                builder.AddAttribute(1, nameof(EditSharedAccountOtp.AccountId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.SharedAccounts_EditSharedAccountOtp_Title, body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateSharedAccounts(PageId);
            }
        }

        private async Task EditSharedAccountAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSharedAccount));
                builder.AddAttribute(1, nameof(EditSharedAccount.AccountId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.SharedAccounts_EditSharedAccount_Title, body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateSharedAccounts(PageId);
            }
        }

        private async Task EditSharedAccountPasswordAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditSharedAccountPassword));
                builder.AddAttribute(1, nameof(EditSharedAccountPassword.AccountId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync(Resources.Resource.SharedAccounts_EditSharedAccountPassword_Title, body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateSharedAccounts(PageId);
            }
        }

        public void Dispose()
        {
            PageSyncService.UpdateSharedAccountsPage -= UpdateSharedAccountsPage;
        }
    }
}