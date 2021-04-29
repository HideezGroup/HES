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

namespace HES.Web.Pages.Groups
{
    public partial class EditGroup : HESModalBase, IDisposable
    {
        public IGroupService GroupService { get; set; }
        [Inject] public ILogger<EditGroup> Logger { get; set; }
        //[Inject] public IModalDialogService ModalDialogService { get; set; }
        //[Inject] public IToastService ToastService { get; set; }
        [Inject] public IMemoryCache MemoryCache { get; set; }
        [Parameter] public string GroupId { get; set; }
        [Parameter] public string ExceptPageId { get; set; }

        public Group Group { get; set; }
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        //public ButtonSpinner ButtonSpinner { get; set; }
        public bool EntityBeingEdited { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                GroupService = ScopedServices.GetRequiredService<IGroupService>();

                //ModalDialogService.OnCancel += ModalDialogService_OnCancel;

                //Group = await GroupService.GetGroupByIdAsync(GroupId);

                if (Group == null)
                    throw new Exception("Group not found");

                EntityBeingEdited = MemoryCache.TryGetValue(Group.Id, out object _);
                if (!EntityBeingEdited)
                    MemoryCache.Set(Group.Id, Group);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                //await ModalDialogService.CancelAsync();
            }
        }

        private async Task EditAsync()
        {
            try
            {
                //await ButtonSpinner.SpinAsync(async () =>
                //{
                //    await GroupService.EditGroupAsync(Group);
                //    await ToastService.ShowToastAsync("Group updated.", ToastType.Success);          
                //    //await SynchronizationService.UpdateGroups(ExceptPageId);
                //    //await ModalDialogService.CloseAsync();
                //});
            }
            //catch (AlreadyExistException ex)
            //{
            //    ValidationErrorMessage.DisplayError(nameof(Core.Entities.Group.Name), ex.Message);
            //}
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                //await ModalDialogService.CancelAsync();
            }
        }

        private async Task ModalDialogService_OnCancel()
        {
            //await GroupService.UnchangedGroupAsync(Group);
        }

        public void Dispose()
        {
            //ModalDialogService.OnCancel -= ModalDialogService_OnCancel;

            if (!EntityBeingEdited)
                MemoryCache.Remove(Group.Id);
        }
    }
}