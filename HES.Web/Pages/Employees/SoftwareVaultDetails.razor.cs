using HES.Core.Entities;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Employees
{
    public partial class SoftwareVaultDetails: HESModalBase
    {
        [Parameter] public SoftwareVault SoftwareVault { get; set; }  
    }
}