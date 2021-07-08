using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models.Identity;
using HES.Web.Components;
using HES.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.PersonalData
{
    public partial class DeletePersonalData : HESModalBase
    {
        [Inject] public NavigationManager NavigationManager { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public ILogger<DeletePersonalData> Logger { get; set; }
        [Parameter] public ApplicationUser ApplicationUser { get; set; }

        public UserPasswordModel PasswordModel { get; set; } = new UserPasswordModel();
        public Button ButtonSpinner { get; set; }

        protected override void OnInitialized()
        {
            SetInitialized();
        }

        private async Task DeletePersonalDataAsync()
        {
            try
            {
                await ButtonSpinner.SpinAsync(async () =>
                {
                    await JSRuntime.InvokeWebApiPostVoidAsync(Routes.ApiDeletePersonalData, PasswordModel);
                    await JSRuntime.InvokeWebApiPostVoidAsync(Routes.ApiLogout);
                    NavigationManager.NavigateTo(Routes.Login, true);
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }
    }
}