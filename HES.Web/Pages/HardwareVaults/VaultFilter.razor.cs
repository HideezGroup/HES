using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.Filters;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.HardwareVaults
{
    public partial class VaultFilter : ComponentBase
    {
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IHardwareVaultService HardwareVaultService { get; set; }
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Parameter] public Func<HardwareVaultFilter, Task> FilterChanged { get; set; }

        public HardwareVaultFilter Filter { get; set; }
        public SelectList VaultStatusList { get; set; }
        public SelectList LicenseStatusList { get; set; }
        public Button ButtonSpinner { get; set; }

        protected override void OnInitialized()
        {
            Filter = new HardwareVaultFilter();
            VaultStatusList = new SelectList(Enum.GetValues(typeof(VaultStatus)).Cast<VaultStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            LicenseStatusList = new SelectList(Enum.GetValues(typeof(VaultLicenseStatus)).Cast<VaultLicenseStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
        }

        private async Task FilteredAsync()
        {
            await ButtonSpinner.SpinAsync(async () =>
            {
                await FilterChanged.Invoke(Filter);
            });
        }

        private async Task ClearAsync()
        {
            Filter = new HardwareVaultFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}