using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class DeleteLicenseOrder : HESModalBase, IDisposable
    {
        public ILicenseService LicenseService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteLicenseOrder> Logger { get; set; }
        [Parameter] public string LicenseOrderId { get; set; }

        public LicenseOrder LicenseOrder { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                LicenseService = ScopedServices.GetRequiredService<ILicenseService>();

                LicenseOrder = await LicenseService.GetLicenseOrderByIdAsync(LicenseOrderId);
                if (LicenseOrder == null)
                    throw new HESException(HESCode.LicenseOrderNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(LicenseOrder.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(LicenseOrder.Id, LicenseOrder);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task DeleteOrderAsync()
        {
            try
            {
                await LicenseService.DeleteOrderAsync(LicenseOrderId);
                await ToastService.ShowToastAsync("License order deleted.", ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(LicenseOrder.Id);
        }
    }
}