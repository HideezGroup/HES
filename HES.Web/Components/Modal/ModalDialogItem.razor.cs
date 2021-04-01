using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class ModalDialogItem : HESDomComponentBase
    {
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Parameter] public ModalDialogInstance ModalDialogInstance { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            await JSRuntime.InvokeVoidAsync("showModalDialog", ModalDialogInstance.Id);
            await InvokeAsync(StateHasChanged);
        }
    }
}