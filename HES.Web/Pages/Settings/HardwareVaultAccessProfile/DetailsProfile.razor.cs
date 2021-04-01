using HES.Core.Entities;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class DetailsProfile : HESModalBase
    {
        [Parameter] public HardwareVaultProfile AccessProfile { get; set; }
    }
}
