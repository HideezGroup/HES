using System;
using HES.Core.Enums;
using HES.Core.Interfaces;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Components
{
    public class ToastService : IToastService
    {
        public event Func<ToastType, RenderFragment, string, Task> OnShow;

        public async Task ShowToastAsync(string message, ToastType toastType, string header = "")
        {
            await OnShow?.Invoke(toastType, builder => builder.AddContent(0, message), header);
        }
    }
}