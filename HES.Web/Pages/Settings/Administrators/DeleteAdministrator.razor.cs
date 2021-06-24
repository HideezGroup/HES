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

namespace HES.Web.Pages.Settings.Administrators
{
    public partial class DeleteAdministrator : HESModalBase
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public ILogger<DeleteAdministrator> Logger { get; set; }
        [Parameter] public string ApplicationUserId { get; set; }

        public ApplicationUser ApplicationUser { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();

                ApplicationUser = await ApplicationUserService.GetUserByIdAsync(ApplicationUserId);
                if (ApplicationUser == null)
                {
                    throw new HESException(HESCode.UserNotFound);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task DeleteUserAsync()
        {
            try
            {
                await ApplicationUserService.DeleteUserAsync(ApplicationUserId);
                await ToastService.ShowToastAsync(Resources.Resource.Administrators_DeleteAdministrator_Toast, ToastType.Success);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }
    }
}