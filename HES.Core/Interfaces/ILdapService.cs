using HES.Core.Entities;
using HES.Core.Models.AppSettings;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILdapService
    {
        Task ValidateCredentialsAsync(LdapSettings ldapSettings);
        Task SetUserPasswordAsync(string employeeId, string password, LdapSettings ldapSettings);
        Task ChangePasswordWhenExpiredAsync(LdapSettings ldapSettings);
        Task SyncUsersAsync(LdapSettings ldapSettings);
        Task AddUserToHideezKeyOwnersAsync(LdapSettings ldapSettings, string activeDirectoryGuid);
        Task VerifyAdUserAsync(Employee employee);
    }
}