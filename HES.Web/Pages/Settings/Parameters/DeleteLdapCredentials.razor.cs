using HES.Core.Constants;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.AppSettings;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class DeleteLdapCredentials : HESModalBase
    {
        [Inject] public IAppSettingsService AppSettingsService { get; set; }
        [Inject] public ILogger<DeleteLdapCredentials> Logger { get; set; }

        private async Task DeleteAsync()
        {
            try
            {
                var ldapSettings = await AppSettingsService.GetSettingsAsync<LdapSettings>(ServerConstants.Domain);
                ldapSettings.UserName = null;
                ldapSettings.Password = null;
                await AppSettingsService.SetSettingsAsync(ldapSettings, ServerConstants.Domain);
                await ToastService.ShowToastAsync("Domain settings updated.", ToastType.Success);
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