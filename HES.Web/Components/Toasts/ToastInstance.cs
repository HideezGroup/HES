using System;
using HES.Core.Enums;
using Microsoft.AspNetCore.Components;

namespace HES.Web.Components
{
    public class ToastInstance
    {
        public string Id { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public ToastType ToastType { get; private set; }

        public string Header { get; set; }
        public RenderFragment Message { get; set; }

        public ToastInstance(ToastType toastType, RenderFragment message, string header = "")
        {
            Id = Guid.NewGuid().ToString();
            TimeStamp = DateTime.Now;
            Message = message;
            ToastType = toastType;

            switch (toastType)
            {
                case ToastType.Success:
                    Header = string.IsNullOrWhiteSpace(header) ? "Success" : header;
                    break;
                case ToastType.Notify:
                    Header = string.IsNullOrWhiteSpace(header) ? "Notification" : header;
                    break;
                case ToastType.Error:
                    Header = string.IsNullOrWhiteSpace(header) ? "Error" : header;
                    break;
                default:
                    break;
            }
        }
    }
}
