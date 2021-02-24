using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public class HESComponentBase : OwningComponentBase
    {
        [CascadingParameter] protected Task<AuthenticationState> AuthenticationStateTask { get; set; }
        [Inject] protected ISynchronizationService SynchronizationService { get; set; }
        [Inject] protected NavigationManager NavigationManager { get; set; }

        public bool Initialized { get; private set; }
        public string PageId { get; private set; }
        public bool LoadFailed { get; private set; }
        public string ErrorMessage { get; private set; }

        protected void SetInitialized()
        {
            PageId = Guid.NewGuid().ToString();
            Initialized = true;
        }

        protected void SetLoadFailed(string message)
        {
            ErrorMessage = message;
            LoadFailed = true;
        }

        protected void SetErrorMessage(string message)
        {
            ErrorMessage = message;
            StateHasChanged();
        }

        protected void ClearErrorMessage()
        {
            ErrorMessage = string.Empty;
            StateHasChanged();
        }
    }
}