using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Services;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.DataProtection
{
    public partial class DataProtectionPage : HESComponentBase, IDisposable
    {
        [Inject] public IDataProtectionService DataProtectionService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public ILogger<DataProtectionPage> Logger { get; set; }

        public ProtectionStatus Status { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                SynchronizationService.UpdateDataProtectionPage += UpdateDataProtectionPage;
                ProtectionStatus();
                await BreadcrumbsService.SetDataProtection();
                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateDataProtectionPage(string exceptPageId, string userName)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                ProtectionStatus();
                await ToastService.ShowToastAsync($"Page edited by {userName}.", ToastType.Notify);
                StateHasChanged();
            });
        }

        private void ProtectionStatus()
        {
            Status = DataProtectionService.Status();
            StateHasChanged();
        }

        private async Task EnableDataProtectionAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EnableDataProtection));
                builder.AddAttribute(1, nameof(EnableDataProtection.Refresh), EventCallback.Factory.Create(this, ProtectionStatus));
                builder.AddAttribute(2, nameof(EnableDataProtection.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Enable Data Protection", body, ModalDialogSize.Default);
        }

        private async Task ChangeDataProtectionPasswordAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeDataProtectionPassword));
                builder.AddAttribute(1, nameof(ChangeDataProtectionPassword.Refresh), EventCallback.Factory.Create(this, ProtectionStatus));
                builder.AddAttribute(2, nameof(ChangeDataProtectionPassword.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Change Data Protection Password", body, ModalDialogSize.Default);
        }

        private async Task DisableDataProtectionAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DisableDataProtection));
                builder.AddAttribute(1, nameof(DisableDataProtection.Refresh), EventCallback.Factory.Create(this, ProtectionStatus));
                builder.AddAttribute(2, nameof(DisableDataProtection.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Disable Data Protection", body, ModalDialogSize.Default);
        }

        public void Dispose()
        {
            SynchronizationService.UpdateDataProtectionPage -= UpdateDataProtectionPage;
        }
    }
}
