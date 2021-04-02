using Fido2NetLib;
using HES.Core.Entities;
using HES.Core.Models.API;
using HES.Core.Models.Identity;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IFido2Service
    {
        Task<FidoStoredCredential> AddSecurityKeyAsync(string userEmail, IJSRuntime jsRuntime);
        Task RemoveSecurityKeyAsync(string credentialId);
        Task<AuthenticatorAssertionRawResponse> MakeAssertionRawResponse(string userName, IJSRuntime jsRuntime);
        Task<AuthorizationResponse> SignInAsync(SecurityKeySignInModel parameters);
        Task<List<FidoStoredCredential>> GetCredentialsByUserEmail(string userEmail);
        Task RemoveCredentialsByUsername(string username);
        Task<FidoStoredCredential> GetCredentialById(byte[] id);
        Task<List<FidoStoredCredential>> GetCredentialsByUserHandleAsync(byte[] userHandle);
        Task UpdateCounter(byte[] credentialId, uint counter);
        Task<FidoStoredCredential> GetCredentialsById(string credentialId);
        Task UpdateSecurityKeyNameAsync(string securityKeyId, string name);
        Task<List<Fido2User>> GetUsersByCredentialIdAsync(byte[] credentialId);
    }
}