using HES.Core.Entities;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class DetailsLicenseOrder : HESModalBase
    {
        [Parameter] public LicenseOrder LicenseOrder { get; set; }
    }
}