using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Filters;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class TemplatesPage : HESPageBase, IDisposable
    {
        public ITemplateService TemplateService { get; set; }
        public IDataTableService<Template, TemplateFilter> DataTableService { get; set; }
        [Inject] public ILogger<TemplatesPage> Logger { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                TemplateService = ScopedServices.GetRequiredService<ITemplateService>();
                DataTableService = ScopedServices.GetRequiredService<IDataTableService<Template, TemplateFilter>>();

                PageSyncService.UpdateTemplatesPage += UpdateTemplatesPage;

                await BreadcrumbsService.SetTemplates();
                await DataTableService.InitializeAsync(TemplateService.GetTemplatesAsync, TemplateService.GetTemplatesCountAsync, StateHasChanged, nameof(Template.Name), ListSortDirection.Ascending);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateTemplatesPage(string exceptPageId)
        {

            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await DataTableService.LoadTableDataAsync();
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

            var instance = await ModalDialogService.ShowAsync("Create Template", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {              
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateTemplates(PageId);
            }
        }

        private async Task EditTemplateAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditTemplate));
                builder.AddAttribute(1, nameof(EditTemplate.TemplateId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Edit Template", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateTemplates(PageId);
            }
        }

        private async Task DeleteTemplateAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteTemplate));
                builder.AddAttribute(1, nameof(DeleteTemplate.TemplateId), DataTableService.SelectedEntity.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService.ShowAsync("Delete Template", body, ModalDialogSize.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await DataTableService.LoadTableDataAsync();
                await PageSyncService.UpdateTemplates(PageId);
            }
        }

        public void Dispose()
        {
            PageSyncService.UpdateTemplatesPage -= UpdateTemplatesPage; 
        }
    }
}