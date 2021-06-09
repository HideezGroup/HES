using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class EditSaml2RelyingParty : HESModalBase
    {
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<EditSaml2RelyingParty> Logger { get; set; }
        [Parameter] public string RelyingPartyId { get; set; }

        public SamlRelyingParty RelyingParty { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button Button { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();

                RelyingParty = await AppSettingsService.GetSaml2RelyingPartyAsync(RelyingPartyId);
                if (RelyingParty == null)
                {
                    throw new HESException(HESCode.Saml2RelyingPartyNotFound);
                }

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task EditAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await AppSettingsService.EditSaml2RelyingPartyAsync(RelyingParty);
                    await ToastService.ShowToastAsync("Service provider updated.", ToastType.Success);
                    await ModalDialogClose();
                });
            }
            catch (HESException ex) when (ex.Code == HESCode.Saml2IssuerAlreadyExist)
            {
                ValidationErrorMessage.DisplayError(nameof(RelyingParty.Issuer), ex.Message);
            }
            catch (HESException ex) when (ex.Code == HESCode.InvalidCertificate)
            {
                ValidationErrorMessage.DisplayError(nameof(RelyingParty.SignatureValidationCertificateBase64), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        protected override async Task ModalDialogCancel()
        {
            AppSettingsService.UnchangedSaml2RelyingParty(RelyingParty);
            await base.ModalDialogCancel();
        }
    }
}