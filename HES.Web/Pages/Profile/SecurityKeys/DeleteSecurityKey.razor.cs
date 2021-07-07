using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.SecurityKeys
{
    public partial class DeleteSecurityKey : HESModalBase
    {
        public IFido2Service FidoService { get; set; }
        [Inject] public ILogger<DeleteSecurityKey> Logger { get; set; }
        [Parameter] public string SecurityKeyId { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                FidoService = ScopedServices.GetRequiredService<IFido2Service>();
                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ModalDialogCancel();
            }
        }

        private async Task DeleteSecurityKeyAsync()
        {
            try
            {
                await FidoService.RemoveSecurityKeyAsync(SecurityKeyId);
                await ToastService.ShowToastAsync(Resources.Resource.Profile_Security_DeleteSecurityKey_Toast, ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }
    }
}
