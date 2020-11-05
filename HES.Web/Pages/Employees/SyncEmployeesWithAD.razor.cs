using HES.Core.Enums;
using HES.Core.Hubs;
using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class SyncEmployeesWithAD : OwningComponentBase
    {
        public ILdapService LdapService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<SyncEmployeesWithAD> Logger { get; set; }
        [Inject] public IModalDialogService ModalDialogService { get; set; }
        [Inject] public IToastService ToastService { get; set; }
        [Inject] public IHubContext<RefreshHub> HubContext { get; set; }
        [Parameter] public string ConnectionId { get; set; }

        public LdapSettings LdapSettings { get; set; }
        public bool CredentialsNotSet { get; set; }
        public bool Initialized { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                LdapService = ScopedServices.GetRequiredService<ILdapService>();

                LdapSettings = await AppSettingsService.GetLdapSettingsAsync();

                if (LdapSettings?.UserName == null && LdapSettings?.Password == null)
                {
                    CredentialsNotSet = true;
                }

                Initialized = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogService.CancelAsync();
            }
        }

        private async Task SyncAsync()
        {
            try
            {
                await LdapService.SyncUsersAsync(LdapSettings);
                await LdapService.ChangePasswordWhenExpiredAsync(LdapSettings);
                await HubContext.Clients.AllExcept(ConnectionId).SendAsync(RefreshPage.Employees);
                await ToastService.ShowToastAsync("Users synced.", ToastType.Success);
                await ModalDialogService.CloseAsync();
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