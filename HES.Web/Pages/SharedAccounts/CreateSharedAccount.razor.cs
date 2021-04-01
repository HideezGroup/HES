using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Web.SharedAccounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.SharedAccounts
{
    public partial class CreateSharedAccount : HESModalBase
    {
        public ISharedAccountService SharedAccountService { get; set; }
        public ITemplateService TemplateService { get; set; }
        public IRemoteDeviceConnectionsService RemoteDeviceConnectionsService { get; set; }
        [Inject] public ILogger<CreateSharedAccount> Logger { get; set; }

        public SharedAccountAddModel SharedAccount { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button Button { get; set; }
        public List<Template> Templates { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                SharedAccountService = ScopedServices.GetRequiredService<ISharedAccountService>();
                TemplateService = ScopedServices.GetRequiredService<ITemplateService>();
                RemoteDeviceConnectionsService = ScopedServices.GetRequiredService<IRemoteDeviceConnectionsService>();

                Templates = await TemplateService.GetTemplatesAsync();
                SharedAccount = new SharedAccountAddModel();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task CreateAccountAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await SharedAccountService.CreateSharedAccountAsync(SharedAccount);
                    await ToastService.ShowToastAsync("Account created.", ToastType.Success);
                    await ModalDialogClose();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.Name), ex.Message);
            }
            catch (IncorrectUrlException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.Urls), ex.Message);
            }
            catch (IncorrectOtpException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(SharedAccount.OtpSecret), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private void TemplateSelected(ChangeEventArgs e)
        {
            var template = Templates.FirstOrDefault(x => x.Id == e.Value.ToString());
            if (template != null)
            {
                SharedAccount.Name = template.Name;
                SharedAccount.Urls = template.Urls;
                SharedAccount.Apps = template.Apps;
            }
        }
    }
}