using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public class ModalDialogInstance
    {
        private readonly IModalDialogService _modalDialogService;
        private readonly TaskCompletionSource<ModalResult> _resultCompletion;

        public string Id { get; }
        public string Title { get; }
        public RenderFragment Body { get; }
        public string ModalDialogSizeClass { get; }
        public Task<ModalResult> Result => _resultCompletion.Task;

        public ModalDialogInstance(string title, RenderFragment body, ModalDialogSize modalDialogSize, IModalDialogService modalDialogService)
        {
            _modalDialogService = modalDialogService;
            _resultCompletion = new TaskCompletionSource<ModalResult>();

            Body = body;
            Title = title;
            Id = Guid.NewGuid().ToString();

            switch (modalDialogSize)
            {
                case ModalDialogSize.Default:
                    ModalDialogSizeClass = "modal-fullscreen-sm-down";
                    break;
                case ModalDialogSize.Small:
                    ModalDialogSizeClass = "modal-sm modal-fullscreen-sm-down";
                    break;
                case ModalDialogSize.Large:
                    ModalDialogSizeClass = "modal-lg modal-fullscreen-sm-down";
                    break;
                case ModalDialogSize.ExtraLarge:
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