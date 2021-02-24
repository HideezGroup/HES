using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models.API;
using HES.Core.Models.Web;
using HES.Core.Models.Web.AppUsers;
using HES.Core.Models.Web.Identity;
using HES.Core.Models.Web.Users;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IApplicationUserService : IDisposable
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetUserByEmailAsync(string email);

        #region Administrators
        Task<List<ApplicationUser>> GetAdministratorsAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions);
        Task<int> GetAdministratorsCountAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions);
        Task<string> InviteAdministratorAsync(string email, string domain);
        Task<string> GetCallBackUrl(string email, string domain);
        Task<ApplicationUser> DeleteUserAsync(string id);
        Task<IList<ApplicationUser>> GetAllAdministratorsAsync();
        #endregion

        #region Profile
        Task UpdateProfileInfoAsync(UserProfileModel parameters);
        Task ChangeEmailAsync(ChangeEmailModel parameters);
        Task ConfirmEmailChangeAsync(UserConfirmEmailChangeModel parameters);
        Task UpdateAccountPasswordAsync(ChangePasswordModel parameters);
        Task<TwoFactorInfo> GetTwoFactorInfoAsync(HttpClient httpClient);
        Task ForgetBrowserAsync(HttpClient httpClient);

        #endregion

        #region Email
        Task SendLicenseChangedAsync(DateTime createdAt, LicenseOrderStatus status);
        Task SendHardwareVaultLicenseStatus(List<HardwareVault> vaults);
        Task SendActivateDataProtectionAsync();
        #endregion

        #region API
        Task<AuthorizationResponse> LoginWithPasswordAsync(PasswordSignInModel parameters);
        #endregion
    }
}