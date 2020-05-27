﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.HardwareVaults
{
    public partial class EditRfid : ComponentBase
    {
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public ILogger<EditRfid> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Parameter] public string HardwareVaultId { get; set; }

        public HardwareVault HardwareVault { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ModalDialogService.OnCancel += ModalDialogService_OnCancel;
                HardwareVault = await HardwareVaultService.GetVaultByIdAsync(HardwareVaultId);

                if (HardwareVault == null)
                {
                    throw new Exception("Vault not found");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task EditAsync()
        {
            try
            {
                await HardwareVaultService.EditRfidAsync(HardwareVault);
                ToastService.ShowToast("RFID updated.", ToastLevel.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private async Task ModalDialogService_OnCancel()
        {
            await HardwareVaultService.UnchangedVaultAsync(HardwareVault);
            ModalDialogService.OnCancel -= ModalDialogService_OnCancel;
        }
    }
}