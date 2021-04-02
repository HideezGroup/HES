using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.SoftwareVaults
{
    public partial class ResendSoftwareVaultInvitation : OwningComponentBase, IDisposable
    {
        public ISoftwareVaultService SoftwareVaultService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<ResendSoftwareVaultInvitation> Logger { get; set; }
        //[Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] IToastService ToastService { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }
        [Parameter] public SoftwareVaultInvitation SoftwareVaultInvitation { get; set; }

        public ServerSettings ServerSettings { get; set; }

        private bool _initialized;
        protected override async Task OnInitializedAsync()
        {
            try
            {
                SoftwareVaultService = ScopedServices.GetRequiredService<ISoftwareVaultService>();

                ServerSettings = await AppSettingsService.GetServerSettingsAsync();
                _initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                //await ModalDialogService.CloseAsync();
            }
        }

        public async Task SendAsync()
        {
            try
            {
                await SoftwareVaultService.ResendInvitationAsync(SoftwareVaultInvitation.Employee, ServerSettings, SoftwareVaultInvitation.Id);
                await Refresh.InvokeAsync(this);
                await ToastService.ShowToastAsync("Invitation sent.", ToastType.Success);
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

        public void Dispose()
        {
            SoftwareVaultService.Dispose();
        }
    }
}