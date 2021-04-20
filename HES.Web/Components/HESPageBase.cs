using HES.Core.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Threading.Tasks;

namespace HES.Web.Components
{
    public class HESPageBase : HESBase
    {
        [Inject] protected AuthenticationStateProvider AuthenticationStateProvider { get; set; }
        [Inject] protected IPageSyncService PageSyncService { get; set; }
        [Inject] protected IModalDialogService ModalDialogService { get; set; }
        [Inject] protected IBreadcrumbsService BreadcrumbsService { get; set; }

        protected string PageId { get; private set; }
        protected bool LoadFailed { get; private set; }
        protected string ErrorMessage { get; private set; }

        protected async Task<string> GetCurrentUserEmailAsync()
        {
            return (await AuthenticationStateProvider.GetAuthenticationStateAsync()).User.Identity.Name;
        }

        protected override void SetInitialized()
        {
            PageId = Guid.NewGuid().ToString();
            base.SetInitialized();
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