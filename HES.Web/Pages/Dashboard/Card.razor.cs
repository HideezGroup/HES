using HES.Core.Models.Dashboard;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Dashboard
{
    public partial class Card : ComponentBase
    {
        [Parameter] public DashboardCard DashboardCard { get; set; }
    }
}