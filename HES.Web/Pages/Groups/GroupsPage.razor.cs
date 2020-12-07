using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Web.Group;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class GroupsPage : HESComponentBase, IDisposable
    {
        public IGroupService GroupService { get; set; }
        public IMainTableService<Group, GroupFilter> MainTableService { get; set; }
        [Inject] public IBreadcrumbsService BreadcrumbsService { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<GroupsPage> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                GroupService = ScopedServices.GetRequiredService<IGroupService>();
                MainTableService = ScopedServices.GetRequiredService<IMainTableService<Group, GroupFilter>>();

                SynchronizationService.UpdateGroupsPage += UpdateGroupsPage;
                    
                await BreadcrumbsService.SetGroups();
                await MainTableService.InitializeAsync(GroupService.GetGroupsAsync, GroupService.GetGroupsCountAsync, ModalDialogService, StateHasChanged, nameof(Group.Name));

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private async Task UpdateGroupsPage(string exceptPageId, string userName)
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

        private Task NavigateToGroupDetails()
        {
            NavigationManager.NavigateTo($"/Groups/Details/{MainTableService.SelectedEntity.Id}");
            return Task.CompletedTask;
        }

        private async Task OpenModalAddGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(AddGroup));
                builder.AddAttribute(1, "ExceptPageId", PageId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Add group", body);
        }

        private async Task OpenModalCreateGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(CreateGroup));
                builder.AddAttribute(1, "ExceptPageId", PageId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Create group", body);
        }

        private async Task OpenModalEditGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(EditGroup));
                builder.AddAttribute(1, nameof(DeleteGroup.GroupId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, "ExceptPageId", PageId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Edit group", body);
        }

        private async Task OpenModalDeleteGroupAsync()
        {
            RenderFragment body = (builder) =>
            {
                builder.OpenComponent(0, typeof(DeleteGroup));
                builder.AddAttribute(1, nameof(DeleteGroup.GroupId), MainTableService.SelectedEntity.Id);
                builder.AddAttribute(2, "ExceptPageId", PageId);
                builder.CloseComponent();
            };

            await ModalDialogService.ShowAsync("Delete group", body);
        }

        public void Dispose()
        {
            SynchronizationService.UpdateGroupsPage -= UpdateGroupsPage;
            MainTableService.Dispose();
        }
    }
}