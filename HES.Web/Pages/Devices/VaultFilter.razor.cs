﻿using HES.Core.Enums;
using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;
using HES.Core.Models.Web.HardwareVault;

namespace HES.Web.Pages.Devices
{
    public partial class VaultFilter : ComponentBase
    {
        [Inject]
        public IEmployeeService EmployeeService { get; set; }

        [Inject]
        public IHardwareVaultService HardwareVaultService { get; set; }

        [Inject]
        public IOrgStructureService OrgStructureService { get; set; }

        [Parameter] 
        public Func<HardwareVaultFilter, Task> FilterChanged { get; set; }

        public HardwareVaultFilter Filter { get; set; }
        public SelectList LicenseStatuses { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Filter = new HardwareVaultFilter();
            LicenseStatuses = new SelectList(Enum.GetValues(typeof(VaultLicenseStatus)).Cast<VaultLicenseStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
        }

        private async Task FilteredAsync()
        {
            await FilterChanged.Invoke(Filter);
        }

        private async Task ClearAsync()
        {
            Filter = new HardwareVaultFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}
