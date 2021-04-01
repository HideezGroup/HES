﻿using HES.Core.Entities;
using HES.Core.Enums;
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
    public partial class SendLicenseOrder : HESModalBase, IDisposable
    {
        public ILicenseService LicenseService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<SendLicenseOrder> Logger { get; set; }
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
                    throw new Exception("License Order not found.");

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

        private async Task SendOrderAsync()
        {
            try
            {
                await LicenseService.SendOrderAsync(LicenseOrder);
                await ToastService.ShowToastAsync("License order has been sent.", ToastType.Success);
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
