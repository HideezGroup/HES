using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public class HESModalBase : HESBase
    {
        [CascadingParameter] protected ModalDialogInstance ModalDialogInstance { get; set; }

        protected virtual async Task ModalDialogClose()
        {
            await ModalDialogInstance.CloseAsync(ModalResult.Success);
        }

        protected virtual async Task ModalDialogCancel()
        {
            await ModalDialogInstance.CloseAsync(ModalResult.Cancel);
        }
    }
}