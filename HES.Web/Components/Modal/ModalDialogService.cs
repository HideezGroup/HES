﻿using Microsoft.AspNetCore.Components;
using System;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public class ModalDialogService2 : IModalDialogService2
    {
        public event Func<string, RenderFragment, ModalDialogSize2, Task<ModalDialogInstance>> OnShow;
        public event Func<ModalDialogInstance, Task> OnClose;

        public async Task<ModalDialogInstance> ShowAsync(string header, RenderFragment body, ModalDialogSize2 modalDialogSize = ModalDialogSize2.Default)
        {
            return await OnShow?.Invoke(header, body, modalDialogSize);
        }

        public async Task CloseAsync(ModalDialogInstance modalDialogInstance)
        {
            await OnClose?.Invoke(modalDialogInstance);
        }
    }
}
