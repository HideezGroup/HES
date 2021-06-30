using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Interfaces;
using ITfoxtec.Identity.Saml2;
using ITfoxtec.Identity.Saml2.MvcCore;
using ITfoxtec.Identity.Saml2.Schemas;
using ITfoxtec.Identity.Saml2.Schemas.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens.Saml2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [AllowAnonymous]
    [Route("Saml")]
    public class SamlController : Controller
    {

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly Saml2Configuration _saml2Configuration;
        private readonly IAppSettingsService _appSettingsService;
        private readonly ILogger<SamlController> _logger;

        public SamlController(SignInManager<ApplicationUser> signInManager, IAppSettingsService appSettingsService, IOptions<Saml2Configuration> saml2Configuration, ILogger<SamlController> logger)
        {
            _signInManager = signInManager;
            _saml2Configuration = saml2Configuration.Value;
            _appSettingsService = appSettingsService;
            _logger = logger;
        }

        [HttpGet("Login")]
        public async Task<IActionResult> Login()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return LocalRedirect(Routes.SingleSignOn + Request.QueryString);
            }

            Saml2RedirectBinding requestBinding;
            SamlRelyingParty relyingParty;
            Saml2AuthnRequest saml2AuthnRequest;

            try
            {
                requestBinding = new Saml2RedirectBinding();
                relyingParty = await ValidateRelyingParty(ReadRelyingPartyFromLoginRequest(requestBinding));
                saml2AuthnRequest = new Saml2AuthnRequest(_saml2Configuration);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }

            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), saml2AuthnRequest);

                var sessionIndex = Guid.NewGuid().ToString();
                var user = await _signInManager.UserManager.FindByNameAsync(User.Identity.Name);

                return LoginResponse(saml2AuthnRequest.Id, Saml2StatusCodes.Success, requestBinding.RelayState, relyingParty, sessionIndex, GetUserClaims(user));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return LoginResponse(saml2AuthnRequest.Id, Saml2StatusCodes.Responder, requestBinding.RelayState, relyingParty);
            }
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            Saml2PostBinding requestBinding;
            SamlRelyingParty relyingParty;
            Saml2LogoutRequest saml2LogoutRequest;

            try
            {
                requestBinding = new Saml2PostBinding();
                relyingParty = await ValidateRelyingParty(ReadRelyingPartyFromLogoutRequest(requestBinding));
                saml2LogoutRequest = new Saml2LogoutRequest(_saml2Configuration)
                {
                    SignatureValidationCertificates = new X509Certificate2[] { relyingParty.SignatureValidationCertificate }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }

            try
            {
                requestBinding.Unbind(Request.ToGenericHttpRequest(), saml2LogoutRequest);

                await _signInManager.SignOutAsync();

                return LogoutResponse(saml2LogoutRequest.Id, Saml2StatusCodes.Success, requestBinding.RelayState, saml2LogoutRequest.SessionIndex, relyingParty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return LogoutResponse(saml2LogoutRequest.Id, Saml2StatusCodes.Responder, requestBinding.RelayState, saml2LogoutRequest.SessionIndex, relyingParty);
            }
        }

        [HttpGet("Metadata")]
        public IActionResult Metadata(bool download)
        {
            try
            {
                var entityDescriptor = new EntityDescriptor(_saml2Configuration)
                {
                    ValidUntil = 365,
                    IdPSsoDescriptor = new IdPSsoDescriptor
                    {
                        SigningCertificates = new X509Certificate2[]
                    {
                    _saml2Configuration.SigningCertificate
                    },
                        SingleSignOnServices = new SingleSignOnService[]
                    {
                    new SingleSignOnService { Binding = ProtocolBindings.HttpRedirect, Location = _saml2Configuration.SingleSignOnDestination }
                    },
                        SingleLogoutServices = new SingleLogoutService[]
                    {
                    new SingleLogoutService { Binding = ProtocolBindings.HttpPost, Location = _saml2Configuration.SingleLogoutDestination }
                    },
                        NameIDFormats = new Uri[] { NameIdentifierFormats.X509SubjectName },
                    }
                };

                if (download)
                {
                    Response.Headers.Add("Content-Disposition", $"attachment; filename=metadata.xml");
                    return File(Encoding.ASCII.GetBytes(new Saml2Metadata(entityDescriptor).CreateMetadata().ToXml()), "application/octet-stream");
                }
                else
                {
                    return new Saml2Metadata(entityDescriptor).CreateMetadata().ToActionResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("Cert")]
        public IActionResult Cert()
        {
            try
            {
                var builder = new StringBuilder();

                builder.AppendLine("-----BEGIN CERTIFICATE-----");
                builder.AppendLine(Convert.ToBase64String(_saml2Configuration.SigningCertificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
                builder.AppendLine("-----END CERTIFICATE-----");

                Response.Headers.Add("Content-Disposition", $"attachment; filename=signing.cer");
                return File(Encoding.ASCII.GetBytes(builder.ToString()), "application/octet-stream");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string ReadRelyingPartyFromLoginRequest<T>(Saml2Binding<T> binding)
        {
            return binding.ReadSamlRequest(Request.ToGenericHttpRequest(), new Saml2AuthnRequest(_saml2Configuration))?.Issuer;
        }

        private string ReadRelyingPartyFromLogoutRequest<T>(Saml2Binding<T> binding)
        {
            return binding.ReadSamlRequest(Request.ToGenericHttpRequest(), new Saml2LogoutRequest(_saml2Configuration))?.Issuer;
        }

        private IActionResult LoginResponse(Saml2Id inResponseTo, Saml2StatusCodes status, string relayState, SamlRelyingParty relyingParty, string sessionIndex = null, IEnumerable<Claim> claims = null)
        {
            var responsebinding = new Saml2PostBinding
            {
                RelayState = relayState
            };

            var saml2AuthnResponse = new Saml2AuthnResponse(_saml2Configuration)
            {
                InResponseTo = inResponseTo,
                Status = status,
                Destination = new Uri(relyingParty.SingleSignOnDestination)
            };

            if (status == Saml2StatusCodes.Success && claims != null)
            {
                saml2AuthnResponse.SessionIndex = sessionIndex;

                var claimsIdentity = new ClaimsIdentity(claims);
                saml2AuthnResponse.NameId = new Saml2NameIdentifier(claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single(), relyingParty.NameIdentifierFormat);
                saml2AuthnResponse.ClaimsIdentity = claimsIdentity;

                var token = saml2AuthnResponse.CreateSecurityToken(relyingParty.Issuer, subjectConfirmationLifetime: 5, issuedTokenLifetime: 60);
            }

            return responsebinding.Bind(saml2AuthnResponse).ToActionResult();
        }

        private IActionResult LogoutResponse(Saml2Id inResponseTo, Saml2StatusCodes status, string relayState, string sessionIndex, SamlRelyingParty relyingParty)
        {
            var responsebinding = new Saml2PostBinding
            {
                RelayState = relayState
            };

            var saml2LogoutResponse = new Saml2LogoutResponse(_saml2Configuration)
            {
                InResponseTo = inResponseTo,
                Status = status,
                Destination = new Uri(relyingParty.SingleLogoutResponseDestination),
                SessionIndex = sessionIndex
            };

            return responsebinding.Bind(saml2LogoutResponse).ToActionResult();
        }

        private async Task<SamlRelyingParty> ValidateRelyingParty(string issuer)
        {
            var saml2RelyingParties = await _appSettingsService.GetSaml2RelyingPartiesAsync();

            var relyingParty = saml2RelyingParties.FirstOrDefault(x => x.Issuer.Equals(issuer, StringComparison.InvariantCultureIgnoreCase));

            if (relyingParty == null)
            {
                throw new Exception($"Relying party not found by {issuer}");
            }

            return relyingParty;
        }

        private IEnumerable<Claim> GetUserClaims(ApplicationUser user)
        {
            yield return new Claim(ClaimTypes.NameIdentifier, User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value);
            yield return new Claim(ClaimTypes.Email, User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email).Value);
            yield return new Claim(ClaimTypes.Name, User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name).Value);
            yield return new Claim(ClaimTypes.GivenName, user.FirstName);
            yield return new Claim(ClaimTypes.Surname, user.LastName);
        }
    }
}