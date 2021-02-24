using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Areas.Identity.Pages.Account.Manage.SecurityKeys
{
    public partial class EditSecurityKey : OwningComponentBase
    {
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public ILogger<EditSecurityKey> Logger { get; set; }
        [Parameter] public string SecurityKeyId { get; set; }

        public IFido2Service FidoService { get; set; }
        public string SecurityKeyName { get; set; }
        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                FidoService = ScopedServices.GetRequiredService<IFido2Service>();
                var credential = await FidoService.GetCredentialsById(SecurityKeyId);
                SecurityKeyName = credential.SecurityKeyName;

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task UpdateSecurityKeyAsync()
        {
            try
            {
                await FidoService.UpdateSecurityKeyNameAsync(SecurityKeyId, SecurityKeyName);
                await ToastService.ShowToastAsync("Security key updated.", ToastType.Success);
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
