using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.HardwareVaults
{
    public partial class ShowActivationCode : HESModalBase
    {
        public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IEmailSenderService EmailSenderService { get; set; }
        [Inject] public ILogger<ShowActivationCode> Logger { get; set; }
        [Inject] public IJSRuntime JsRuntime { get; set; }
        [Parameter] public string HardwareVaultId { get; set; }

        public HardwareVault HardwareVault { get; set; }
        public string Code { get; set; }
        public string InputType { get; private set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                HardwareVaultService = ScopedServices.GetRequiredService<IHardwareVaultService>();

                InputType = "Password";

                HardwareVault = await HardwareVaultService.GetVaultByIdAsync(HardwareVaultId);
                if (HardwareVault == null)
                    throw new Exception("HardwareVault not found.");

                Code = await HardwareVaultService.GetVaultActivationCodeAsync(HardwareVault.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task SendEmailAsync()
        {
            try
            {
                await EmailSenderService.SendHardwareVaultActivationCodeAsync(HardwareVault.Employee, Code);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private async Task CopyToClipboardAsync()
        {
            try
            {
                await JsRuntime.InvokeVoidAsync("copyToClipboard");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
            }
        }

        private async Task CloseAsync()
        {
            Code = string.Empty;
            await ModalDialogClose();
        }
    }
}