using HES.Core.Interfaces;
using HES.Core.Models.Filters;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Settings.LicenseOrders
{
    public partial class LicenseOrderFilterComponent : ComponentBase
    {
        [Inject] public ILicenseService LicenseService { get; set; }
        [Parameter] public Func<LicenseOrderFilter, Task> FilterChanged { get; set; }

        public LicenseOrderFilter Filter { get; set; }
        public Button Button { get; set; }

        protected override void OnInitialized()
        {
            Filter = new LicenseOrderFilter();
        }

        private async Task FilteredAsync()
        {
            await Button.SpinAsync(async () =>
            {
                await FilterChanged.Invoke(Filter);
            });
        }

        private void OnChange_ProlongLicense(ChangeEventArgs args)
        {
            var value = (string)args.Value;
            if (string.IsNullOrWhiteSpace(value))
                return;

            if (value == "Yes")
            {
                Filter.ProlongLicense = true;
                return;
            }

            Filter.ProlongLicense = false;
        }

        private async Task ClearAsync()
        {
            Filter = new LicenseOrderFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}