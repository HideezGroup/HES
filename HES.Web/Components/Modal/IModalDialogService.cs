using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public interface IModalDialogService
    {
        event Func<string, RenderFragment, ModalDialogSize, Task<ModalDialogInstance>> OnShow;
        event Func<ModalDialogInstance, Task> OnClose;

        Task<ModalDialogInstance> ShowAsync(string header, RenderFragment body, ModalDialogSize modalDialogSize = ModalDialogSize.Default);
        Task CloseAsync(ModalDialogInstance modalDialogInstance);
    }
}
