using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Threading.Tasks;
using System.Timers;

namespace HES.Web.Components
{
    public partial class SearchBox : HESDomComponentBase
    {
        [Parameter] public Func<string, Task> SearchTextChanged { get; set; }
        public string SearchText { get; set; }

        private Timer _timer;

        protected override void OnInitialized()
        {
            SearchBoxTimer();
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
    }
}