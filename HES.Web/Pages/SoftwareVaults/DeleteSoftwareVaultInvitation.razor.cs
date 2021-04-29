using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.SoftwareVaults
{
    public partial class DeleteSoftwareVaultInvitation : ComponentBase
    {
        [Inject] public ISoftwareVaultService SoftwareVaultService { get; set; }
        [Inject] public ILogger<DeleteSoftwareVaultInvitation> Logger { get; set; }
        //[Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public SoftwareVaultInvitation SoftwareVaultInvitation { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                if (SoftwareVaultInvitation == null)
                {
                    throw new ArgumentNullException(nameof(SoftwareVaultInvitation));
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                //await ModalDialogService.CloseAsync();
            }
        }

        public async Task DeleteAsync()
        {
            //try
            //{
            //    await SoftwareVaultService.DeleteInvitationAsync(SoftwareVaultInvitation.Id);
            //    await Refresh.InvokeAsync(this);
            //    await ToastService.ShowToastAsync("Invitation deleted.", ToastType.Success);
            //}
            //catch (Exception ex)
            //{
            //    Logger.LogError(ex.Message);
            //    await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            //}
            //finally
            //{
            //    await ModalDialogService.CloseAsync();
            //}
        }
    }
}