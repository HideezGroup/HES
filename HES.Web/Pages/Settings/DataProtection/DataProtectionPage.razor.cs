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
        [Inject] public ILogger<DataProtectionPage> Logger { get; set; }

        public ProtectionStatus Status { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                PageSyncService.UpdateDataProtectionPage += UpdateDataProtectionPage;
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

        private async Task UpdateDataProtectionPage(string exceptPageId)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(() =>
            {
                ProtectionStatus();
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

            var instance = await ModalDialogService.ShowAsync("Enable Data Protection", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                ProtectionStatus();
                await PageSyncService.UpdateDataProtection(PageId);
            }
        }

        private async Task ChangeDataProtectionPasswordAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(ChangeDataProtectionPassword));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Change Data Protection Password", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                ProtectionStatus();
                await PageSyncService.UpdateDataProtection(PageId);
            }
        }

        private async Task DisableDataProtectionAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DisableDataProtection));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Disable Data Protection", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                ProtectionStatus();
                await PageSyncService.UpdateDataProtection(PageId);
            }
        }

        public void Dispose()
        {
            PageSyncService.UpdateDataProtectionPage -= UpdateDataProtectionPage;
        }
    }
}