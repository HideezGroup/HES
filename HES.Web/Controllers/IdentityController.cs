using HES.Core.Entities;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Core.Models.API.Identity;
using HES.Core.Models.AppUsers;
using HES.Core.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class IdentityController : ControllerBase
    {
        private readonly IApplicationUserService _applicationUserService;
        private readonly IFido2Service _fido2Service;
        private readonly UrlEncoder _urlEncoder;
        private readonly ILogger<IdentityController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IdentityController(IApplicationUserService applicationUserService,
                                  IFido2Service fido2Service,
                                  UrlEncoder urlEncoder,
                                  ILogger<IdentityController> logger,
                                  UserManager<ApplicationUser> userManager,
                                  SignInManager<ApplicationUser> signInManager)
        {
            _logger = logger;
            _applicationUserService = applicationUserService;
            _fido2Service = fido2Service;
            _urlEncoder = urlEncoder;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApplicationUser>> GetUser()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                    throw new Exception("User is null");

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region 2FA

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TwoFactorInfo>> GetTwoFactorInfo()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                    throw new Exception("User is null");

                var twoFactorInfo = new TwoFactorInfo
                {
                    HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                    Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                    IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                    RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
                };

                return Ok(twoFactorInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgetTwoFactorClient()
        {
            try
            {
                await _signInManager.ForgetTwoFactorClientAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SharedKeyInfo>> LoadSharedKeyAndQrCodeUri()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                    throw new Exception("User is null");

                var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                if (string.IsNullOrWhiteSpace(unformattedKey))
                {
                    await _userManager.ResetAuthenticatorKeyAsync(user);
                    unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
                }

                var sharedKeyInfo = new SharedKeyInfo
                {
                    SharedKey = FormatKey(unformattedKey),
                    AuthenticatorUri = GenerateQrCodeUri(user.Email, unformattedKey)
                };

                return Ok(sharedKeyInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetAuthenticatorKey()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                    throw new Exception("User is null");

                await _userManager.SetTwoFactorEnabledAsync(user, false);
                await _userManager.ResetAuthenticatorKeyAsync(user);

                _logger.LogInformation($"User '{user.Id}' has reset their authentication app key.");

                await _signInManager.RefreshSignInAsync(user);

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<VerifyTwoFactorInfo>> VerifyTwoFactor(VerificationCode verificationCode)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                    throw new Exception("User is null");

                var code = verificationCode.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
                var isTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, code);

                var verifyTwoFactorInfo = new VerifyTwoFactorInfo { IsTwoFactorTokenValid = isTokenValid };

                if (!isTokenValid)
                    return Ok(verifyTwoFactorInfo);

                await _userManager.SetTwoFactorEnabledAsync(user, true);

                if (await _userManager.CountRecoveryCodesAsync(user) == 0)
                {
                    var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                    verifyTwoFactorInfo.RecoveryCodes = recoveryCodes.ToList();

                    return Ok(verifyTwoFactorInfo);
                }

                return Ok(verifyTwoFactorInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DisableTwoFactor()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                    throw new Exception("User is null");

                var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
                if (!disable2faResult.Succeeded)
                    throw new InvalidOperationException($"Unexpected error occurred disabling 2FA for user '{user.Id}'.");

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<string>>> GenerateNewTwoFactorRecoveryCodes()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                    throw new Exception("User is null");

                var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                if (!isTwoFactorEnabled)
                    throw new InvalidOperationException($"Cannot generate recovery codes for user '{user.Id}' as they do not have 2FA enabled.");

                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

                _logger.LogInformation($"User '{user.Id}' has generated new 2FA recovery codes.");

                return Ok(recoveryCodes.ToList());
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

            return string.Format(
                AuthenticatorUriFormat,
                _urlEncoder.Encode("HES.Web"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }

        #endregion

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<HttpResponseMessage> DownloadPersonalData()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                    throw new Exception("User is null");

                _logger.LogInformation($"User '{user.Id}' asked for their personal data.");

                var personalData = new Dictionary<string, string>();
                var personalDataProps = typeof(ApplicationUser).GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(PersonalDataAttribute)));

                foreach (var prop in personalDataProps)
                    personalData.Add(prop.Name, prop.GetValue(user)?.ToString() ?? "null");


                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(personalData)))
                };
                result.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = "PersonalData.json"
                    };
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/json");

                return result;
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(ex.Message)
                };
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeletePersonalData(RequiredPassword requiredPassword)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                if (user == null)
                    throw new Exception("User is null");

                var requirePassword = await _userManager.HasPasswordAsync(user);
                if (requirePassword)
                    if (!await _userManager.CheckPasswordAsync(user, requiredPassword.Password))
                        throw new Exception("Password not correct.");

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                    throw new InvalidOperationException($"Unexpected error occurred deleteing user '{user.Id}'.");

                await _signInManager.SignOutAsync();

                _logger.LogInformation($"User '{user.Id}' deleted themselves.");

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region Authorization

        [HttpPost]
        [AllowAnonymous]
        public async Task<AuthorizationResponse> LoginWithPassword(PasswordSignInModel parameters)
        {
            return await _applicationUserService.LoginWithPasswordAsync(parameters);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<AuthorizationResponse> LoginWithFido2(SecurityKeySignInModel parameters)
        {
            return await _fido2Service.SignInAsync(parameters);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<AuthorizationResponse> Logout()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    await _signInManager.SignOutAsync();
                }
                return AuthorizationResponse.Success(null);
            }
            catch (Exception ex)
            {
                return AuthorizationResponse.Error(ex.Message);
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshSignIn()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                await _signInManager.RefreshSignInAsync(user);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (loginDto == null)
            {
                _logger.LogWarning($"{nameof(loginDto)} is null");
                return BadRequest(new { error = "CredentialsNull" });
            }

            // Find user
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning($"User {loginDto.Email} not found");
                return Unauthorized(new { error = "Unauthorized" });
            }

            // Verify password
            var passwordResult = await _signInManager.PasswordSignInAsync(loginDto.Email, loginDto.Password, false, lockoutOnFailure: true);
            if (passwordResult.Succeeded)
            {
                return Ok();
            }

            // Verify two factor
            if (passwordResult.RequiresTwoFactor)
            {
                if (string.IsNullOrWhiteSpace(loginDto.Otp))
                {
                    return Unauthorized(new { error = "TwoFactorRequired" });
                }

                var authenticatorCode = loginDto.Otp.Replace(" ", string.Empty).Replace("-", string.Empty);

                var twoFactorResult = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, false, false);

                if (twoFactorResult.Succeeded)
                {
                    return Ok();
                }
                else if (twoFactorResult.IsLockedOut)
                {
                    _logger.LogWarning($"User {user.Email} account locked out.");
                    return Unauthorized(new { error = "UserIsLockedout" });
                }
                else
                {
                    _logger.LogWarning($"Invalid authenticator code entered for user {user.Email}.");
                    return Unauthorized(new { error = "InvalidAuthenticatorCode" });
                }
            }

            // Is locked out
            if (passwordResult.IsLockedOut)
            {
                _logger.LogWarning($"User account {user.Email} locked out.");
                return Unauthorized(new { error = "UserIsLockedout" });
            }
            else
            {
                _logger.LogError($"User {user.Email} unauthorized.");
                return Unauthorized(new { error = "Unauthorized" });
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> AuthN(LoginDto loginDto)
        {
            if (loginDto == null)
            {
                _logger.LogWarning($"{nameof(loginDto)} is null");
                return BadRequest(new { error = "CredentialsNullException" });
            }

            // Find user
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                _logger.LogWarning($"User {loginDto.Email} not found");
                return Unauthorized(new { error = "UserNotFoundException" });
            }

            // Verify password
            var passwordResult = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!passwordResult)
            {
                _logger.LogWarning($"User {user.Email} verify password failed.");
                return Unauthorized(new { error = "UnauthorizedException" });
            }

            // Verify two factor
            if (user.TwoFactorEnabled)
            {
                if (string.IsNullOrWhiteSpace(loginDto.Otp))
                {
                    return Unauthorized(new { error = "TwoFactorRequiredException" });
                }

                var authenticatorCode = loginDto.Otp.Replace(" ", string.Empty).Replace("-", string.Empty);

                var tokenResult = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, authenticatorCode);
                if (!tokenResult)
                {
                    _logger.LogWarning($"User {loginDto.Email} verify 2fa failed.");
                    return Unauthorized(new { error = "InvalidAuthenticatorCodeException" });
                }
            }

            return Ok(new UserDto()
            {
                FirstName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            });
        }
    }
}