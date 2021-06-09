using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
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
                    await ToastService.ShowToastAsync(Resources.Resource.Parameters_AddSaml2RelyingParty_Toast, ToastType.Success);
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

        private async Task OnInputFileChangeAsync(InputFileChangeEventArgs args)
        {
            try
            {
                var file = args.File;
                if (file == null)
                {
                    return;
                }

                using var stream = file.OpenReadStream();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                var metadata = Encoding.UTF8.GetString(ms.ToArray());

                var entityDescriptor = new EntityDescriptor();
                entityDescriptor = entityDescriptor.ReadSPSsoDescriptor(metadata);

                if (entityDescriptor.SPSsoDescriptor == null)
                {
                    throw new HESException(HESCode.Saml2SPDescriptorNotLoaded);
                }

                RelyingParty.Issuer = entityDescriptor.EntityId;
                RelyingParty.SingleSignOnDestination = entityDescriptor.SPSsoDescriptor.AssertionConsumerServices.First().Location.ToString();
                var singleLogoutService = entityDescriptor.SPSsoDescriptor.SingleLogoutServices.First();
                RelyingParty.SingleLogoutResponseDestination = singleLogoutService.ResponseLocation != null ? singleLogoutService.ResponseLocation.ToString() : singleLogoutService.Location.ToString();
                RelyingParty.SignatureValidationCertificate = entityDescriptor.SPSsoDescriptor.SigningCertificates.First();
            }
            catch (Exception ex)
            {
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
        }
    }
}