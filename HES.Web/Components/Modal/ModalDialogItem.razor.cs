using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class ModalDialogItem : HESDomComponentBase
    {
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] ILogger<ModalDialogItem> Logger { get; set; }
        [Parameter] public ModalDialogInstance ModalDialogInstance { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            try
            {
                await JSRuntime.InvokeVoidAsync("showModalDialog", ModalDialogInstance.Id);
            }
            catch (Exception ex)
            {
                Logger.LogError($"JSRuntime - {ex.Message}");
            }
            await InvokeAsync(StateHasChanged);
        }
    }
}