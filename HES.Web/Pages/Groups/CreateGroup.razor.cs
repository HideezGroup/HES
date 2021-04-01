using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class CreateGroup : HESModalBase
    {
        public IGroupService GroupService { get; set; }
        [Inject] public ILogger<CreateGroup> Logger { get; set; }
        //[Inject] public IModalDialogService ModalDialogService { get; set; }
        //[Inject] IToastService ToastService { get; set; }
        [Parameter] public string ExceptPageId { get; set; }

        public Group Group = new Group();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        protected override void OnInitialized()
        {
            GroupService = ScopedServices.GetRequiredService<IGroupService>();
        }

        private async Task CreateAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await GroupService.CreateGroupAsync(Group);
                    await ToastService.ShowToastAsync("Group created.", ToastType.Success);               
                    //await SynchronizationService.UpdateGroups(ExceptPageId);
                    //await ModalDialogService.CloseAsync();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Core.Entities.Group.Name), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                //await ModalDialogService.CancelAsync();
            }
        }
    }
}