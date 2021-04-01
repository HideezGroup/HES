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

namespace HES.Web.Pages.Templates
{
    public partial class CreateTemplateModal : HESModalBase
    {
        public ITemplateService TemplateService { get; set; }
        [Inject] public ILogger<CreateTemplateModal> Logger { get; set; }

        public Template Template { get; set; } = new Template();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button Button { get; set; }

        protected override void OnInitialized()
        {
            TemplateService = ScopedServices.GetRequiredService<ITemplateService>();
        }

        private async Task CreateTemplateAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await TemplateService.CreateTmplateAsync(Template);
                    await ToastService.ShowToastAsync("Template created.", ToastType.Success);
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
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }
    }
}