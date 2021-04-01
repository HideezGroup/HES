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
    public partial class DataProtectionPage : HESPageBase, IDisposable
    {
        [Inject] public IDataProtectionService DataProtectionService { get; set; }
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
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Enable Data Protection", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                ProtectionStatus();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        private async Task ChangeDataProtectionPasswordAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeDataProtectionPassword));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Change Data Protection Password", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                ProtectionStatus();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        private async Task DisableDataProtectionAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DisableDataProtection));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Disable Data Protection", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                ProtectionStatus();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        public void Dispose()
        {
            SynchronizationService.UpdateDataProtectionPage -= UpdateDataProtectionPage;
        }
    }
}
