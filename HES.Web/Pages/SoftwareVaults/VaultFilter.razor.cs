using HES.Core.Enums;
using HES.Core.Models.SoftwareVault;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.SoftwareVaults
{
    public partial class VaultFilter : ComponentBase
    {
        [Parameter] public Func<SoftwareVaultFilter, Task> FilterChanged { get; set; }

        SoftwareVaultFilter Filter { get; set; } = new SoftwareVaultFilter();
        public bool Initialized { get; set; }
        public SelectList StatusList { get; set; }
        public SelectList LicenseStatusList { get; set; }
   
        protected override void OnInitialized()
        {
            StatusList = new SelectList(Enum.GetValues(typeof(VaultStatus)).Cast<VaultStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            LicenseStatusList = new SelectList(Enum.GetValues(typeof(VaultLicenseStatus)).Cast<VaultLicenseStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            Initialized = true;
        }

        private async Task FilterAsync()
        {
            await FilterChanged.Invoke(Filter);
        }

        private async Task ClearAsync()
        {
            Filter = new SoftwareVaultFilter();
            await FilterChanged.Invoke(Filter);
        }        
    }
}