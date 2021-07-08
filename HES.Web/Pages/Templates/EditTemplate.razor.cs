using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class EditTemplate : HESModalBase, IDisposable
    {
        public ITemplateService TemplateService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditTemplate> Logger { get; set; }
        [Parameter] public string TemplateId { get; set; }

        public Template Template { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public Button Button { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                TemplateService = ScopedServices.GetRequiredService<ITemplateService>();

                Template = await TemplateService.GetTemplateByIdAsync(TemplateId);

                if (Template == null)
                    throw new HESException(HESCode.TemplateNotFound);

                EntityBeingEdited = MemoryCache.TryGetValue(Template.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Template.Id, Template);

                SetInitialized();
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
            TemplateService.UnchangedTemplate(Template);
            await base.ModalDialogCancel();
        }

        private async Task EditTemplateAsync()
        {
            try
            {
                await Button.SpinAsync(async () =>
                {
                    await TemplateService.EditTemplateAsync(Template);
                    await ToastService.ShowToastAsync(Resources.Resource.Templates_EditTemplate_Toast, ToastType.Success);
                    await ModalDialogClose();
                });
            }
            catch (HESException ex) when (ex.Code == HESCode.TemplateExist)
            {
                ValidationErrorMessage.DisplayError(nameof(Template.Name), ex.Message);
            }
            catch (HESException ex) when (ex.Code == HESCode.IncorrectUrl)
            {
                ValidationErrorMessage.DisplayError(nameof(Template.Urls), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Template.Id);
        }
    }
}