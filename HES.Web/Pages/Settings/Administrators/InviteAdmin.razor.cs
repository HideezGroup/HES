using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppUsers;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Administrators
{
    public partial class InviteAdmin : HESComponentBase
    {
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IEmailSenderService EmailSenderService { get; set; }
        [Inject] public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<InviteAdmin> Logger { get; set; }

        [Parameter] public string ExceptPageId { get; set; }

        public Invitation Invitation = new Invitation();
        public ValidationErrorMessage ValidationErrorMessage { get; set; }
        public ButtonSpinner ButtonSpinner { get; set; }

        private async Task InviteAdminAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    var callBakcUrl = await ApplicationUserService.InviteAdministratorAsync(Invitation.Email, NavigationManager.BaseUri);
                    await EmailSenderService.SendUserInvitationAsync(Invitation.Email, callBakcUrl);
                    await ToastService.ShowToastAsync("Administrator invited.", ToastType.Success);              
                    await SynchronizationService.UpdateAdministrators(ExceptPageId);
                    await ModalDialogService.CloseAsync();
                });
            }
            catch (AlreadyExistException ex)
            {
                ValidationErrorMessage.DisplayError(nameof(Invitation.Email), ex.Message);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }  
        }
    }
}