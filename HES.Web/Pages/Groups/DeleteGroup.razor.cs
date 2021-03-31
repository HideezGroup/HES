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

namespace HES.Web.Pages.Groups
{
    public partial class DeleteGroup : HESModalBase, IDisposable
    {
        public IGroupService GroupService { get; set; }
        [Inject] public ILogger<DeleteGroup> Logger { get; set; }
        //[Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Parameter] public string ExceptPageId { get; set; }
        [Parameter] public string GroupId { get; set; }

        public Group Group { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                GroupService = ScopedServices.GetRequiredService<IGroupService>();

                Group = await GroupService.GetGroupByIdAsync(GroupId);

                if (Group == null)
                    throw new Exception("Group not found");

                EntityBeingEdited = MemoryCache.TryGetValue(Group.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Group.Id, Group);

            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                //await ModalDialogService.CloseAsync();
            }
        }

        public async Task DeleteAsync()
        {
            try
            {
                await GroupService.DeleteGroupAsync(GroupId);               
                //await SynchronizationService.UpdateGroups(ExceptPageId);
                await ToastService.ShowToastAsync("Group deleted.", ToastType.Success);
                //await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                //await ModalDialogService.CancelAsync();
            }
        }

        public void Dispose()
        {
            if (!EntityBeingEdited)
                MemoryCache.Remove(Group.Id);
        }
    }
}