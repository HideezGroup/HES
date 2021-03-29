using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public class ModalDialogInstance
    {
        private readonly IModalDialogService2 _modalDialogService;
        private readonly TaskCompletionSource<ModalResult> _resultCompletion;

        public string Id { get; }
        public string Title { get; }
        public RenderFragment Body { get; }
        public string ModalDialogSizeClass { get; }
        public Task<ModalResult> Result => _resultCompletion.Task;

        public ModalDialogInstance(string title, RenderFragment body, ModalDialogSize2 modalDialogSize, IModalDialogService2 modalDialogService)
        {
            _modalDialogService = modalDialogService;
            _resultCompletion = new TaskCompletionSource<ModalResult>();

            Body = body;
            Title = title;
            Id = Guid.NewGuid().ToString();

            switch (modalDialogSize)
            {
                case ModalDialogSize2.Default:
                    ModalDialogSizeClass = "modal-fullscreen-sm-down";
                    break;
                case ModalDialogSize2.Small:
                    ModalDialogSizeClass = "modal-sm modal-fullscreen-sm-down";
                    break;
                case ModalDialogSize2.Large:
                    ModalDialogSizeClass = "modal-lg modal-fullscreen-sm-down";
                    break;
                case ModalDialogSize2.ExtraLarge:
                    ModalDialogSizeClass = "modal-xl modal-fullscreen-sm-down";
                    break;
            }
        }

        public async Task CloseAsync(ModalResult obj)
        {
            await _modalDialogService.CloseAsync(this);
            _resultCompletion.TrySetResult(obj);
        }
    }
}