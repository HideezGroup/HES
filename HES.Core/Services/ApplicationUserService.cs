using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Core.Models.AppUsers;
using HES.Core.Models.DataTableComponent;
using HES.Core.Models.Filters;
using HES.Core.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class ApplicationUserService : IApplicationUserService
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IEmailSenderService _emailSenderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        private const string _registerSecurityKeyTokenProvoderName = "RegisterSecurityKey";
        private const string _registerSecurityKeyTokenProvoderPurpose = "RegisterSecurityKeyPurpose";

        public ApplicationUserService(IEmailSenderService emailSenderService,
                                      IApplicationDbContext applicationDbContext,
                                      UserManager<ApplicationUser> userManager,
                                      SignInManager<ApplicationUser> signInManager)
        {
            _dbContext = applicationDbContext;
            _emailSenderService = emailSenderService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new HESException(HESCode.UserNotFound);
            }

            return user;
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                throw new HESException(HESCode.UserNotFound);
            }

            return user;
        }

        #region Administrators

        public async Task<List<ApplicationUser>> GetAdministratorsAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions)
        {
            return await AdministratorsQuery(dataLoadingOptions).Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetAdministratorsCountAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions)
        {
            return await AdministratorsQuery(dataLoadingOptions).CountAsync();
        }

        private IQueryable<ApplicationUser> AdministratorsQuery(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions)
        {
            var query = _dbContext.Users
            .Include(x => x.UserRoles)
            .ThenInclude(x => x.Role)
            .Where(x => x.UserRoles.Any(x => x.Role.Name == ApplicationRoles.Admin));

            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Email.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.PhoneNumber.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            switch (dataLoadingOptions.SortedColumn)
            {
                case nameof(ApplicationUser.Email):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.Email) : query.OrderByDescending(x => x.Email);
                    break;
                case nameof(ApplicationUser.PhoneNumber):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.PhoneNumber) : query.OrderByDescending(x => x.PhoneNumber);
                    break;
                case nameof(ApplicationUser.EmailConfirmed):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.EmailConfirmed) : query.OrderByDescending(x => x.EmailConfirmed);
                    break;
                case nameof(ApplicationUser.TwoFactorEnabled):
                    query = dataLoadingOptions.SortDirection == ListSortDirection.Ascending ? query.OrderBy(x => x.TwoFactorEnabled) : query.OrderByDescending(x => x.TwoFactorEnabled);
                    break;
            }

            return query;
        }

        public async Task<string> InviteAdministratorAsync(string email, string domain)
        {
            var userExist = await _userManager.FindByEmailAsync(email);
            if (userExist != null)
            {
                throw new HESException(HESCode.EmailAlreadyTaken);
            }

            var user = new ApplicationUser { UserName = email, Email = email };
            var password = Guid.NewGuid().ToString();

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new Exception(HESException.GetIdentityResultErrors(result.Errors));
            }

            var roleResult = await _userManager.AddToRoleAsync(user, ApplicationRoles.Admin);

            if (!roleResult.Succeeded)
            {
                throw new Exception(HESException.GetIdentityResultErrors(roleResult.Errors));
            }

            return await GenerateInviteCallBackUrl(email, domain);   
        }

        public async Task<string> GenerateInviteCallBackUrl(string email, string domain)
        {
            var user = await GetUserByEmailAsync(email);

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"{domain.TrimEnd('/')}{Routes.Invite}?code={WebUtility.UrlEncode(code)}&Email={email}";

            return HtmlEncoder.Default.Encode(callbackUrl);
        }

        public async Task<ApplicationUser> DeleteUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentNullException(nameof(userId));
            }

            var user = await GetUserByIdAsync(userId);
            
            await _userManager.DeleteAsync(user);
            return user;
        }

        public async Task<IList<ApplicationUser>> GetAllAdministratorsAsync()
        {
            return await _userManager.GetUsersInRoleAsync(ApplicationRoles.Admin);
        }

        #endregion

        #region Users

        public async Task<string> GenerateEnableSsoCallBackUrlAsync(string email, string domain)
        {
            var user = await GetUserByEmailAsync(email);

            var code = await _userManager.GenerateUserTokenAsync(user, _registerSecurityKeyTokenProvoderName, _registerSecurityKeyTokenProvoderPurpose);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl = $"{domain.TrimEnd('/')}{Routes.RegisterSecurityKey}?code={code}&email={email}";

            return HtmlEncoder.Default.Encode(callbackUrl);
        }

        public async Task<bool> VerifyRegisterSecurityKeyTokenAsync(ApplicationUser user, string code)
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            return await _userManager.VerifyUserTokenAsync(user, _registerSecurityKeyTokenProvoderName, _registerSecurityKeyTokenProvoderPurpose, code);
        }

        #endregion

        #region Profile

        public async Task UpdateProfileInfoAsync(UserProfileModel parameters)
        {
            var user = await GetUserByIdAsync(parameters.UserId);

            if (parameters.FullName != user.FullName)
            {
                user.FullName = parameters.FullName;
                var userResult = await _userManager.UpdateAsync(user);

                if (!userResult.Succeeded)
                {
                    throw new Exception(HESException.GetIdentityResultErrors(userResult.Errors));
                }
            }

            if (parameters.PhoneNumber != user.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, parameters.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                {
                    throw new Exception(HESException.GetIdentityResultErrors(setPhoneResult.Errors));
                }
            }
        }

        public async Task ChangeEmailAsync(ChangeEmailModel parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var user = await GetUserByEmailAsync(parameters.CurrentEmail);

            var exist = await GetUserByEmailAsync(parameters.NewEmail);
            if (exist != null)
            {
                throw new HESException(HESCode.EmailAlreadyTaken);
            }

            var code = await _userManager.GenerateChangeEmailTokenAsync(user, parameters.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            await _emailSenderService.SendUserConfirmEmailAsync(user.Id, parameters.NewEmail, code);
        }

        public async Task ConfirmEmailChangeAsync(UserConfirmEmailChangeModel parameters)
        {
            var user = await GetUserByIdAsync(parameters.UserId);

            using (var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update FIDO credentials
                var credentials = await _dbContext.FidoStoredCredential.Where(x => x.Username == user.Email).ToListAsync();
                foreach (var item in credentials)
                {
                    item.UserId = Encoding.UTF8.GetBytes(parameters.Email);
                    item.UserHandle = Encoding.UTF8.GetBytes(parameters.Email);
                    item.Username = parameters.Email;
                }
                _dbContext.FidoStoredCredential.UpdateRange(credentials);
                await _dbContext.SaveChangesAsync();

                // Change email
                var changeEmailResult = await _userManager.ChangeEmailAsync(user, parameters.Email, parameters.Code);
                if (!changeEmailResult.Succeeded)
                {
                    throw new Exception(HESException.GetIdentityResultErrors(changeEmailResult.Errors));
                }

                // In our UI email and user name are one and the same, so when we update the email we need to update the user name.
                var setUserNameResult = await _userManager.SetUserNameAsync(user, parameters.Email);
                if (!setUserNameResult.Succeeded)
                {
                    throw new Exception(HESException.GetIdentityResultErrors(setUserNameResult.Errors));
                }

                transactionScope.Complete();
            }
        }

        public async Task UpdateAccountPasswordAsync(ChangePasswordModel parameters)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            var user = await GetUserByIdAsync(parameters.UserId);

            var isValidPassword = await _userManager.CheckPasswordAsync(user, parameters.OldPassword);
            if (!isValidPassword)
            {
                throw new HESException(HESCode.IncorrectCurrentPassword);
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, parameters.OldPassword, parameters.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                throw new Exception(HESException.GetIdentityResultErrors(changePasswordResult.Errors));
            }
        }

        public async Task<TwoFactorInfo> GetTwoFactorInfoAsync(HttpClient httpClient)
        {
            var response = await httpClient.GetAsync("api/Identity/GetTwoFactorInfo");

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }

            return JsonConvert.DeserializeObject<TwoFactorInfo>(await response.Content.ReadAsStringAsync());
        }

        public async Task ForgetBrowserAsync(HttpClient httpClient)
        {
            var response = await httpClient.PostAsync("api/Identity/ForgetTwoFactorClient", new StringContent(string.Empty));

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(await response.Content.ReadAsStringAsync());
            }
        }

        #endregion

        #region Email

        public async Task SendLicenseChangedAsync(DateTime createdAt, LicenseOrderStatus status)
        {
            var administrators = await GetAllAdministratorsAsync();
            await _emailSenderService.SendLicenseChangedAsync(createdAt, status, administrators);
        }

        public async Task SendHardwareVaultLicenseStatus(List<HardwareVault> vaults)
        {
            var administrators = await GetAllAdministratorsAsync();
            await _emailSenderService.SendHardwareVaultLicenseStatus(vaults, administrators);
        }

        public async Task SendActivateDataProtectionAsync()
        {
            var administrators = await GetAllAdministratorsAsync();
            await _emailSenderService.SendActivateDataProtectionAsync(administrators);
        }

        #endregion

        #region API

        // Only API call
        public async Task<AuthorizationResponse> LoginWithPasswordAsync(PasswordSignInModel parameters)
        {
            try
            {
                var user = await GetUserByEmailAsync(parameters.Email);

                var result = await _signInManager.PasswordSignInAsync(parameters.Email, parameters.Password, parameters.RememberMe, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    return AuthorizationResponse.Success(user);
                }

                if (result.RequiresTwoFactor)
                {
                    return AuthorizationResponse.TwoFactorRequired(user);
                }

                if (result.IsLockedOut)
                {
                    return AuthorizationResponse.LockedOut(user);
                }

                return AuthorizationResponse.Error(HESCode.InvalidLoginAttempt);
            }
            catch (HESException ex)
            {
                return AuthorizationResponse.Error(ex.Code);
            }
            catch (Exception ex)
            {
                return AuthorizationResponse.Error(ex.Message);
            }
        }

        #endregion
    }
}