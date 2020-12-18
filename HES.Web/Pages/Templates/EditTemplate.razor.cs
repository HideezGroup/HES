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
    public partial class EditTemplate : HESComponentBase, IDisposable
    {
        public ITemplateService TemplateService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<EditTemplate> Logger { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public string TemplateId { get; set; }

        public Template Template { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                TemplateService = ScopedServices.GetRequiredService<ITemplateService>();

                Template = await TemplateService.GetByIdAsync(TemplateId);

                if (Template == null)
                    throw new Exception("Template not found.");

                ModalDialogService.OnCancel += OnCancelAsync;

                EntityBeingEdited = MemoryCache.TryGetValue(Template.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Template.Id, Template);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task EditTemplateAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await TemplateService.EditTemplateAsync(Template);
                    await ToastService.ShowToastAsync("Template updated.", ToastType.Success);
                    await SynchronizationService.UpdateTemplates(ExceptPageId);
                    await ModalDialogService.CloseAsync();
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
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task OnCancelAsync()
        {
            await TemplateService.UnchangedTemplateAsync(Template);
        }

        public void Dispose()
        {
            ModalDialogService.OnCancel -= OnCancelAsync;

            if (!EntityBeingEdited)
                MemoryCache.Remove(Template.Id);
        }
    }
}