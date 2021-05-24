﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Models.API;
using HES.Core.Models.AppUsers;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Filters;
using HES.Core.Models.Identity;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IApplicationUserService
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetUserByEmailAsync(string email);

        #region Administrators
        Task<List<ApplicationUser>> GetAdministratorsAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions);
        Task<int> GetAdministratorsCountAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions);
        Task<string> InviteAdministratorAsync(string email, string domain);
        Task<string> GenerateInviteCallBackUrl(string email, string domain);
        Task<ApplicationUser> DeleteUserAsync(string id);
        Task<IList<ApplicationUser>> GetAllAdministratorsAsync();
        #endregion

        #region Users
        Task<string> GenerateEnableSsoCallBackUrlAsync(string email, string domain);
        Task<bool> VerifyRegisterSecurityKeyTokenAsync(ApplicationUser user, string code);
        #endregion

        #region Profile
        Task UpdateProfileInfoAsync(UserProfileModel parameters);
        Task ChangeEmailAsync(UserChangeEmailModel parameters);
        Task ConfirmEmailChangeAsync(UserConfirmEmailChangeModel parameters);
        Task UpdateAccountPasswordAsync(UserChangePasswordModel parameters);
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