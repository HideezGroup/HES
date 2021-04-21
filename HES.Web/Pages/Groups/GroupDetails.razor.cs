using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Groups;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class GroupDetails : HESPageBase, IDisposable
    {
        public IGroupService GroupService { get; set; }
        [Inject] public ILogger<GroupDetails> Logger { get; set; }
        [Parameter] public string GroupId { get; set; }

        public Group Group { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                GroupService = ScopedServices.GetRequiredService<IGroupService>();
                //MainTableService = ScopedServices.GetRequiredService<IMainTableService<GroupMembership, GroupMembershipFilter>>();

                //SynchronizationService.UpdateGroupDetailsPage += UpdateGroupDetailsPage;
  
                await LoadGroupAsync();
                await BreadcrumbsService.SetGroupDetails(Group.Name);
                //await MainTableService.InitializeAsync(GroupService.GetGruopMembersAsync, GroupService.GetGruopMembersCountAsync, ModalDialogService, StateHasChanged, nameof(GroupMembership.Employee.FullName), entityId: GroupId);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                //SetLoadFailed(ex.Message);
            }
        }

        //private async Task UpdateGroupDetailsPage(string exceptPageId, string groupId)
        //{
        //    //if (Group.Id != groupId || PageId == exceptPageId)
        //    //    return;

        //    await InvokeAsync(async () =>
        //    {      
        //        //await MainTableService.LoadTableDataAsync();
        //        StateHasChanged();
        //    });
        //}

        private async Task LoadGroupAsync()
        {
            //Group = await GroupService.GetGroupByIdAsync(GroupId);
            if (Group == null)
                throw new Exception("Group not found.");
            StateHasChanged();
        }

        //private async Task OpenModalAddEmployeesAsync()
        //{
        //    RenderFragment body = (builder) =>
        //    {
        //        builder.OpenComponent(0, typeof(AddEmployee));
        //        //builder.AddAttribute(1, "ExceptPageId", PageId);
        //        builder.AddAttribute(2, "GroupId", GroupId);
        //        builder.CloseComponent();
        //    };

        //    //await ModalDialogService.ShowAsync("Add Employees", body, ModalDialogSize.Large);
        //}

        //private async Task OpenModalDeleteEmployeeAsync()
        //{
        //    RenderFragment body = (builder) =>
        //    {
        //        builder.OpenComponent(0, typeof(RemoveEmployee));
        //        //builder.AddAttribute(1, "ExceptPageId", PageId);
        //        builder.AddAttribute(2, "GroupId", GroupId);
        //        //builder.AddAttribute(3, "EmployeeId", MainTableService.SelectedEntity.EmployeeId);
        //        builder.CloseComponent();
        //    };

        //    //await ModalDialogService.ShowAsync("Delete Employee", body);
        //}

        public void Dispose()
        {
            //SynchronizationService.UpdateGroupDetailsPage -= UpdateGroupDetailsPage;
            //MainTableService.Dispose();
        }
    }
}