﻿using HES.Core.Models.Web.HardwareVaults;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.HardwareVaultAccessProfile
{
    public partial class ProfileFilter : ComponentBase
    {
        [Parameter] public Func<HardwareVaultProfileFilter, Task> FilterChanged { get; set; }

        public HardwareVaultProfileFilter Filter { get; set; } = new HardwareVaultProfileFilter();

        private async Task FilteredAsync()
        {
            await FilterChanged.Invoke(Filter);
        }

        private async Task ClearAsync()
        {
            Filter = new HardwareVaultProfileFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}