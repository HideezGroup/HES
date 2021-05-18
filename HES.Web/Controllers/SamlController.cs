using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Models.Saml;
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
using System.Net.Http;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace HES.Web.Controllers
{
    [AllowAnonymous]
    [Route("Saml")]
    public class SamlController : Controller
    {
        private readonly Saml2RelyingParties _saml2RelyingParties;
        private readonly Saml2Configuration _saml2Configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<SamlController> _logger;

        public SamlController(SignInManager<ApplicationUser> signInManager, IOptions<Saml2RelyingParties> saml2RelyingParties, IOptions<Saml2Configuration> saml2Configuration, ILogger<SamlController> logger)
        {
            _saml2RelyingParties = saml2RelyingParties.Value;
            _saml2Configuration = saml2Configuration.Value;
            _signInManager = signInManager;
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
            RelyingParty relyingParty;
            Saml2AuthnRequest saml2AuthnRequest;

            try
            {
                requestBinding = new Saml2RedirectBinding();
                relyingParty = await ValidateRelyingParty(ReadRelyingPartyFromLoginRequest(requestBinding));
                saml2AuthnRequest = new Saml2AuthnRequest(_saml2Configuration);
            }
            catch (Exception)
            {
                var message = "Relying party validation failed";
                _logger.LogError(message);
                return BadRequest(message);
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
            RelyingParty relyingParty;
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
            catch (Exception)
            {
                var message = "Relying party validation failed";
                _logger.LogError(message);
                return BadRequest(message);
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
        public IActionResult Metadata()
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

            //entityDescriptor.ContactPerson = new ContactPerson(ContactTypes.Administrative)
            //{
            //    Company = "company",
            //    GivenName = "given name",
            //    SurName = "sur name",
            //    EmailAddress = "example@domain.com",
            //    TelephoneNumber = "tel",
            //};

            return new Saml2Metadata(entityDescriptor).CreateMetadata().ToActionResult();
        }

        private string ReadRelyingPartyFromLoginRequest<T>(Saml2Binding<T> binding)
        {
            return binding.ReadSamlRequest(Request.ToGenericHttpRequest(), new Saml2AuthnRequest(_saml2Configuration))?.Issuer;
        }

        private string ReadRelyingPartyFromLogoutRequest<T>(Saml2Binding<T> binding)
        {
            return binding.ReadSamlRequest(Request.ToGenericHttpRequest(), new Saml2LogoutRequest(_saml2Configuration))?.Issuer;
        }

        private IActionResult LoginResponse(Saml2Id inResponseTo, Saml2StatusCodes status, string relayState, RelyingParty relyingParty, string sessionIndex = null, IEnumerable<Claim> claims = null)
        {
            var responsebinding = new Saml2PostBinding
            {
                RelayState = relayState
            };

            var saml2AuthnResponse = new Saml2AuthnResponse(_saml2Configuration)
            {
                InResponseTo = inResponseTo,
                Status = status,
                Destination = relyingParty.SingleSignOnDestination,
            };

            if (status == Saml2StatusCodes.Success && claims != null)
            {
                saml2AuthnResponse.SessionIndex = sessionIndex;

                var claimsIdentity = new ClaimsIdentity(claims);
                saml2AuthnResponse.NameId = new Saml2NameIdentifier(claimsIdentity.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).Single(), NameIdentifierFormats.Persistent);
                saml2AuthnResponse.ClaimsIdentity = claimsIdentity;

                var token = saml2AuthnResponse.CreateSecurityToken(relyingParty.Issuer, subjectConfirmationLifetime: 5, issuedTokenLifetime: 60);
            }

            return responsebinding.Bind(saml2AuthnResponse).ToActionResult();
        }

        private IActionResult LogoutResponse(Saml2Id inResponseTo, Saml2StatusCodes status, string relayState, string sessionIndex, RelyingParty relyingParty)
        {
            var responsebinding = new Saml2PostBinding
            {
                RelayState = relayState
            };

            var saml2LogoutResponse = new Saml2LogoutResponse(_saml2Configuration)
            {
                InResponseTo = inResponseTo,
                Status = status,
                Destination = relyingParty.SingleLogoutResponseDestination,
                SessionIndex = sessionIndex
            };

            return responsebinding.Bind(saml2LogoutResponse).ToActionResult();
        }

        private async Task<RelyingParty> ValidateRelyingParty(string issuer)
        {
            foreach (var rp in _saml2RelyingParties.RelyingParties)
            {
                try
                {
                    if (string.IsNullOrEmpty(rp.Issuer))
                    {
                        var clientHandler = new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
                        };
                        using var client = new HttpClient(clientHandler);
                        var result = await client.GetStringAsync(rp.Metadata);

                        var entityDescriptor = new EntityDescriptor();
                        entityDescriptor = entityDescriptor.ReadSPSsoDescriptor(result);

                        //var entityDescriptor = new EntityDescriptor();
                        //entityDescriptor.ReadSPSsoDescriptorFromUrl(new Uri(rp.Metadata));

                        if (entityDescriptor.SPSsoDescriptor != null)
                        {
                            rp.Issuer = entityDescriptor.EntityId;
                            rp.SingleSignOnDestination = entityDescriptor.SPSsoDescriptor.AssertionConsumerServices.First().Location;
                            var singleLogoutService = entityDescriptor.SPSsoDescriptor.SingleLogoutServices.First();
                            rp.SingleLogoutResponseDestination = singleLogoutService.ResponseLocation ?? singleLogoutService.Location;
                            rp.SignatureValidationCertificate = entityDescriptor.SPSsoDescriptor.SigningCertificates.First();
                        }
                        else
                        {
                            throw new Exception($"SP SSO Descriptor not loaded from metadata '{rp.Metadata}'.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }

            return _saml2RelyingParties.RelyingParties.Where(rp => rp.Issuer != null && rp.Issuer.Equals(issuer, StringComparison.InvariantCultureIgnoreCase)).Single();
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