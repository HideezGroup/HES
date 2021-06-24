using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.SecurityKeys
{
    public partial class AddSecurityKey : HESModalBase
    {
        public enum SecurityKeyAddingStep
        {
            Start,
            Configuration,
            Done,
            Error
        }

        [Inject] public ILogger<AddSecurityKey> Logger { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Parameter] public ApplicationUser CurrentUser { get; set; }

        public IFido2Service FidoService { get; set; }
        public SecurityKeyAddingStep AddingStep { get; set; }
        public FidoStoredCredential FidoStoredCredential { get; set; }
        public string SecurityKeyName { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                FidoService = ScopedServices.GetRequiredService<IFido2Service>();
                ChangeState(SecurityKeyAddingStep.Start);
                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task ConfigureSecurityKeyAsync()
        {
            try
            {
                ChangeState(SecurityKeyAddingStep.Configuration);

                FidoStoredCredential = await FidoService.AddSecurityKeyAsync(CurrentUser.Email, JSRuntime);
                await ToastService.ShowToastAsync(Resources.Resource.Profile_Security_AddSecurityKey_Toast, ToastType.Success);

                ChangeState(SecurityKeyAddingStep.Done);
            }
            catch (JSException ex)
            {
                Logger.LogWarning($"JS. {CurrentUser.Email}. {ex.Message}");
                ChangeState(SecurityKeyAddingStep.Error);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ChangeState(SecurityKeyAddingStep.Error);
            }
        }

        private async Task SaveSecurityKeyAsync()
        {
            try
            {
                await FidoService.UpdateSecurityKeyNameAsync(FidoStoredCredential.Id, SecurityKeyName);
                await ModalDialogClose();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private void ChangeState(SecurityKeyAddingStep securityKeyAddingStep)
        {
            AddingStep = securityKeyAddingStep;
            StateHasChanged();
        }
    }
}