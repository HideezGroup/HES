using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.LicenseOrders;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class LicenseOrdersPage : HESComponentBase, IDisposable
    {
        public ILicenseService LicenseService { get; set; }
        public IMainTableService<LicenseOrder, LicenseOrderFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<LicenseOrdersPage> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                LicenseService = ScopedServices.GetRequiredService<ILicenseService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<LicenseOrder, LicenseOrderFilter>>();

                SynchronizationService.UpdateLicensesPage += UpdateLicensesPage;     
                
                await BreadcrumbsService.SetLicenseOrders();
                await MainTableService.InitializeAsync(LicenseService.GetLicenseOrdersAsync, LicenseService.GetLicenseOrdersCountAsync, ModalDialogService, StateHasChanged, nameof(LicenseOrder.CreatedAt), ListSortDirection.Descending);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateLicensesPage(string exceptPageId, string userName)
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

        private async Task CreateLicenseOrderAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateLicenseOrder));
                builder.AddAttribute(1, nameof(CreateLicenseOrder.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Create License Order", body, ModalDialogSize.Large);
        }

        private async Task SendLicenseOrderAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SendLicenseOrder));
                builder.AddAttribute(1, nameof(SendLicenseOrder.LicenseOrderId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(SendLicenseOrder.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Send License Order", body);
        }

        private async Task LicenseOrderDetailsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DetailsLicenseOrder));
                builder.AddAttribute(1, nameof(DetailsLicenseOrder.LicenseOrder), MainTableService.SelectedEntity);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("License Order Details", body, ModalDialogSize.ExtraLarge);
        }

        private async Task EditLicenseOrderAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditLicenseOrder));
                builder.AddAttribute(1, nameof(EditLicenseOrder.LicenseOrderId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditLicenseOrder.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit License Order", body, ModalDialogSize.Large);
        }

        private async Task DeleteLicenseOrderAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteLicenseOrder));
                builder.AddAttribute(1, nameof(DeleteLicenseOrder.LicenseOrderId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteLicenseOrder.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete License Order", body);
        }

        public void Dispose()
        {
            SynchronizationService.UpdateLicensesPage -= UpdateLicensesPage;
            MainTableService.Dispose();
        }
    }
}