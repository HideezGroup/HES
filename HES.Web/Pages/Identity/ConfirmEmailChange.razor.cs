using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.Identity;
using HES.Web.Components;
using HES.Web.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System;
using System.Text;
using System.Threading.Tasks;

namespace HES.Web.Pages.Identity
{
    public partial class ConfirmEmailChange : HESPageBase
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public IJSRuntime JSRuntime { get; set; }
        [Inject] public UserManager<ApplicationUser> UserManager { get; set; }
        [Inject] public ILogger<ConfirmEmailChange> Logger { get; set; }
        [Inject] public NavigationManager NavigationManager { get; set; }

        public string ErrorTitle { get; set; }
        public string ErrorDescription { get; set; }
        public bool Success { get; set; }

        protected override async Task OnInitializedAsync()
        {
            try
            {
                ApplicationUserService = ScopedServices.GetRequiredService<IApplicationUserService>();

                var userId = NavigationManager.GetQueryValue("userId");
                var code = NavigationManager.GetQueryValue("code");
                var email = NavigationManager.GetQueryValue("email");

                if (userId == null || email == null || code == null)
                {
                    SetWrongParameters(Resources.Resource.Common_WrongCode_Title, Resources.Resource.Common_WrongCode_Description);
                    SetInitialized();
                    return;
                }

                var user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    SetWrongParameters(Resources.Resource.Common_UserNotFound_Title, Resources.Resource.Common_UserNotFound_Description);
                    SetInitialized();
                    return;
                }

                try
                {
                    code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                    await ApplicationUserService.ConfirmEmailChangeAsync(new UserConfirmEmailChangeModel() { UserId = userId, Email = email, Code = code });
                    await JSRuntime.InvokeWebApiPostVoidAsync(Routes.ApiLogout);
                }
                catch (Exception ex)
                {
                    SetWrongParameters(Resources.Resource.Common_ErrorChangingEmail_Title, ex.Message);
                    SetInitialized();
                    return;
                }

                Success = true;
                SetInitialized();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                SetLoadFailed(ex.Message);
            }
        }

        private void SetWrongParameters(string title, string description)
        {
            ErrorTitle = title;
            ErrorDescription = description;
        }
    }
}