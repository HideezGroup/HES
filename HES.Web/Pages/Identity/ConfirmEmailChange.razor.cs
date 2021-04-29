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
using System;
using System.Text;
using System.Threading.Tasks;

namespace HES.Web.Pages.Identity
{
    public partial class ConfirmEmailChange : HESPageBase
    {
        public IApplicationUserService ApplicationUserService { get; set; }
        [Inject] public IIdentityApiClient IdentityApiClient { get; set; }
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
                    SetWrongParameters("Wrong code", "This code is not valid.");
                    SetInitialized();
                    return;
                }

                var user = await UserManager.FindByIdAsync(userId);
                if (user == null)
                {
                    SetWrongParameters("User not found", "Unable to load user.");
                    SetInitialized();
                    return;
                }

                code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
                try
                {
                    await ApplicationUserService.ConfirmEmailChangeAsync(new UserConfirmEmailChangeModel() { UserId = userId, Email = email, Code = code });
                    await IdentityApiClient.LogoutAsync();
                }
                catch (Exception ex)
                {
                    SetWrongParameters("Error changing email", ex.Message);
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