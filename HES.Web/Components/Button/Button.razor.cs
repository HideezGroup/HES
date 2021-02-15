using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class Button : HESDomComponentBase
    {
        [Parameter] public RenderFragment Image { get; set; }
        [Parameter] public EventCallback OnClick { get; set; }
        [Parameter] public string Text { get; set; } = "Button";
        [Parameter] public bool IsDisabled { get; set; }

        private bool _busy;

        private async Task ClickHandler()
        {
            if (_busy)
                return;

            _busy = true;

            try
            {
                await OnClick.InvokeAsync(this);
            }
            finally
            {
                _busy = false;
            }
        }

        public async Task SpinAsync(Func<Task> func)
        {
            if (_busy)
                return;

            _busy = true;
            StateHasChanged();

            try
            {
                await func.Invoke();
            }
            finally
            {
                _busy = false;
                StateHasChanged();
            }
        }

        public void ChangeBtnText(string text)
        {
            Text = text;
            StateHasChanged();
        }
    }
}