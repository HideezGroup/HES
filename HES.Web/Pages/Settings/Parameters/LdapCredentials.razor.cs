using HES.Core.Interfaces;
using HES.Core.Models.Web.AppSettings;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.Parameters
{
    public partial class LdapCredentials : OwningComponentBase, IDisposable
    {
        public IAppSettingsService AppSettingsService { get; set; }
        [Parameter] public Func<LdapSettings, Task> LoadEntities { get; set; }
        [Parameter] public EventCallback CancelRequested { get; set; }
        [Parameter] public string Host { get; set; }

        public LdapSettings LdapSettings { get; set; }
        public bool SaveCredentials { get; set; }

        protected override void OnInitialized()
        {
            AppSettingsService = ScopedServices.GetRequiredService<IAppSettingsService>();
            LdapSettings = new LdapSettings() { Host = Host };
        }

        private async Task ConnectAsync()
        {
            if (SaveCredentials)
                await AppSettingsService.SetLdapSettingsAsync(LdapSettings);

            await LoadEntities.Invoke(LdapSettings);
        }    

        public void Dispose()
        {    
            AppSettingsService.Dispose();
        }
    }
}