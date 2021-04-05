using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.LicenseOrders;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class LicenseOrdersPage : HESPageBase, IDisposable
    {
        public ILicenseService LicenseService { get; set; }
        public IDataTableService<LicenseOrder, LicenseOrderFilter> DataTableService { get; set; }
        [Inject] public ILogger<LicenseOrdersPage> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                LicenseService = ScopedServices.GetRequiredService<ILicenseService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<LicenseOrder, LicenseOrderFilter>>();

                SynchronizationService.UpdateLicensesPage += UpdateLicensesPage;     
                
                await BreadcrumbsService.SetLicenseOrders();
                await DataTableService.InitializeAsync(LicenseService.GetLicenseOrdersAsync, LicenseService.GetLicenseOrdersCountAsync, StateHasChanged, nameof(LicenseOrder.CreatedAt), ListSortDirection.Descending);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateLicensesPage(string exceptPageId)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
                StateHasChanged();
            });
        }

        private async Task UpdateLicenseInfoAsync()
        {
            try
            {
                await LicenseService.UpdateLicenseOrdersAsync();
                await LicenseService.UpdateHardwareVaultsLicenseStatusAsync();
                await DataTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync("License info updated.", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }

        private async Task CreateLicenseOrderAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateLicenseOrder));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Create License Order", body, ModalDialogSize.Large);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateLicenses(PageId);
            }
        }

        private async Task SendLicenseOrderAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(SendLicenseOrder));
                builder.AddAttribute(1, nameof(SendLicenseOrder.LicenseOrderId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Send License Order", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateLicenses(PageId);
            }
        }

        private async Task LicenseOrderDetailsAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DetailsLicenseOrder));
                builder.AddAttribute(1, nameof(DetailsLicenseOrder.LicenseOrder), DataTableService.SelectedEntity);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("License Order Details", body, ModalDialogSize.ExtraLarge);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateLicenses(PageId);
            }
        }

        private async Task EditLicenseOrderAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditLicenseOrder));
                builder.AddAttribute(1, nameof(EditLicenseOrder.LicenseOrderId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Edit License Order", body, ModalDialogSize.ExtraLarge);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateLicenses(PageId);
            }
        }

        private async Task DeleteLicenseOrderAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteLicenseOrder));
                builder.AddAttribute(1, nameof(DeleteLicenseOrder.LicenseOrderId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Delete License Order", body, ModalDialogSize.ExtraLarge);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateLicenses(PageId);
            }
        }

        public void Dispose()
        {
            SynchronizationService.UpdateLicensesPage -= UpdateLicensesPage;
        }
    }
}