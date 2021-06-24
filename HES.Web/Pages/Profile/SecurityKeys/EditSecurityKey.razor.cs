using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Profile;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.SecurityKeys
{
    public partial class EditSecurityKey : HESModalBase
    {
        public IFido2Service FidoService { get; set; }
        [Inject] public ILogger<EditSecurityKey> Logger { get; set; }
        [Parameter] public string SecurityKeyId { get; set; }

        public EditSecurityKeyModel EditSecurityKeyModel { get; set; }
        public Button ButtonSpinner { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                FidoService = ScopedServices.GetRequiredService<IFido2Service>();

                var credential = await FidoService.GetCredentialsById(SecurityKeyId);
                EditSecurityKeyModel = new EditSecurityKeyModel { Name = credential.SecurityKeyName };

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ModalDialogCancel();
            }
        }

        private async Task UpdateSecurityKeyAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await FidoService.UpdateSecurityKeyNameAsync(SecurityKeyId, EditSecurityKeyModel.Name);
                    await ToastService.ShowToastAsync(Resources.Resource.Profile_Security_EditSecurityKey_Toast, ToastType.Success);
                    await ModalDialogClose();
                });
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