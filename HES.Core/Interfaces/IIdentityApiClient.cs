using HES.Core.Models.API;
using HES.Core.Models.Identity;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IIdentityApiClient
    {
        Task<AuthorizationResponse> LoginWithPasswordAsync(PasswordSignInModel parameters);
        Task<AuthorizationResponse> LoginWithFido2Async(SecurityKeySignInModel parameters);
        Task<AuthorizationResponse> LogoutAsync();
        Task RefreshSignInAsync();
    }
}