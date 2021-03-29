using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace HES.Web.Components
{
    public partial class DataTableTableActions : HESDomComponentBase
    {
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Parameter] public RenderFragment FilterForm { get; set; }
        [Parameter] public RenderFragment ActionButtons { get; set; }
        [Parameter] public Func<string, Task> SearchTextChanged { get; set; }
        [Parameter] public bool ShowFilterButton { get; set; } = true;
        [Parameter] public bool CollapseFilter { get; set; }
        [Parameter] public string TooltipText { get; set; }
        [Parameter] public Func<Task> RefreshTable { get; set; }

        public string SearchText { get; set; }

        private Timer _timer;

        protected override void OnInitialized()
        {
            SearchBoxTimer();
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await JSRuntime.InvokeVoidAsync("initTooltips");
            }
        }
       
        private void SearchBoxTimer()
        {
            _timer = new Timer(500);
            _timer.Elapsed += async (sender, args) =>
            {
                await InvokeAsync(async () =>
                {
                    await SearchTextChanged.Invoke(SearchText);
                });
            };
            _timer.AutoReset = false;
        }

        private void SearchBoxKeyUp(KeyboardEventArgs e)
        {
            _timer.Stop();
            _timer.Start();
        }

        private async Task RefreshAsync()
        {
            if (RefreshTable == null)
                return;

            await RefreshTable.Invoke();
        }
    }
}