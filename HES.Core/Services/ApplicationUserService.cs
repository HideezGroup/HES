using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Identity;
using HES.Core.Models.Web.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class ApplicationUserService : IApplicationUserService, IDisposable
    {
        private readonly IAsyncRepository<ApplicationUser> _applicationUserRepository;
        private readonly IAsyncRepository<FidoStoredCredential> _fidoCredentialsRepository;
        private readonly IEmailSenderService _emailSenderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ApplicationUserService(IAsyncRepository<ApplicationUser> applicationUserRepository,
                                      IAsyncRepository<FidoStoredCredential> fidoCredentialsRepository,
                                      IEmailSenderService emailSenderService,
                                      UserManager<ApplicationUser> userManager,
                                      SignInManager<ApplicationUser> signInManager)
        {
            _applicationUserRepository = applicationUserRepository;
            _fidoCredentialsRepository = fidoCredentialsRepository;
            _emailSenderService = emailSenderService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<ApplicationUser> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        #region Administrators

        public async Task<List<ApplicationUser>> GetAdministratorsAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions)
        {
            var query = _applicationUserRepository.Query();

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

            return await query.Skip(dataLoadingOptions.Skip).Take(dataLoadingOptions.Take).AsNoTracking().ToListAsync();
        }

        public async Task<int> GetAdministratorsCountAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions)
        {
            var query = _applicationUserRepository.Query();

            if (!string.IsNullOrWhiteSpace(dataLoadingOptions.SearchText))
            {
                dataLoadingOptions.SearchText = dataLoadingOptions.SearchText.Trim();

                query = query.Where(x => x.Email.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase) ||
                                    x.PhoneNumber.Contains(dataLoadingOptions.SearchText, StringComparison.OrdinalIgnoreCase));
            }

            return await query.CountAsync();
        }

        public async Task<string> InviteAdministratorAsync(string email, string domain)
        {
            var userExist = await _userManager.FindByEmailAsync(email);

            if (userExist != null)
                throw new AlreadyExistException($"User {email} already exists");

            var user = new ApplicationUser { UserName = email, Email = email };
            var password = Guid.NewGuid().ToString();

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                string errors = string.Empty;
                foreach (var item in result.Errors)
                    errors += $"Code: {item.Code} Description: {item.Description} {Environment.NewLine}";

                throw new Exception(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Administrator");

            if (!roleResult.Succeeded)
            {
                string errors = string.Empty;
                foreach (var item in result.Errors)
                    errors += $"Code: {item.Code} Description: {item.Description} {Environment.NewLine}";

                throw new Exception(errors);
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"{domain}Identity/Account/Invite?code={WebUtility.UrlEncode(code)}&Email={email}";

            return HtmlEncoder.Default.Encode(callbackUrl);
        }

        public async Task<string> GetCallBackUrl(string email, string domain)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new Exception($"User not found.");

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = $"{domain}Identity/Account/Invite?code={WebUtility.UrlEncode(code)}&Email={email}";

            return HtmlEncoder.Default.Encode(callbackUrl);
        }

        public async Task<ApplicationUser> DeleteUserAsync(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                throw new HESException(HESCode.UserNotFound);

            await _userManager.DeleteAsync(user);

            return user;
        }

        public async Task<IList<ApplicationUser>> GetAllAdministratorsAsync()
        {
            return await _userManager.GetUsersInRoleAsync("Administrator");
        }

        #endregion

        #region Profile

        public async Task UpdateProfileInfoAsync(UserProfileModel parameters)
        {
            var user = await _userManager.FindByIdAsync(parameters.UserId);
            if (user == null)
                throw new HESException(HESCode.UserNotFound);

            if (parameters.FullName != user.FirstName)
            {
                user.FirstName = parameters.FullName;
                var userResult = await _userManager.UpdateAsync(user);

                if (!userResult.Succeeded)
                    throw new Exception(HESException.GetIdentityResultErrors(userResult.Errors));
            }

            if (parameters.PhoneNumber != user.PhoneNumber)
            {
                var setPhoneResult = await _userManager.SetPhoneNumberAsync(user, parameters.PhoneNumber);
                if (!setPhoneResult.Succeeded)
                    throw new Exception(HESException.GetIdentityResultErrors(setPhoneResult.Errors));
            }
        }

        public async Task ChangeEmailAsync(ChangeEmailModel parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var user = await GetUserByEmailAsync(parameters.CurrentEmail);
            if (user == null)
                throw new HESException(HESCode.UserNotFound);

            var code = await _userManager.GenerateChangeEmailTokenAsync(user, parameters.NewEmail);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            await _emailSenderService.SendUserConfirmEmailAsync(user.Id, parameters.NewEmail, code);
        }

        public async Task ConfirmEmailChangeAsync(UserConfirmEmailChangeModel parameters)
        {
            var user = await _userManager.FindByIdAsync(parameters.UserId);
            if (user == null)
                throw new HESException(HESCode.UserNotFound);

            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                // Update FIDO credentials
                var credentials = await _fidoCredentialsRepository.Query().Where(x => x.Username == user.Email).ToListAsync();
                foreach (var item in credentials)
                {
                    item.UserId = Encoding.UTF8.GetBytes(parameters.Email);
                    item.UserHandle = Encoding.UTF8.GetBytes(parameters.Email);
                    item.Username = parameters.Email;
                }
                await _fidoCredentialsRepository.UpdatRangeAsync(credentials);

                // Change email
                var changeEmailResult = await _userManager.ChangeEmailAsync(user, parameters.Email, parameters.Code);
                if (!changeEmailResult.Succeeded)
                    throw new Exception(HESException.GetIdentityResultErrors(changeEmailResult.Errors));

                // In our UI email and user name are one and the same, so when we update the email we need to update the user name.
                var setUserNameResult = await _userManager.SetUserNameAsync(user, parameters.Email);
                if (!setUserNameResult.Succeeded)
                    throw new Exception(HESException.GetIdentityResultErrors(setUserNameResult.Errors));

                transactionScope.Complete();
            }
        }

        public async Task UpdateAccountPasswordAsync(ChangePasswordModel parameters)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            var user = await _userManager.FindByIdAsync(parameters.UserId);
            if (user == null)
                throw new Exception(HESException.GetMessage(HESCode.UserNotFound));

            var isValidPassword = await _userManager.CheckPasswordAsync(user, parameters.OldPassword);
            if (!isValidPassword)
                throw new Exception(HESException.GetMessage(HESCode.IncorrectCurrentPassword));

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, parameters.OldPassword, parameters.NewPassword);
            if (!changePasswordResult.Succeeded)
                throw new Exception(HESException.GetIdentityResultErrors(changePasswordResult.Errors));
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

        public void Dispose()
        {
            _applicationUserRepository.Dispose();
            _userManager.Dispose();
        }
    }
}