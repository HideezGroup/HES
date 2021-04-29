using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Core.Models.SoftwareVault;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HES.Web.Pages.SoftwareVaults
{
    public partial class InvitationFilter : ComponentBase
    {
        [Inject] public ISoftwareVaultService SoftwareVaultService { get; set; }
        [Inject] public IEmployeeService EmployeeService { get; set; }
        [Inject] public IOrgStructureService OrgStructureService { get; set; }
        [Parameter] public Func<SoftwareVaultInvitationFilter, Task> FilterChanged { get; set; }

        SoftwareVaultInvitationFilter Filter { get; set; } = new SoftwareVaultInvitationFilter();
        public bool Initialized { get; set; }
        public SelectList StatusList { get; set; }

        protected override void OnInitialized()
        {
            StatusList = new SelectList(Enum.GetValues(typeof(SoftwareVaultInvitationStatus)).Cast<SoftwareVaultInvitationStatus>().ToDictionary(t => (int)t, t => t.ToString()), "Key", "Value");
            Initialized = true;
        }

        private async Task FilterdAsync()
        {
            await FilterChanged.Invoke(Filter);
        }

        private async Task ClearAsync()
        {
            Filter = new SoftwareVaultInvitationFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}