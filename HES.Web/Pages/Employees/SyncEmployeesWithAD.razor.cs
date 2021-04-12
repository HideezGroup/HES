using HES.Core.Constants;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Employees
{
    public partial class SyncEmployeesWithAD : HESModalBase
    {
        public ILdapService LdapService { get; set; }
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<SyncEmployeesWithAD> Logger { get; set; }

        public LdapSettings LdapSettings { get; set; }
        public bool CredentialsNotSet { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                LdapService = ScopedServices.GetRequiredService<ILdapService>();

                LdapSettings = await AppSettingsService.GetSettingsAsync<LdapSettings>(ServerConstants.Domain);

                if (LdapSettings?.UserName == null && LdapSettings?.Password == null)
                {
                    CredentialsNotSet = true;
                }

                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                await ToastService.ShowToastAsync(ex.Message, ToastType.Error);
                await ModalDialogCancel();
            }
        }

        private async Task SyncAsync()
        {
            try
            {
                await LdapService.SyncUsersAsync(LdapSettings);
                await LdapService.ChangePasswordWhenExpiredAsync(LdapSettings);
                await ToastService.ShowToastAsync("Users synced.", ToastType.Success);
                await ModalDialogClose();
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