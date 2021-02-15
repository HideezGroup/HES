using HES.Core.Entities;
using HES.Core.Models.API;
using HES.Core.Models.Web;
using HES.Core.Models.Web.Identity;
using HES.Core.Models.Web.Users;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface IApplicationUserService : IDisposable
    {
        Task<ApplicationUser> GetUserByIdAsync(string userId);
        Task<ApplicationUser> GetUserByEmailAsync(string email);
        Task<List<ApplicationUser>> GetAdministratorsAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions);
        Task<int> GetAdministratorsCountAsync(DataLoadingOptions<ApplicationUserFilter> dataLoadingOptions);
        Task<string> InviteAdministratorAsync(string email, string domain);
        Task<string> GetCallBackUrl(string email, string domain);
        Task<ApplicationUser> DeleteUserAsync(string id);
        Task<IList<ApplicationUser>> GetAllAdministratorsAsync();
        Task<AuthorizationResponse> LoginWithPasswordAsync(PasswordSignInModel parameters);
    }
}