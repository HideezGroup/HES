using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.OrgStructure
{
    public partial class PositionsTab : HESPageBase, IDisposable
    {
        public IOrgStructureService OrgStructureService { get; set; }
        [Inject] public ILogger<PositionsTab> Logger { get; set; }

        public List<Position> Positions { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public bool IsSortedAscending { get; set; } = true;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                OrgStructureService = ScopedServices.GetRequiredService<IOrgStructureService>();

                SynchronizationService.UpdateOrgSructurePositionsPage += UpdateOrgSructurePositionsPage;
 
                await BreadcrumbsService.SetOrgStructure();
                await LoadPositionsAsync();

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateOrgSructurePositionsPage(string exceptPageId, string userName)
        {
            if (PageId == exceptPageId)
                return;

            await InvokeAsync(async () =>
            {
                await LoadPositionsAsync();
                await ToastService.ShowToastAsync($"Page edited by {userName}.", ToastType.Notify);
                StateHasChanged();
            });

        }

        private string GetSortIcon()
        {
            if (IsSortedAscending)
            {
                return "table-sort-arrow-up";
            }
            else
            {
                return "table-sort-arrow-down";
            }
        }

        private void SortTable()
        {
            IsSortedAscending = !IsSortedAscending;

            if (IsSortedAscending)
            {
                Positions = Positions.OrderBy(x => x.Name).ToList();
            }
            else
            {
                Positions = Positions.OrderByDescending(x => x.Name).ToList();
            }
        }

        private async Task LoadPositionsAsync()
        {
            Positions = await OrgStructureService.GetPositionsAsync();
            StateHasChanged();
        }

        private async Task OpenDialogCreatePositionAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreatePosition));
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Create Position", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadPositionsAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        private async Task OpenDialogEditPositionAsync(Position position)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditPosition));
                builder.AddAttribute(1, nameof(EditPosition.PositionId), position.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Edit Position", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadPositionsAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        private async Task OpenDialogDeletePositionAsync(Position position)
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeletePosition));
                builder.AddAttribute(1, nameof(DeletePosition.PositionId), position.Id);
                builder.CloseComponent();
            };

            var instance = await ModalDialogService2.ShowAsync("Delete Position", body, ModalDialogSize2.Default);
            var result = await instance.Result;

            if (result.Succeeded)
            {
                await LoadPositionsAsync();
                await SynchronizationService.UpdateTemplates(PageId);
            }
        }

        public void Dispose()
        {
            SynchronizationService.UpdateOrgSructurePositionsPage -= UpdateOrgSructurePositionsPage;
        }
    }
}
