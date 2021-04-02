using HES.Core.Models.Accounts;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Templates
{
    public partial class TemplateFilterComponent : ComponentBase
    {
        [Parameter] public Func<TemplateFilter, Task> FilterChanged { get; set; }

        public TemplateFilter Filter { get; set; } = new TemplateFilter();
        public Button ButtonSpinner { get; set; }

        private async Task FilteredAsync()
        {
            await ButtonSpinner.SpinAsync(async () =>
            {
                await FilterChanged.Invoke(Filter);
            });
        }

        private async Task ClearAsync()
        {
            Filter = new TemplateFilter();
            await FilterChanged.Invoke(Filter);
        }
    }
}