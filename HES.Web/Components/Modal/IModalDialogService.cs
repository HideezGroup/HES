using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public interface IModalDialogService2
    {
        event Func<string, RenderFragment, ModalDialogSize2, Task<ModalDialogInstance>> OnShow;
        event Func<ModalDialogInstance, Task> OnClose;

        Task<ModalDialogInstance> ShowAsync(string header, RenderFragment body, ModalDialogSize2 modalDialogSize = ModalDialogSize2.Default);
        Task CloseAsync(ModalDialogInstance modalDialogInstance);
    }
}
