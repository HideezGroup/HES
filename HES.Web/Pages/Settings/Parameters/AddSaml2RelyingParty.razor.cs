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
    public partial class AddSaml2RelyingParty : HESModalBase
    {
        public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<AddSaml2RelyingParty> Logger { get; set; }

        public SamlRelyingParty RelyingParty { get; set; } = new();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button Button { get; set; }

        protected override void OnInitialized()
        {
            AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();
        }

        private async Task AddAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await AppSettingsService.AddSaml2RelyingPartyAsync(RelyingParty);
                    await ToastService.ShowToastAsync("Service provider added.", ToastType.Success);
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
    }
}