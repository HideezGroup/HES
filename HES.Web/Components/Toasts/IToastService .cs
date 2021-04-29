using System;
using HES.Core.Enums;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Components
{
    public interface IToastService
    {
        public event Func<ToastType, RenderFragment, string, Task> OnShow;
        Task ShowToastAsync(string message, ToastType toastType, string header = "");
    }
}