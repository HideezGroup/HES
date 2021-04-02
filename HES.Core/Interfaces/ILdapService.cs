using HES.Core.Entities;
using HES.Core.Models.ActiveDirectory;
using HES.Core.Models.AppSettings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Interfaces
{
    public interface ILdapService : IDisposable
    {
        Task ValidateCredentialsAsync(LdapSettings ldapSettings);
        Task<List<ActiveDirectoryUser>> GetUsersAsync(LdapSettings ldapSettings);
        Task AddUsersAsync(List<ActiveDirectoryUser> users, bool createAccounts, bool createGroups);
        Task SetUserPasswordAsync(string employeeId, string password, LdapSettings ldapSettings);
        Task ChangePasswordWhenExpiredAsync(LdapSettings ldapSettings);
        Task SyncUsersAsync(LdapSettings ldapSettings);
        Task AddUserToHideezKeyOwnersAsync(LdapSettings ldapSettings, string activeDirectoryGuid);
        Task<List<ActiveDirectoryGroup>> GetGroupsAsync(LdapSettings ldapSettings);
        Task AddGroupsAsync(List<ActiveDirectoryGroup> groups, bool createEmployees);
        Task VerifyAdUserAsync(Employee employee);
    }
}