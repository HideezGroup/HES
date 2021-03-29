using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Accounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class TemplatesPage : HESComponentBase, IDisposable
    {
        public ITemplateService TemplateService { get; set; }
        public IMainTableService<Template, TemplateFilter> MainTableService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IModalDialogService2 ModalDialogService2 { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<TemplatesPage> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                TemplateService = ScopedServices.GetRequiredService<ITemplateService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<Template, TemplateFilter>>();

                SynchronizationService.UpdateTemplatesPage += UpdateTemplatesPage;

                await BreadcrumbsService.SetTemplates();
                await MainTableService.InitializeAsync(TemplateService.GetTemplatesAsync, TemplateService.GetTemplatesCountAsync, ModalDialogService, StateHasChanged, nameof(Template.Name), ListSortDirection.Ascending);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateTemplatesPage(string exceptPageId, string userName)
        {

            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await MainTableService.LoadTableDataAsync();
                await ToastService.ShowToastAsync($"Page edited by {userName}.", ToastType.Notify);
                StateHasChanged();
            });

        }

        private async Task CreateTemplateAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateTemplateModal));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Create Template", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {              
                await MainTableService.LoadTableDataAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        private async Task EditTemplateAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditTemplate));
                builder.AddAttribute(1, nameof(EditTemplate.TemplateId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(EditTemplate.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Edit Template", body, ModalDialogSize.Default);
        }

        private async Task DeleteTemplateAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteTemplate));
                builder.AddAttribute(1, nameof(DeleteTemplate.TemplateId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, nameof(DeleteTemplate.ExceptPageId), PageId);
                builder.CloseComponent();
            };

            await MainTableService.ShowModalAsync("Delete Template", body, ModalDialogSize.Default);
        }

        public void Dispose()
        {
            SynchronizationService.UpdateTemplatesPage -= UpdateTemplatesPage;
            MainTableService.Dispose();
        }
    }
}