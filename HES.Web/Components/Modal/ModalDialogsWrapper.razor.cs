using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public partial class ModalDialogsWrapper : HESDomComponentBase, IDisposable
    {
        [Inject] IJSRuntime JSRuntime { get; set; }
        [Inject] private IModalDialogService ModalDialogService { get; set; }

        private List<ModalDialogInstance> ModalDialogItems { get; set; }

        protected override void OnInitialized()
        {
            ModalDialogItems = new List<ModalDialogInstance>();
            ModalDialogService.OnShow += OpenDialogAsync;
            ModalDialogService.OnClose += CloseDialogAsync;
        }

        public async Task CloseDialogAsync(ModalDialogInstance modalDialogInstance)
        {
            await InvokeAsync(async () =>
            {
                await JSRuntime.InvokeVoidAsync("hideModalDialog", modalDialogInstance.Id);
                ModalDialogItems.Remove(modalDialogInstance);
                StateHasChanged();
            });
        }

        public async Task<ModalDialogInstance> OpenDialogAsync(string title, RenderFragment body, ModalDialogSize modalDialogSize)
        {
            var modalDialogInstance = new ModalDialogInstance(title, body, modalDialogSize, ModalDialogService);

            await InvokeAsync(() =>
            {
                ModalDialogItems.Add(modalDialogInstance);
                StateHasChanged();
            });

            return modalDialogInstance;
        }

        public void Dispose()
        {
            ModalDialogService.OnShow -= OpenDialogAsync;
            ModalDialogService.OnClose -= CloseDialogAsync;
        }
    }
}