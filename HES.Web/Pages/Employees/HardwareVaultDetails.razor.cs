﻿using HES.Core.Entities;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Pages.Employees
{
    public partial class HardwareVaultDetails : HESModalBase
    {
        [Parameter] public HardwareVault HardwareVault { get; set; }
    }
}