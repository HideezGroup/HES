using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage.SecurityKeys
{
    public partial class DeleteSecurityKey : OwningComponentBase
    {
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public ILogger<DeleteSecurityKey> Logger { get; set; }
        [Parameter] public string SecurityKeyId { get; set; }

        public IFido2Service FidoService { get; set; }
        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                FidoService = ScopedServices.GetRequiredService<IFido2Service>();
                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task DeleteSecurityKeyAsync()
        {
            try
            {
                await FidoService.RemoveSecurityKeyAsync(SecurityKeyId);
                await ToastService.ShowToastAsync("Security key deleted.", ToastType.Success);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CloseAsync();
            }
        }
    }
}
