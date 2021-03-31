using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class RemoveEmployee : HESModalBase
    {
        public IGroupService GroupService { get; set; }
        [Inject] public ILogger<RemoveEmployee> Logger { get; set; }
        //[Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public string GroupId { get; set; }
        [Parameter] public string EmployeeId { get; set; }

        public GroupMembership GroupMembership { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                GroupService = ScopedServices.GetRequiredService<IGroupService>();

                GroupMembership = await GroupService.GetGroupMembershipAsync(EmployeeId, GroupId);

                if (GroupMembership == null)
                    throw new Exception("Group membership not found");

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                //await ModalDialogService.CancelAsync();
            }
        }

        public async Task DeleteAsync()
        {
            try
            {
                await GroupService.RemoveEmployeeFromGroupAsync(GroupMembership.Id);         
                //await SynchronizationService.UpdateGroupDetails(ExceptPageId, GroupId);
                await ToastService.ShowToastAsync("Employee removed.", ToastType.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
            }
            finally
            {
                //await ModalDialogService.CloseAsync();
            }
        }
    }
}