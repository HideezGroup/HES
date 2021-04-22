using HES.Core.Constants;
using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.Accounts;
using HES.Core.Models.AppSettings;
using Hideez.SDK.Communication.Security;
using Novell.Directory.Ldap;
using Novell.Directory.Ldap.Controls;
using Novell.Directory.Ldap.SearchExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;

namespace HES.Core.Services
{
    public class LdapService : ILdapService
    {
        private const string _syncGroupName = "Hideez Key Owners";
        private const string _pwdChangeGroupName = "Hideez Auto Password Change";

        private readonly IEmployeeService _employeeService;
        private readonly IOrgStructureService _orgStructureService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IAppSettingsService _appSettingsService;

        public LdapService(IEmployeeService employeeService,
                           IOrgStructureService orgStructureService,
                           IEmailSenderService emailSenderService,
                           IAppSettingsService appSettingsService)
        {
            _employeeService = employeeService;
            _orgStructureService = orgStructureService;
            _emailSenderService = emailSenderService;
            _appSettingsService = appSettingsService;
        }

        /// <summary>
        /// Checking the correct credentials for the active directory.
        /// </summary>
        public Task ValidateCredentialsAsync(LdapSettings ldapSettings)
        {
            var ldapCredentials = CreateLdapCredential(ldapSettings);
            var ldapConnectionOptions = new LdapConnectionOptions()
                .UseSsl()
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);

            using var connection = new LdapConnection(ldapConnectionOptions);
            connection.Connect(ldapSettings.Host, LdapConnection.DefaultSslPort);
            connection.Bind(LdapConnection.LdapV3, ldapCredentials.UserName, ldapCredentials.Password);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Get all users in the <_syncGroupName> group, if there are users in the active directory, then add to the HES or update the fields.
        /// </summary>
        public async Task SyncUsersAsync(LdapSettings ldapSettings)
        {
            var ldapCredentials = CreateLdapCredential(ldapSettings);

            var ldapConnectionOptions = new LdapConnectionOptions()
                .UseSsl()
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);

            using var connection = new LdapConnection(ldapConnectionOptions);
            connection.Connect(ldapSettings.Host, LdapConnection.DefaultSslPort);
            connection.Bind(LdapConnection.LdapV3, ldapCredentials.UserName, ldapCredentials.Password);

            var dn = GetDnFromHost(ldapSettings.Host);
            var groupFilter = $"(&(objectCategory=group)(name={_syncGroupName}))";
            var groupResponse = connection.Search(dn, LdapConnection.ScopeSub, groupFilter, null, false);

            var groups = GetEntries(groupResponse);
            if (groups.Count == 0)
                return;

            var groupDn = groups.FirstOrDefault().Dn;
            var membersFilter = $"(&(objectCategory=user)(memberOf={groupDn})(givenName=*))";

            var searchOptions = new SearchOptions(dn, LdapConnection.ScopeSub, membersFilter, null);
            var ldapSortControl = new LdapSortControl(new LdapSortKey("cn"), true);
            var members = connection.SearchUsingVlv(ldapSortControl, searchOptions, 1000);

            // Add new users or sync properties 
            foreach (var member in members)
            {
                var activeDirectoryGuid = GetAttributeGUID(member);
                var distinguishedName = TryGetAttribute(member, "distinguishedName");
                var firstName = TryGetAttribute(member, "givenName");
                var lastName = TryGetAttribute(member, "sn") ?? string.Empty;
                var email = TryGetAttribute(member, "mail");
                var phoneNumber = TryGetAttribute(member, "telephoneNumber");
                var companyName = TryGetAttribute(member, "company");
                var departmentName = TryGetAttribute(member, "department");
                var positionName = TryGetAttribute(member, "title");
                var whenChanged = TryGetDateTimeAttribute(member, "whenChanged");

                var employee = await _employeeService.GetEmployeeByNameAsync(firstName, lastName);
                var position = await _orgStructureService.TryAddAndGetPositionAsync(positionName);
                var department = await _orgStructureService.TryAddAndGetDepartmentWithCompanyAsync(companyName, departmentName);

                if (employee != null)
                {
                    if (whenChanged != null && whenChanged == employee.WhenChanged)
                        continue;

                    employee.ActiveDirectoryGuid = activeDirectoryGuid;
                    employee.FirstName = firstName;
                    employee.LastName = lastName;
                    employee.Email = email;
                    employee.PhoneNumber = phoneNumber;
                    employee.WhenChanged = whenChanged;
                    employee.DepartmentId = department?.Id;
                    employee.PositionId = position?.Id;

                    await _employeeService.EditEmployeeAsync(employee);
                }
                else
                {
                    employee = new Employee
                    {
                        ActiveDirectoryGuid = activeDirectoryGuid,
                        FirstName = firstName,
                        LastName = lastName,
                        Email = email,
                        PhoneNumber = phoneNumber,
                        WhenChanged = whenChanged,
                        DepartmentId = department?.Id,
                        PositionId = position?.Id,
                    };

                    await _employeeService.CreateEmployeeAsync(employee);
                }
            }

            // Sync users 
            var membersAdGuids = members.Select(x => GetAttributeGUID(x)).ToList();
            var employeesWithAdGuids = await _employeeService.GetEmployeesADAsync();

            // Employees whose access to hardware was taken away in the active dirictory
            var employeeRemovedFromGroup = employeesWithAdGuids.Where(x => !membersAdGuids.Contains(x.ActiveDirectoryGuid)).ToList();

            foreach (var employee in employeeRemovedFromGroup)
                await _employeeService.RemoveFromHideezKeyOwnersAsync(employee.Id);
        }

        /// <summary>
        /// Changes the password that will soon expire users in the active directory who are in group <_pwdChangeGroupName>.
        /// </summary>
        public async Task ChangePasswordWhenExpiredAsync(LdapSettings ldapSettings)
        {
            var ldapCredentials = CreateLdapCredential(ldapSettings);
            var ldapConnectionOptions = new LdapConnectionOptions()
                .UseSsl()
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);

            using var connection = new LdapConnection(ldapConnectionOptions);
            connection.Connect(ldapSettings.Host, LdapConnection.DefaultSslPort);
            connection.Bind(LdapConnection.LdapV3, ldapCredentials.UserName, ldapCredentials.Password);

            var dn = GetDnFromHost(ldapSettings.Host);
            var groupFilter = $"(&(objectCategory=group)(name={_pwdChangeGroupName}))";
            var groupResponse = connection.Search(dn, LdapConnection.ScopeSub, groupFilter, null, false);

            var groups = GetEntries(groupResponse);
            if (groups.Count == 0)
                return;

            var groupDn = groups.FirstOrDefault().Dn;
            var membersFilter = $"(&(objectCategory=user)(memberOf={groupDn})(givenName=*))";

            var searchOptions = new SearchOptions(dn, LdapConnection.ScopeSub, membersFilter, null);
            var ldapSortControl = new LdapSortControl(new LdapSortKey("cn"), true);
            var members = connection.SearchUsingVlv(ldapSortControl, searchOptions, 1000);

            foreach (var member in members)
            {
                // Find employee
                var memberGuid = GetAttributeGUID(member);
                var employee = await _employeeService.GetEmployeeByIdAsync(memberGuid, byActiveDirectoryGuid: true);

                // Not found because they were not added to the group for synchronization
                if (employee == null)
                    continue;

                var memberLogonName = $"{GetFirstDnFromHost(ldapSettings.Host)}\\{TryGetAttribute(member, "sAMAccountName")}";

                // Check a domain account exist
                var domainAccount = employee.Accounts.FirstOrDefault(x => x.Login == memberLogonName);
                if (domainAccount == null)
                {
                    var password = GeneratePassword();

                    var account = new AccountAddModel()
                    {
                        Name = "Domain Account",
                        LoginType = LoginType.Domain,
                        Domain = GetFirstDnFromHost(ldapSettings.Host),
                        Login = TryGetAttribute(member, "sAMAccountName"),
                        Password = password,
                        EmployeeId = employee.Id
                    };

                    using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                    {
                        // Create domain account
                        await _employeeService.CreatePersonalAccountAsync(account);

                        // Update password in active directory          
                        byte[] encodedBytes = System.Text.Encoding.Unicode.GetBytes($"\"{password}\"");
                        connection.Modify(member.Dn, new LdapModification(LdapModification.Replace, new LdapAttribute("unicodePwd", encodedBytes)));

                        transactionScope.Complete();
                    }

                    // Send notification when pasword changed
                    await _emailSenderService.NotifyWhenPasswordAutoChangedAsync(employee, memberLogonName);
                }
                else
                {
                    int maxPwdAge = ldapSettings.MaxPasswordAge;
                    var pwdLastSet = DateTime.FromFileTimeUtc(long.Parse(TryGetAttribute(member, "pwdLastSet")));
                    var currentPwdAge = DateTime.UtcNow.Subtract(pwdLastSet).TotalDays;

                    if (currentPwdAge >= maxPwdAge)
                    {
                        var password = GeneratePassword();

                        using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                        {
                            // Update domain account
                            await _employeeService.EditPersonalAccountPwdAsync(domainAccount, new AccountPassword() { Password = password });

                            // Update password in active directory     
                            connection.Modify(member.Dn, new LdapModification(Novell.Directory.Ldap.LdapModification.Replace, new LdapAttribute("userPassword", password)));

                            transactionScope.Complete();
                        }

                        // Send notification when password changed
                        await _emailSenderService.NotifyWhenPasswordAutoChangedAsync(employee, memberLogonName);
                    }
                }
            }
        }

        /// <summary>
        /// Changes the user password in the active directory.
        /// </summary>
        public async Task SetUserPasswordAsync(string employeeId, string password, LdapSettings ldapSettings)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);

            var ldapCredentials = CreateLdapCredential(ldapSettings);
            var ldapConnectionOptions = new LdapConnectionOptions()
                .UseSsl()
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);

            using var connection = new LdapConnection(ldapConnectionOptions);
            connection.Connect(ldapSettings.Host, LdapConnection.DefaultSslPort);
            connection.Bind(LdapConnection.LdapV3, ldapCredentials.UserName, ldapCredentials.Password);

            var dn = GetDnFromHost(ldapSettings.Host);
            var objectGUID = GetObjectGuid(employee.ActiveDirectoryGuid);
            var userFilter = $"(&(objectCategory=user)(objectGUID={objectGUID}))";
            var userResponse = connection.Search(dn, LdapConnection.ScopeSub, userFilter, null, false);

            var user = GetEntries(userResponse).FirstOrDefault();

            try
            {
                byte[] encodedBytes = System.Text.Encoding.Unicode.GetBytes($"\"{password}\"");
                connection.Modify(user.Dn, new LdapModification(LdapModification.Replace, new LdapAttribute("unicodePwd", encodedBytes)));
            }
            catch (LdapException ex) when (ex.ResultCode == LdapException.UnwillingToPerform)
            {
                throw new Exception("Active Directory password restrictions prevent the action.");
            }
        }

        /// <summary>
        /// Adds a user automatically to group <_syncGroupName> in the active directory, when a key is added to him on the HES
        /// </summary>
        public Task AddUserToHideezKeyOwnersAsync(LdapSettings ldapSettings, string activeDirectoryGuid)
        {
            var ldapCredentials = CreateLdapCredential(ldapSettings);
            var ldapConnectionOptions = new LdapConnectionOptions()
                .UseSsl()
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);

            using var connection = new LdapConnection(ldapConnectionOptions);
            connection.Connect(ldapSettings.Host, LdapConnection.DefaultSslPort);
            connection.Bind(LdapConnection.LdapV3, ldapCredentials.UserName, ldapCredentials.Password);

            var dn = GetDnFromHost(ldapSettings.Host);
            var filter = $"(&(objectCategory=group)(name={_syncGroupName}))";
            var response = connection.Search(dn, LdapConnection.ScopeSub, filter, null, false);

            var groups = GetEntries(response);
            if (groups.Count == 0)
                return Task.CompletedTask;

            var group = groups.FirstOrDefault();
            var members = TryGetAttributeArray(group, "member");

            var userFilter = $"(&(objectCategory=user)(objectGUID={GetObjectGuid(activeDirectoryGuid)}))";
            var userResponse = connection.Search(dn, LdapConnection.ScopeSub, userFilter, null, false);
            var user = GetEntries(userResponse).FirstOrDefault();
            var distinguishedName = TryGetAttribute(user, "distinguishedName");

            var userExistInGroup = members.Any(x => x.Contains(distinguishedName));
            if (userExistInGroup)
                return Task.CompletedTask;

            connection.Modify(user.Dn, new LdapModification(LdapModification.Add, new LdapAttribute("member", distinguishedName)));
            return Task.CompletedTask;
        }

        public async Task VerifyAdUserAsync(Employee employee)
        {
            if (employee.ActiveDirectoryGuid == null)
                return;

            var ldapSettings = await _appSettingsService.GetSettingsAsync<LdapSettings>(ServerConstants.Domain);
            if (ldapSettings == null)
                throw new HESException(HESCode.LdapSettingsNotSet);

            var ldapCredentials = CreateLdapCredential(ldapSettings);
            var ldapConnectionOptions = new LdapConnectionOptions()
                .UseSsl()
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);

            using var connection = new LdapConnection(ldapConnectionOptions);

            try
            {
                connection.Connect(ldapSettings.Host, LdapConnection.DefaultSslPort);
                connection.Bind(LdapConnection.LdapV3, ldapCredentials.UserName, ldapCredentials.Password);
            }
            catch (LdapException ex) when (ex.ResultCode == LdapException.ConnectError)
            {
                throw new Exception("Cannot establish a connection to the Ldap server");
            }

            var dn = GetDnFromHost(ldapSettings.Host);
            var objectGUID = GetObjectGuid(employee.ActiveDirectoryGuid);
            var filter = $"(&(objectCategory=user)(objectGUID={objectGUID}))";
            var response = connection.Search(dn, LdapConnection.ScopeSub, filter, null, false);

            var users = GetEntries(response);
            if (users.Count == 0)
                throw new HESException(HESCode.ActiveDirectoryUserNotFound);
        }

        #region Helpers

        private List<LdapEntry> GetEntries(ILdapSearchResults searchResult)
        {
            var entries = new List<LdapEntry>();

            try
            {
                while (searchResult.HasMore())
                {
                    entries.Add(searchResult.Next());
                }
            }
            catch (LdapException)
            {
                // Server does not hold the target entry of the request.
            }

            return entries;
        }

        class LdapCredential
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }

        private LdapCredential CreateLdapCredential(LdapSettings ldapSettings)
        {
            return new LdapCredential() { UserName = @$"{GetFirstDnFromHost(ldapSettings.Host)}\{ldapSettings.UserName}", Password = ldapSettings.Password };
        }

        private string GetAttributeGUID(LdapEntry entry)
        {
            return new Guid(entry.GetAttribute("objectGUID").ByteValue).ToString();
        }

        private string TryGetAttribute(LdapEntry entry, string attr)
        {
            try
            {
                return entry.GetAttribute(attr).StringValue;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private string[] TryGetAttributeArray(LdapEntry entry, string attr)
        {
            try
            {
                return entry.GetAttribute(attr).StringValueArray;
            }
            catch (Exception)
            {
                return new string[0];
            }
        }

        private DateTime? TryGetDateTimeAttribute(LdapEntry entry, string attr)
        {
            var succeeded = DateTime.TryParseExact(TryGetAttribute(entry, attr), "yyyyMMddHHmmss.0Z", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);

            if (succeeded)
                return date;
            else
                return null;
        }

        private string GetDnFromHost(string hostname)
        {
            char separator = '.';
            var parts = hostname.Split(separator);
            var dnParts = parts.Select(_ => $"dc={_}");
            return string.Join(",", dnParts);
        }

        private string GetFirstDnFromHost(string hostname)
        {
            char separator = '.';
            var parts = hostname.Split(separator);
            return parts[0];
        }

        private string GetNameFromDn(string dn)
        {
            char separator = ',';
            var parts = dn.Split(separator);
            return parts[0].Replace("CN=", string.Empty);
        }

        private string GetObjectGuid(string guid)
        {
            var ba = new Guid(guid).ToByteArray();
            var hex = BitConverter.ToString(ba).Insert(0, @"\").Replace("-", @"\");
            return hex;
        }

        private string GeneratePassword()
        {
            return PasswordGenerator.Generate();
        }

        #endregion                
    }
}