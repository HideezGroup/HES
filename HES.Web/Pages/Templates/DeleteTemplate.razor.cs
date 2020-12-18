using HES.Core.Entities;
using HES.Core.Enums;
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
    public partial class DeleteTemplate : HESComponentBase, IDisposable
    {
        public ITemplateService TemplateService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Inject] public ILogger<DeleteTemplate> Logger { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public string TemplateId { get; set; }

        public Template Template { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                TemplateService = ScopedServices.GetRequiredService<ITemplateService>();

                Template = await TemplateService.GetByIdAsync(TemplateId);

                if (Template == null)
                    throw new Exception("Template not found.");

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

        private async Task DeleteTemplateAsync()
        {
            try
            {
                await TemplateService.DeleteTemplateAsync(Template.Id);
                await ToastService.ShowToastAsync("Template deleted.", ToastType.Success);
                await SynchronizationService.UpdateTemplates(ExceptPageId);
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Template.Id);
        }
    }
}
