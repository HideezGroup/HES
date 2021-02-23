using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Web.Pages.Profile.SecurityKeys
{
    public partial class AddSecurityKey : HESComponentBase
    {
        public enum SecurityKeyAddingStep
        {
            Start,
            Configuration,
            Done,
            Error
        }

        [Inject] public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public ILogger<AddSecurityKey> Logger { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public HttpClient HttpClient { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public IFido2Service FidoService { get; set; }
        public SecurityKeyAddingStep AddingStep { get; set; }
        public ApplicationUser CurrentUser { get; set; }
        public FidoStoredCredential FidoStoredCredential { get; set; }
        public string SecurityKeyName { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                FidoService = ScopedServices.GetRequiredService<IFido2Service>();

                CurrentUser = await ApplicationUserService.GetUserByEmailAsync(await GetCurrentUserEmail());

                ChangeState(SecurityKeyAddingStep.Start);

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task ConfigureSecurityKeyAsync()
        {
            try
            {
                ChangeState(SecurityKeyAddingStep.Configuration);

                FidoStoredCredential = await FidoService.AddSecurityKeyAsync(CurrentUser.Email, JSRuntime);
                await ToastService.ShowToastAsync("Security key added.", ToastType.Success);

                ChangeState(SecurityKeyAddingStep.Done);
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
                await Refresh.InvokeAsync();
                await ModalDialogService.CloseAsync();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CloseAsync();
            }
        }

        private void ChangeState(SecurityKeyAddingStep securityKeyAddingStep)
        {
            AddingStep = securityKeyAddingStep;
            StateHasChanged();
        }
    }
}