using Fido2NetLib;
using Fido2NetLib.Objects;
using HES.Core.Entities;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.API;
using HES.Core.Models.Web.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class Fido2Service : IFido2Service
    {
        // possible values: none, direct, indirect
        private const AttestationConveyancePreference AttestationType = AttestationConveyancePreference.None;
        // possible values: <empty>, platform, cross-platform
        private const AuthenticatorAttachment Attachment = AuthenticatorAttachment.CrossPlatform;
        // possible values: preferred, required, discouraged
        private const UserVerificationRequirement UserVerification = UserVerificationRequirement.Preferred;
        // possible values: true, false
        private const bool RequireResidentKey = false;

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IAsyncRepository<FidoStoredCredential> _fidoCredentialsRepository;
        private readonly IAsyncRepository<ApplicationUser> _applicationUserRepository;
        private readonly Fido2 _lib;
        private readonly IOptions<Fido2Configuration> _fido2Configuration;
        private readonly IMemoryCache _memoryCache;

        public Fido2Service(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IAsyncRepository<FidoStoredCredential> fidoCredentialsRepository,
            IAsyncRepository<ApplicationUser> applicationUserRepository,
            IOptions<Fido2Configuration> fido2Configuration,
            IMemoryCache memoryCache)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _fidoCredentialsRepository = fidoCredentialsRepository;
            _applicationUserRepository = applicationUserRepository;
            _fido2Configuration = fido2Configuration;
            _memoryCache = memoryCache;
            _lib = new Fido2(new Fido2Configuration()
            {
                ServerDomain = _fido2Configuration.Value.ServerDomain,
                ServerName = _fido2Configuration.Value.ServerName,
                Origin = _fido2Configuration.Value.Origin,
                TimestampDriftTolerance = _fido2Configuration.Value.TimestampDriftTolerance
            });
        }

        #region Manage

        public async Task<FidoStoredCredential> AddSecurityKeyAsync(string userEmail, IJSRuntime jsRuntime)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                throw new ArgumentNullException(nameof(userEmail));

            if (jsRuntime == null)
                throw new ArgumentNullException(nameof(jsRuntime));

            var fidoUser = new Fido2User
            {
                Id = Encoding.UTF8.GetBytes(userEmail),
                Name = userEmail,
                DisplayName = userEmail
            };

            var authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = RequireResidentKey,
                UserVerification = UserVerification,
                AuthenticatorAttachment = null // platform and cross-platform
            };

            var exts = new AuthenticationExtensionsClientInputs
            {
                Extensions = true,
                UserVerificationIndex = true,
                Location = true,
                UserVerificationMethod = true,
                BiometricAuthenticatorPerformanceBounds = new AuthenticatorBiometricPerfBounds
                {
                    FAR = float.MaxValue,
                    FRR = float.MaxValue
                }
            };

            var credentials = await GetCredentialsByUserEmail(userEmail);

            var options = _lib.RequestNewCredential(fidoUser, credentials.Select(x => x.Descriptor).ToList(), authenticatorSelection, AttestationType, exts);

            var attestationResponseJson = await jsRuntime.InvokeAsync<string>("createCredentials", TimeSpan.FromMinutes(3), JsonConvert.SerializeObject(options, new JsonSerializerSettings { Converters = new List<JsonConverter> { new StringEnumConverter() } }));

            var attestationResponse = JsonConvert.DeserializeObject<AuthenticatorAttestationRawResponse>(attestationResponseJson);

            // Create callback so that lib can verify credential id is unique to this user
            IsCredentialIdUniqueToUserAsyncDelegate callback = async (IsCredentialIdUniqueToUserParams args) =>
            {
                var users = await GetUsersByCredentialIdAsync(args.CredentialId);
                if (users.Count > 0)
                    return false;

                return true;
            };

            // Verify and make the credentials
            var success = await _lib.MakeNewCredentialAsync(attestationResponse, options, callback);

            // Store the credentials in db
            return await _fidoCredentialsRepository.AddAsync(new FidoStoredCredential
            {
                UserId = options.User.Id,
                Username = options.User.Name,
                SecurityKeyName = $"Security Key",
                Descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId),
                PublicKey = success.Result.PublicKey,
                UserHandle = success.Result.User.Id,
                SignatureCounter = success.Result.Counter,
                CredType = success.Result.CredType,
                RegDate = DateTime.UtcNow,
                AaGuid = success.Result.Aaguid
            });
        }

        public async Task RemoveSecurityKeyAsync(string credentialId)
        {
            var fido2Key = await _fidoCredentialsRepository.GetByIdAsync(credentialId);
            if (fido2Key == null)
                throw new HESException(HESCode.SecurityKeyNotFound);

            await _fidoCredentialsRepository.DeleteAsync(fido2Key);
        }

        public async Task UpdateSecurityKeyNameAsync(string securityKeyId, string name)
        {
            if (string.IsNullOrWhiteSpace(securityKeyId))
                throw new ArgumentNullException(nameof(securityKeyId));

            if (string.IsNullOrWhiteSpace(name))
                name = "Security Key";

            var credential = await _fidoCredentialsRepository.GetByIdAsync(securityKeyId);
            if (credential == null)
                throw new HESException(HESCode.SecurityKeyNotFound);

            credential.SecurityKeyName = name;

            await _fidoCredentialsRepository.UpdateAsync(credential);
        }

        #endregion

        #region Sign In

        public async Task<AuthenticatorAssertionRawResponse> MakeAssertionRawResponse(string userEmail, IJSRuntime jsRuntime)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                throw new ArgumentNullException(nameof(userEmail));

            if (jsRuntime == null)
                throw new ArgumentNullException(nameof(jsRuntime));

            var identityUser = await _userManager.FindByEmailAsync(userEmail);
            if (identityUser == null)
                throw new HESException(HESCode.UserNotFound);

            var user = new Fido2User
            {
                DisplayName = identityUser.UserName,
                Name = identityUser.UserName,
                Id = Encoding.UTF8.GetBytes(identityUser.UserName) // byte representation of userID is required
            };

            // Get registered credentials from database          
            var items = await GetCredentialsByUserEmail(identityUser.UserName);
            var existingCredentials = items.Select(c => c.Descriptor).ToList();

            var exts = new AuthenticationExtensionsClientInputs() { SimpleTransactionAuthorization = "FIDO", GenericTransactionAuthorization = new TxAuthGenericArg { ContentType = "text/plain", Content = new byte[] { 0x46, 0x49, 0x44, 0x4F } }, UserVerificationIndex = true, Location = true, UserVerificationMethod = true };

            // Create options
            var options = _lib.GetAssertionOptions(existingCredentials, UserVerification, exts);

            var optionsJson = JsonConvert.SerializeObject(options, new JsonSerializerSettings { Converters = new List<JsonConverter> { new StringEnumConverter() } });

            var assertionRawResponseJson = await jsRuntime.InvokeAsync<string>("getCredentials", TimeSpan.FromMinutes(3), optionsJson);

            var assertionRawResponse = JsonConvert.DeserializeObject<AuthenticatorAssertionRawResponse>(assertionRawResponseJson);

            // Temporarily store options
            _memoryCache.Set(Convert.ToBase64String(assertionRawResponse.Id), options.ToJson(), TimeSpan.FromMinutes(5));

            return assertionRawResponse;
        }

        // Only API call
        public async Task<AuthorizationResponse> SignInAsync(SecurityKeySignInModel parameters)
        {
            var assertionRawResponse = parameters.AuthenticatorAssertionRawResponse;

            // Get the assertion options we sent the client     
            var jsonOptions = _memoryCache.Get<string>(Convert.ToBase64String(assertionRawResponse.Id));
            var options = AssertionOptions.FromJson(jsonOptions);

            // Get registered credential from database
            var creds = await GetCredentialById(assertionRawResponse.Id);

            if (creds == null)
                throw new Exception("Unknown credentials");

            // Get credential counter from database
            var storedCounter = creds.SignatureCounter;

            // Create callback to check if userhandle owns the credentialId
            IsUserHandleOwnerOfCredentialIdAsync callback = async (args) =>
            {
                var storedCreds = await GetCredentialsByUserHandleAsync(args.UserHandle);
                return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
            };

            // Make the assertion
            var res = await _lib.MakeAssertionAsync(assertionRawResponse, options, creds.PublicKey, storedCounter, callback);

            // Store the updated counter
            await UpdateCounter(res.CredentialId, res.Counter);

            // Get authenticator flags
            var authData = new AuthenticatorData(assertionRawResponse.Response.AuthenticatorData);

            if (authData.UserPresent && authData.UserVerified)
            {
                var user = await _userManager.FindByNameAsync(creds.Username);
                if (user == null)
                    throw new HESException(HESCode.UserNotFound);

                await _signInManager.SignInAsync(user, parameters.RememberMe);

                return AuthorizationResponse.Success(user);
            }

            return AuthorizationResponse.Error(HESCode.AuthenticatorNotFIDO2);
        }

        #endregion

        #region Helpers

        public async Task<FidoStoredCredential> GetCredentialsById(string credentialId)
        {
            var credential = await _fidoCredentialsRepository.GetByIdAsync(credentialId);
            if (credential == null)
                throw new HESException(HESCode.SecurityKeyNotFound);

            return credential;
        }

        public async Task<List<FidoStoredCredential>> GetCredentialsByUserEmail(string userEmail)
        {
            return await _fidoCredentialsRepository.Query().Where(c => c.Username == userEmail).AsNoTracking().ToListAsync();
        }

        public async Task RemoveCredentialsByUsername(string username)
        {
            var items = await _fidoCredentialsRepository.Query().Where(c => c.Username == username).AsNoTracking().ToListAsync();
            if (items != null)
                await _fidoCredentialsRepository.DeleteRangeAsync(items);
        }

        public async Task<FidoStoredCredential> GetCredentialById(byte[] id)
        {
            var credentialIdString = Base64Url.Encode(id);
            //byte[] credentialIdStringByte = Base64Url.Decode(credentialIdString);

            var cred = await _fidoCredentialsRepository.Query().Where(c => c.DescriptorJson.Contains(credentialIdString)).FirstOrDefaultAsync();

            return cred;
        }

        public async Task<List<FidoStoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle)
        {
            return await _fidoCredentialsRepository.Query().Where(c => c.UserHandle.SequenceEqual(userHandle)).AsNoTracking().ToListAsync();
        }

        public async Task UpdateCounter(byte[] credentialId, uint counter)
        {
            var credentialIdString = Base64Url.Encode(credentialId);
            //byte[] credentialIdStringByte = Base64Url.Decode(credentialIdString);

            var cred = await _fidoCredentialsRepository.Query().Where(c => c.DescriptorJson.Contains(credentialIdString)).FirstOrDefaultAsync();

            cred.SignatureCounter = counter;
            await _fidoCredentialsRepository.UpdateAsync(cred);
        }

        public async Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] credentialId)
        {
            var credentialIdString = Base64Url.Encode(credentialId);
            //byte[] credentialIdStringByte = Base64Url.Decode(credentialIdString);

            var cred = await _fidoCredentialsRepository.Query().Where(c => c.DescriptorJson.Contains(credentialIdString)).FirstOrDefaultAsync();

            if (cred == null)
            {
                return new List<Fido2User>();
            }

            return await _applicationUserRepository.Query()
                    .Where(u => Encoding.UTF8.GetBytes(u.UserName)
                    .SequenceEqual(cred.UserId))
                    .Select(u => new Fido2User
                    {
                        DisplayName = u.UserName,
                        Name = u.UserName,
                        Id = Encoding.UTF8.GetBytes(u.UserName) // byte representation of userID is required
                    }).ToListAsync();
        }

        #endregion
    }
}
