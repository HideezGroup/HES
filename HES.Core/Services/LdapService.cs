using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Exceptions;
using HES.Core.Interfaces;
using HES.Core.Models.ActiveDirectory;
using HES.Core.Models.Web.Accounts;
using HES.Core.Models.Web.AppSettings;
using Hideez.SDK.Communication.Security;
using LdapForNet;
using Microsoft.EntityFrameworkCore;
using Novell.Directory.Ldap.SearchExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using static LdapForNet.Native.Native;

namespace HES.Core.Services
{
    public class LdapService : ILdapService, IDisposable
    {
        private const string _syncGroupName = "Hideez Key Owners";
        private const string _pwdChangeGroupName = "Hideez Auto Password Change";

        private readonly IEmployeeService _employeeService;
        private readonly IGroupService _groupService;
        private readonly IOrgStructureService _orgStructureService;
        private readonly IEmailSenderService _emailSenderService;
        private readonly IAppSettingsService _appSettingsService;

        public LdapService(IEmployeeService employeeService,
                           IGroupService groupService,
                           IOrgStructureService orgStructureService,
                           IEmailSenderService emailSenderService,
                           IAppSettingsService appSettingsService)
        {
            _employeeService = employeeService;
            _groupService = groupService;
            _orgStructureService = orgStructureService;
            _emailSenderService = emailSenderService;
            _appSettingsService = appSettingsService;
        }

        public async Task ValidateCredentialsAsync(LdapSettings ldapSettings)
        {
            using (var connection = new LdapConnection())
            {
                connection.Connect(ldapSettings.Host, 636, LdapSchema.LDAPS);
                connection.TrustAllCertificates();
                connection.SetOption(LdapOption.LDAP_OPT_REFERRALS, 0);
                await connection.BindAsync(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));
            }
        }

        public async Task<List<ActiveDirectoryUser>> GetUsersAsync(LdapSettings ldapSettings)
        {
            var users = new List<ActiveDirectoryUser>();

            using (var connection = new LdapConnection())
            {
                connection.Connect(ldapSettings.Host, 3268);
                await connection.BindAsync(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));

                var dn = GetDnFromHost(ldapSettings.Host);

                var filter = "(&(objectCategory=user)(givenName=*))";
                var pageResultRequestControl = new PageResultRequestControl(500) { IsCritical = true };
                var searchRequest = new SearchRequest(dn, filter, LdapSearchScope.LDAP_SCOPE_SUBTREE)
                {
                    AttributesOnly = false,
                    TimeLimit = TimeSpan.Zero,
                    Controls = { pageResultRequestControl }
                };

                var entries = new List<DirectoryEntry>();

                while (true)
                {
                    var response = (SearchResponse)connection.SendRequest(searchRequest);

                    foreach (var control in response.Controls)
                    {
                        if (control is PageResultResponseControl)
                        {
                            // Update the cookie for next set
                            pageResultRequestControl.Cookie = ((PageResultResponseControl)control).Cookie;
                            break;
                        }
                    }

                    // Add them to our collection
                    foreach (var entry in response.Entries)
                    {
                        entries.Add(entry);
                    }

                    // Our exit condition is when our cookie is empty
                    if (pageResultRequestControl.Cookie.Length == 0)
                        break;
                }

                foreach (var entity in entries)
                {
                    var activeDirectoryUser = new ActiveDirectoryUser()
                    {
                        Employee = new Employee()
                        {
                            Id = Guid.NewGuid().ToString(),
                            ActiveDirectoryGuid = GetAttributeGUID(entity),
                            FirstName = TryGetAttribute(entity, "givenName"),
                            LastName = TryGetAttribute(entity, "sn"),
                            Email = TryGetAttribute(entity, "mail"),
                            PhoneNumber = TryGetAttribute(entity, "telephoneNumber")
                        },
                        Account = new AccountAddModel()
                        {
                            Name = "Domain Account",
                            LoginType = LoginType.Domain,
                            Domain = GetFirstDnFromHost(ldapSettings.Host),
                            Login = TryGetAttribute(entity, "sAMAccountName"),
                            Password = GeneratePassword(),
                            UpdateInActiveDirectory = true
                        }
                    };

                    List<Group> groups = new List<Group>();

                    var groupNames = TryGetAttributeArray(entity, "memberOf");
                    if (groupNames != null)
                    {
                        foreach (var groupName in groupNames)
                        {
                            var name = GetNameFromDn(groupName);
                            var filterGroup = $"(&(objectCategory=group)(name={name}))";
                            var groupResponse = (SearchResponse)connection.SendRequest(new SearchRequest(dn, filterGroup, LdapSearchScope.LDAP_SCOPE_SUBTREE));

                            groups.Add(new Group()
                            {
                                Id = GetAttributeGUID(groupResponse.Entries.First()),
                                Name = TryGetAttribute(groupResponse.Entries.First(), "name"),
                                Description = TryGetAttribute(groupResponse.Entries.First(), "description"),
                                Email = TryGetAttribute(groupResponse.Entries.First(), "mail")
                            });
                        }
                    }

                    activeDirectoryUser.Groups = groups.Count > 0 ? groups : null;

                    users.Add(activeDirectoryUser);
                }
            }

            return users.OrderBy(x => x.Employee.FullName).ToList();
        }

        public async Task AddUsersAsync(List<ActiveDirectoryUser> users, bool createAccounts, bool createGroups)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var user in users)
                {
                    var employee = await _employeeService.ImportEmployeeAsync(user.Employee);

                    if (createAccounts)
                    {
                        try
                        {
                            // The employee may already be in the database, so we get his ID and create an account
                            user.Account.EmployeeId = employee.Id;
                            await _employeeService.CreatePersonalAccountAsync(user.Account);
                        }
                        catch (AlreadyExistException)
                        {
                            // Ignore if a domain account exists
                        }
                    }

                    if (createGroups && user.Groups != null)
                    {
                        await _groupService.CreateGroupRangeAsync(user.Groups);
                        await _groupService.AddEmployeeToGroupsAsync(employee.Id, user.Groups.Select(s => s.Id).ToList());
                    }
                }
                transactionScope.Complete();
            }
        }

        public async Task SetUserPasswordAsync(string employeeId, string password, LdapSettings ldapSettings)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(employeeId);

            var ldapConnectionOptions = new Novell.Directory.Ldap.LdapConnectionOptions()
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);

            using (var connection = new Novell.Directory.Ldap.LdapConnection(ldapConnectionOptions) { SecureSocketLayer = true })
            {
                connection.Connect(ldapSettings.Host, Novell.Directory.Ldap.LdapConnection.DefaultSslPort);

                var credentials = CreateLdapCredential(ldapSettings);
                connection.Bind(Novell.Directory.Ldap.LdapConnection.LdapV3, credentials.UserName, credentials.Password);

                var dn = GetDnFromHost(ldapSettings.Host);
                var objectGUID = GetObjectGuid(employee.ActiveDirectoryGuid);
                var userFilter = $"(&(objectCategory=user)(objectGUID={objectGUID}))";
                var userResponse = connection.Search(dn, Novell.Directory.Ldap.LdapConnection.ScopeSub, userFilter, null, false);

                var user = GetEntries(userResponse).FirstOrDefault();

                try
                {
                    connection.Modify(user.Dn, new Novell.Directory.Ldap.LdapModification(Novell.Directory.Ldap.LdapModification.Replace, new Novell.Directory.Ldap.LdapAttribute("userPassword", password)));
                }
                catch (Novell.Directory.Ldap.LdapException ex) when (ex.ResultCode == (int)ResultCode.UnwillingToPerform)
                {
                    throw new Exception("Active Directory password restrictions prevent the action.");
                }
            }

            #region LdapForNet
            //using (var connection = new LdapConnection())
            //{
            //    connection.Connect(ldapSettings.Host, 636, LdapSchema.LDAPS);
            //    connection.TrustAllCertificates();
            //    connection.SetOption(LdapOption.LDAP_OPT_REFERRALS, 0);
            //    connection.Bind(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));

            //    var dn = GetDnFromHost(ldapSettings.Host);
            //    var objectGUID = GetObjectGuid(employee.ActiveDirectoryGuid);
            //    var user = (SearchResponse)connection.SendRequest(new SearchRequest(dn, $"(&(objectCategory=user)(objectGUID={objectGUID}))", LdapSearchScope.LDAP_SCOPE_SUBTREE));

            //    try
            //    {
            //        await connection.ModifyAsync(new LdapModifyEntry
            //        {
            //            Dn = user.Entries.First().Dn,
            //            Attributes = new List<LdapModifyAttribute>
            //            {
            //                new LdapModifyAttribute
            //                {
            //                    LdapModOperation = LdapModOperation.LDAP_MOD_REPLACE,
            //                    Type = "userPassword",
            //                    Values = new List<string> { password }
            //                }
            //            }
            //        });
            //    }
            //    catch (LdapException ex) when (ex.ResultCode == ResultCode.UnwillingToPerform)
            //    {
            //        throw new Exception("Active Directory password restrictions prevent the action.");
            //    }
            //}
            #endregion
        }

        private List<Novell.Directory.Ldap.LdapEntry> GetEntries(Novell.Directory.Ldap.ILdapSearchResults searchResult)
        {
            List<Novell.Directory.Ldap.LdapEntry> entries = new List<Novell.Directory.Ldap.LdapEntry>();

            try
            {
                while (searchResult.HasMore())
                {
                    entries.Add(searchResult.Next());
                }
            }
            catch (Novell.Directory.Ldap.LdapException)
            {
                //trash
            }

            return entries;
        }

        public async Task ChangePasswordWhenExpiredAsync(LdapSettings ldapSettings)
        {
            var ldapConnectionOptions = new Novell.Directory.Ldap.LdapConnectionOptions()
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);

            using (var connection = new Novell.Directory.Ldap.LdapConnection(ldapConnectionOptions) { SecureSocketLayer = true })
            {
                connection.Connect(ldapSettings.Host, Novell.Directory.Ldap.LdapConnection.DefaultSslPort);

                var credentials = CreateLdapCredential(ldapSettings);
                connection.Bind(Novell.Directory.Ldap.LdapConnection.LdapV3, credentials.UserName, credentials.Password);


                var dn = GetDnFromHost(ldapSettings.Host);
                var groupFilter = $"(&(objectCategory=group)(name={_pwdChangeGroupName}))";
                var groupResponse = connection.Search(dn, Novell.Directory.Ldap.LdapConnection.ScopeSub, groupFilter, null, false);

                var groups = GetEntries(groupResponse);

                if (groups.Count == 0)
                    return;

                var groupDn = groups.FirstOrDefault().Dn;
                var membersFilter = $"(&(objectCategory=user)(memberOf={groupDn})(givenName=*))";

                var searchOptions = new Novell.Directory.Ldap.SearchOptions(
                dn,
                Novell.Directory.Ldap.LdapConnection.ScopeSub,
                membersFilter,
                null);
                var ldapSortControl = new Novell.Directory.Ldap.Controls.LdapSortControl(new Novell.Directory.Ldap.Controls.LdapSortKey("cn"), true);
                var members = connection.SearchUsingVlv(
                        ldapSortControl,
                        searchOptions,
                        1000
                    );


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
                            connection.Modify(member.Dn, new Novell.Directory.Ldap.LdapModification(Novell.Directory.Ldap.LdapModification.Replace, new Novell.Directory.Ldap.LdapAttribute("userPassword", password)));

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
                                connection.Modify(member.Dn, new Novell.Directory.Ldap.LdapModification(Novell.Directory.Ldap.LdapModification.Replace, new Novell.Directory.Ldap.LdapAttribute("userPassword", password)));

                                transactionScope.Complete();
                            }

                            // Send notification when password changed
                            await _emailSenderService.NotifyWhenPasswordAutoChangedAsync(employee, memberLogonName);
                        }
                    }
                }

                connection.Disconnect();
            }

            #region LdapForNet
            //using (var connection = new LdapConnection())
            //{
            //    connection.Connect(ldapSettings.Host, 636, LdapSchema.LDAPS);
            //    connection.TrustAllCertificates();
            //    connection.SetOption(LdapOption.LDAP_OPT_REFERRALS, 0);
            //    connection.Bind(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));

            //    var dn = GetDnFromHost(ldapSettings.Host);
            //    var groupFilter = $"(&(objectCategory=group)(name={_pwdChangeGroupName}))";
            //    var groupSearchRequest = new SearchRequest(dn, groupFilter, LdapSearchScope.LDAP_SCOPE_SUBTREE);
            //    var groupResponse = (SearchResponse)connection.SendRequest(groupSearchRequest);

            //    // If the group was not created, exit the method
            //    if (groupResponse.Entries.Count == 0)
            //        return;

            //    var groupDn = groupResponse.Entries.FirstOrDefault().Dn;

            //    var membersFilter = $"(&(objectCategory=user)(memberOf={groupDn})(givenName=*))";
            //    var membersPageResultRequestControl = new PageResultRequestControl(500) { IsCritical = true };
            //    var membersSearchRequest = new SearchRequest(dn, membersFilter, LdapSearchScope.LDAP_SCOPE_SUBTREE)
            //    {
            //        AttributesOnly = false,
            //        TimeLimit = TimeSpan.Zero,
            //        Controls = { membersPageResultRequestControl }
            //    };

            //    var members = new List<DirectoryEntry>();

            //    while (true)
            //    {
            //        var membersResponse = (SearchResponse)connection.SendRequest(membersSearchRequest);

            //        foreach (var control in membersResponse.Controls)
            //        {
            //            if (control is PageResultResponseControl)
            //            {
            //                // Update the cookie for next set
            //                membersPageResultRequestControl.Cookie = ((PageResultResponseControl)control).Cookie;
            //                break;
            //            }
            //        }

            //        // Add them to our collection
            //        members.AddRange(membersResponse.Entries);

            //        // Our exit condition is when our cookie is empty
            //        if (membersPageResultRequestControl.Cookie.Length == 0)
            //            break;
            //    }

            //    foreach (var member in members)
            //    {
            //        // Find employee
            //        var memberGuid = GetAttributeGUID(member);
            //        var employee = await _employeeService.GetEmployeeByIdAsync(memberGuid, byActiveDirectoryGuid: true);

            //        // Not found because they were not added to the group for synchronization
            //        if (employee == null)
            //            continue;

            //        var memberLogonName = $"{GetFirstDnFromHost(ldapSettings.Host)}\\{TryGetAttribute(member, "sAMAccountName")}";

            //        // Check a domain account exist
            //        var domainAccount = employee.Accounts.FirstOrDefault(x => x.Login == memberLogonName);
            //        if (domainAccount == null)
            //        {
            //            var password = GeneratePassword();

            //            var account = new AccountAddModel()
            //            {
            //                Name = "Domain Account",
            //                LoginType = LoginType.Domain,
            //                Domain = GetFirstDnFromHost(ldapSettings.Host),
            //                Login = TryGetAttribute(member, "sAMAccountName"),
            //                Password = password,
            //                EmployeeId = employee.Id
            //            };

            //            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //            {
            //                // Create domain account
            //                await _employeeService.CreatePersonalAccountAsync(account);

            //                // Update password in active directory                   
            //                await connection.ModifyAsync(new LdapModifyEntry
            //                {
            //                    Dn = member.Dn,
            //                    Attributes = new List<LdapModifyAttribute>
            //                    {
            //                        new LdapModifyAttribute
            //                        {
            //                            LdapModOperation = LdapModOperation.LDAP_MOD_REPLACE,
            //                            Type = "userPassword",
            //                            Values = new List<string> { password }
            //                        }
            //                    }
            //                });

            //                transactionScope.Complete();
            //            }

            //            // Send notification when pasword changed
            //            await _emailSenderService.NotifyWhenPasswordAutoChangedAsync(employee, memberLogonName);
            //        }
            //        else
            //        {
            //            int maxPwdAge = ldapSettings.MaxPasswordAge;
            //            var pwdLastSet = DateTime.FromFileTimeUtc(long.Parse(TryGetAttribute(member, "pwdLastSet")));
            //            var currentPwdAge = DateTime.UtcNow.Subtract(pwdLastSet).TotalDays;

            //            if (currentPwdAge >= maxPwdAge)
            //            {
            //                var password = GeneratePassword();

            //                using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            //                {
            //                    // Update domain account
            //                    await _employeeService.EditPersonalAccountPwdAsync(domainAccount, new AccountPassword() { Password = password });

            //                    // Update password in active directory                             
            //                    await connection.ModifyAsync(new LdapModifyEntry
            //                    {
            //                        Dn = member.Dn,
            //                        Attributes = new List<LdapModifyAttribute>
            //                        {
            //                            new LdapModifyAttribute
            //                            {
            //                                LdapModOperation = LdapModOperation.LDAP_MOD_REPLACE,
            //                                Type = "userPassword",
            //                                Values = new List<string> { password }
            //                            }
            //                        }
            //                    });

            //                    transactionScope.Complete();
            //                }

            //                // Send notification when password changed
            //                await _emailSenderService.NotifyWhenPasswordAutoChangedAsync(employee, memberLogonName);
            //            }
            //        }
            //    }
            //}
            #endregion
        }

        public async Task SyncUsersAsync(LdapSettings ldapSettings)
        {
            var ldapConnectionOptions = new Novell.Directory.Ldap.LdapConnectionOptions()
                .ConfigureRemoteCertificateValidationCallback((sender, certificate, chain, errors) => true);

            using (var connection = new Novell.Directory.Ldap.LdapConnection(ldapConnectionOptions) { SecureSocketLayer = true })
            {
                connection.Connect(ldapSettings.Host, Novell.Directory.Ldap.LdapConnection.DefaultSslPort);
                var credentials = CreateLdapCredential(ldapSettings);
                connection.Bind(Novell.Directory.Ldap.LdapConnection.LdapV3, credentials.UserName, credentials.Password);


                var dn = GetDnFromHost(ldapSettings.Host);
                var groupFilter = $"(&(objectCategory=group)(name={_syncGroupName}))";
                var groupResponse = connection.Search(dn, Novell.Directory.Ldap.LdapConnection.ScopeSub, groupFilter, null, false);

                var groups = GetEntries(groupResponse);

                if (groups.Count == 0)
                    return;

                var groupDn = groups.FirstOrDefault().Dn;
                var membersFilter = $"(&(objectCategory=user)(memberOf={groupDn})(givenName=*))";

                var searchOptions = new Novell.Directory.Ldap.SearchOptions(
                dn,
                Novell.Directory.Ldap.LdapConnection.ScopeSub,
                membersFilter,
                null);
                var ldapSortControl = new Novell.Directory.Ldap.Controls.LdapSortControl(new Novell.Directory.Ldap.Controls.LdapSortKey("cn"), true);
                var members = connection.SearchUsingVlv(
                        ldapSortControl,
                        searchOptions,
                        1000
                    );

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

                    var employee = await _employeeService.EmployeeQuery().FirstOrDefaultAsync(x => x.FirstName == firstName && x.LastName == lastName);
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
                var employeesWithAdGuids = await _employeeService.EmployeeQuery().Where(x => x.ActiveDirectoryGuid != null).ToListAsync();

                // Employees whose access to hardware was taken away in the active dirictory
                var employeeRemovedFromGroup = employeesWithAdGuids.Where(x => !membersAdGuids.Contains(x.ActiveDirectoryGuid)).ToList();

                foreach (var employee in employeeRemovedFromGroup)
                    await _employeeService.RemoveFromHideezKeyOwnersAsync(employee.Id);
            }

            #region LdapforNet
            //using (var connection = new LdapConnection())
            //{
            //    connection.Connect(ldapSettings.Host, 636, LdapSchema.LDAPS);
            //    connection.TrustAllCertificates();
            //    connection.SetOption(LdapOption.LDAP_OPT_REFERRALS, 0);
            //    connection.Bind(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));

            //    var dn = GetDnFromHost(ldapSettings.Host);

            //    var groupFilter = $"(&(objectCategory=group)(name={_syncGroupName}))";
            //    var groupSearchRequest = new SearchRequest(dn, groupFilter, LdapSearchScope.LDAP_SCOPE_SUBTREE);
            //    var groupResponse = (SearchResponse)connection.SendRequest(groupSearchRequest);

            //    // If the group was not created, exit the method
            //    if (groupResponse.Entries.Count == 0)
            //        return;

            //    var groupDn = groupResponse.Entries.FirstOrDefault().Dn;

            //    var membersFilter = $"(&(objectCategory=user)(memberOf={groupDn})(givenName=*))";
            //    var membersPageResultRequestControl = new PageResultRequestControl(500) { IsCritical = true };
            //    var membersSearchRequest = new SearchRequest(dn, membersFilter, LdapSearchScope.LDAP_SCOPE_SUBTREE)
            //    {
            //        AttributesOnly = false,
            //        TimeLimit = TimeSpan.Zero,
            //        Controls = { membersPageResultRequestControl }
            //    };

            //    var members = new List<DirectoryEntry>();

            //    while (true)
            //    {
            //        var membersResponse = (SearchResponse)connection.SendRequest(membersSearchRequest);

            //        foreach (var control in membersResponse.Controls)
            //        {
            //            if (control is PageResultResponseControl)
            //            {
            //                // Update the cookie for next set
            //                membersPageResultRequestControl.Cookie = ((PageResultResponseControl)control).Cookie;
            //                break;
            //            }
            //        }

            //        // Add them to our collection
            //        members.AddRange(membersResponse.Entries);

            //        // Our exit condition is when our cookie is empty
            //        if (membersPageResultRequestControl.Cookie.Length == 0)
            //            break;
            //    }

            //    // Add new users or sync properties 
            //    foreach (var member in members)
            //    {
            //        var activeDirectoryGuid = GetAttributeGUID(member);
            //        var distinguishedName = TryGetAttribute(member, "distinguishedName");
            //        var firstName = TryGetAttribute(member, "givenName");
            //        var lastName = TryGetAttribute(member, "sn") ?? string.Empty;
            //        var email = TryGetAttribute(member, "mail");
            //        var phoneNumber = TryGetAttribute(member, "telephoneNumber");
            //        var companyName = TryGetAttribute(member, "company");
            //        var departmentName = TryGetAttribute(member, "department");
            //        var positionName = TryGetAttribute(member, "title");
            //        var whenChanged = TryGetDateTimeAttribute(member, "whenChanged");

            //        var employee = await _employeeService.EmployeeQuery().FirstOrDefaultAsync(x => x.FirstName == firstName && x.LastName == lastName);
            //        var position = await _orgStructureService.TryAddAndGetPositionAsync(positionName);
            //        var department = await _orgStructureService.TryAddAndGetDepartmentWithCompanyAsync(companyName, departmentName);

            //        if (employee != null)
            //        {
            //            if (whenChanged != null && whenChanged == employee.WhenChanged)
            //                continue;

            //            employee.ActiveDirectoryGuid = activeDirectoryGuid;
            //            employee.FirstName = firstName;
            //            employee.LastName = lastName;
            //            employee.Email = email;
            //            employee.PhoneNumber = phoneNumber;
            //            employee.WhenChanged = whenChanged;
            //            employee.DepartmentId = department?.Id;
            //            employee.PositionId = position?.Id;

            //            await _employeeService.EditEmployeeAsync(employee);
            //        }
            //        else
            //        {
            //            employee = new Employee
            //            {
            //                ActiveDirectoryGuid = activeDirectoryGuid,
            //                FirstName = firstName,
            //                LastName = lastName,
            //                Email = email,
            //                PhoneNumber = phoneNumber,
            //                WhenChanged = whenChanged,
            //                DepartmentId = department?.Id,
            //                PositionId = position?.Id,
            //            };

            //            await _employeeService.CreateEmployeeAsync(employee);
            //        }
            //    }

            //    // Sync users 
            //    var membersAdGuids = members.Select(x => GetAttributeGUID(x)).ToList();
            //    var employeesWithAdGuids = await _employeeService.EmployeeQuery().Where(x => x.ActiveDirectoryGuid != null).ToListAsync();

            //    // Employees whose access to hardware was taken away in the active dirictory
            //    var employeeRemovedFromGroup = employeesWithAdGuids.Where(x => !membersAdGuids.Contains(x.ActiveDirectoryGuid)).ToList();

            //    foreach (var employee in employeeRemovedFromGroup)
            //        await _employeeService.RemoveFromHideezKeyOwnersAsync(employee.Id);
            //}
            #endregion
        }

        public async Task AddUserToHideezKeyOwnersAsync(LdapSettings ldapSettings, string activeDirectoryGuid)
        {
            using (var connection = new LdapConnection())
            {
                connection.Connect(ldapSettings.Host, 636, LdapSchema.LDAPS);
                connection.TrustAllCertificates();
                connection.SetOption(LdapOption.LDAP_OPT_REFERRALS, 0);
                connection.Bind(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));

                var dn = GetDnFromHost(ldapSettings.Host);
                var filter = $"(&(objectCategory=group)(name={_syncGroupName}))";
                var searchRequest = new SearchRequest(dn, filter, LdapSearchScope.LDAP_SCOPE_SUBTREE);

                var response = (SearchResponse)connection.SendRequest(searchRequest);

                if (response.Entries.Count == 0)
                    return;

                var group = response.Entries.FirstOrDefault();
                var members = TryGetAttributeArray(group, "member");

                var user = (SearchResponse)connection.SendRequest(new SearchRequest(dn, $"(&(objectCategory=user)(objectGUID={GetObjectGuid(activeDirectoryGuid)}))", LdapSearchScope.LDAP_SCOPE_SUBTREE));
                var distinguishedName = TryGetAttribute(user.Entries.FirstOrDefault(), "distinguishedName");

                var userExistInGroup = members.Any(x => x.Contains(distinguishedName));
                if (userExistInGroup)
                    return;

                await connection.ModifyAsync(new LdapModifyEntry
                {
                    Dn = group.Dn,
                    Attributes = new List<LdapModifyAttribute>
                    {
                        new LdapModifyAttribute
                        {
                            LdapModOperation = LdapModOperation.LDAP_MOD_ADD,
                            Type = "member",
                            Values = new List<string> { distinguishedName }
                        },
                    }
                });
            }
        }

        public async Task<List<ActiveDirectoryGroup>> GetGroupsAsync(LdapSettings ldapSettings)
        {
            var groups = new List<ActiveDirectoryGroup>();

            using (var connection = new LdapConnection())
            {
                connection.Connect(ldapSettings.Host, 3268);
                await connection.BindAsync(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));

                var dn = GetDnFromHost(ldapSettings.Host);

                var filter = "(objectCategory=group)";
                var pageResultRequestControl = new PageResultRequestControl(500) { IsCritical = true };
                var searchRequest = new SearchRequest(dn, filter, LdapSearchScope.LDAP_SCOPE_SUBTREE)
                {
                    AttributesOnly = false,
                    TimeLimit = TimeSpan.Zero,
                    Controls = { pageResultRequestControl }
                };

                var entries = new List<DirectoryEntry>();

                while (true)
                {
                    var response = (SearchResponse)connection.SendRequest(searchRequest);

                    foreach (var control in response.Controls)
                    {
                        if (control is PageResultResponseControl)
                        {
                            // Update the cookie for next set
                            pageResultRequestControl.Cookie = ((PageResultResponseControl)control).Cookie;
                            break;
                        }
                    }

                    // Add them to our collection
                    foreach (var entry in response.Entries)
                    {
                        entries.Add(entry);
                    }

                    // Our exit condition is when our cookie is empty
                    if (pageResultRequestControl.Cookie.Length == 0)
                        break;
                }

                foreach (var entity in entries)
                {
                    var activeDirectoryGroup = new ActiveDirectoryGroup()
                    {
                        Group = new Group()
                        {
                            Id = GetAttributeGUID(entity),
                            Name = TryGetAttribute(entity, "name"),
                            Description = TryGetAttribute(entity, "description"),
                            Email = TryGetAttribute(entity, "mail")
                        }
                    };

                    List<ActiveDirectoryGroupMembers> groupMembers = new List<ActiveDirectoryGroupMembers>();
                    var membersFilter = $"(&(objectCategory=user)(memberOf={entity.Dn})(givenName=*))";
                    var membersPageResultRequestControl = new PageResultRequestControl(500) { IsCritical = true };
                    var membersSearchRequest = new SearchRequest(dn, membersFilter, LdapSearchScope.LDAP_SCOPE_SUBTREE)
                    {
                        AttributesOnly = false,
                        TimeLimit = TimeSpan.Zero,
                        Controls = { membersPageResultRequestControl }
                    };

                    var members = new List<DirectoryEntry>();

                    while (true)
                    {
                        var response = (SearchResponse)connection.SendRequest(membersSearchRequest);

                        foreach (var control in response.Controls)
                        {
                            if (control is PageResultResponseControl)
                            {
                                // Update the cookie for next set
                                membersPageResultRequestControl.Cookie = ((PageResultResponseControl)control).Cookie;
                                break;
                            }
                        }

                        // Add them to our collection
                        foreach (var entry in response.Entries)
                        {
                            members.Add(entry);
                        }

                        // Our exit condition is when our cookie is empty
                        if (membersPageResultRequestControl.Cookie.Length == 0)
                            break;
                    }

                    foreach (var member in members)
                    {
                        groupMembers.Add(new ActiveDirectoryGroupMembers()
                        {
                            Employee = new Employee()
                            {
                                Id = Guid.NewGuid().ToString(),
                                ActiveDirectoryGuid = GetAttributeGUID(member),
                                FirstName = TryGetAttribute(member, "givenName"),
                                LastName = TryGetAttribute(member, "sn"),
                                Email = TryGetAttribute(member, "mail"),
                                PhoneNumber = TryGetAttribute(member, "telephoneNumber")
                            },
                            Account = new AccountAddModel()
                            {
                                Name = "Domain Account",
                                LoginType = LoginType.Domain,
                                Domain = GetFirstDnFromHost(ldapSettings.Host),
                                Login = TryGetAttribute(member, "sAMAccountName"),
                                Password = GeneratePassword(),
                                UpdateInActiveDirectory = true
                            }
                        });
                    }

                    activeDirectoryGroup.Members = groupMembers.Count > 0 ? groupMembers : null;
                    groups.Add(activeDirectoryGroup);
                }
            }

            return groups.OrderBy(x => x.Group.Name).ToList();
        }

        public async Task AddGroupsAsync(List<ActiveDirectoryGroup> groups, bool createEmployees)
        {
            using (TransactionScope transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                foreach (var group in groups)
                {
                    Group currentGroup = null;
                    try
                    {
                        currentGroup = await _groupService.CreateGroupAsync(group.Group);
                    }
                    catch (AlreadyExistException)
                    {
                        // If group exist
                        currentGroup = await _groupService.GetGroupByNameAsync(group.Group);
                    }

                    if (createEmployees && group.Members != null)
                    {
                        foreach (var member in group.Members)
                        {
                            var employee = await _employeeService.ImportEmployeeAsync(member.Employee);
                            member.Employee.Id = employee.Id;

                            try
                            {
                                // The employee may already be in the database, so we get his ID and create an account
                                member.Account.EmployeeId = employee.Id;
                                await _employeeService.CreatePersonalAccountAsync(member.Account);
                            }
                            catch (AlreadyExistException)
                            {
                                // Ignore if a domain account exists
                            }
                        }

                        await _groupService.AddEmployeesToGroupAsync(group.Members.Select(s => s.Employee.Id).ToList(), currentGroup.Id);
                    }
                }
                transactionScope.Complete();
            }
        }

        public async Task VerifyAdUserAsync(Employee employee)
        {
            if (employee.ActiveDirectoryGuid == null)
                return;

            var setting = await _appSettingsService.GetLdapSettingsAsync();
            if (setting == null)
                throw new HESException(HESCode.LdapSettingsNotSet);

            var user = GetUserByGuid(setting, employee.ActiveDirectoryGuid);
            if (user == null)
                throw new HESException(HESCode.ActiveDirectoryUserNotFound);
        }

        #region Helpers

        private LdapCredential CreateLdapCredential(LdapSettings ldapSettings)
        {
            return new LdapCredential() { UserName = @$"{GetFirstDnFromHost(ldapSettings.Host)}\{ldapSettings.UserName}", Password = ldapSettings.Password };
        }

        private string GetAttributeGUID(DirectoryEntry entry)
        {
            return new Guid(entry.Attributes["objectGUID"].GetValues<byte[]>().First()).ToString();
        }

        private string GetAttributeGUID(Novell.Directory.Ldap.LdapEntry entry)
        {
            return new Guid(entry.GetAttribute("objectGUID").ByteValue).ToString();
        }

        private string TryGetAttribute(DirectoryEntry entry, string attr)
        {
            DirectoryAttribute directoryAttribute;
            return entry.Attributes.TryGetValue(attr, out directoryAttribute) == true ? directoryAttribute.GetValues<string>().First() : null;
        }

        private string TryGetAttribute(Novell.Directory.Ldap.LdapEntry entry, string attr)
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

        private string[] TryGetAttributeArray(DirectoryEntry entry, string attr)
        {
            DirectoryAttribute directoryAttribute;
            return entry.Attributes.TryGetValue(attr, out directoryAttribute) == true ? directoryAttribute.GetValues<string>().ToArray() : null;
        }

        private DateTime? TryGetDateTimeAttribute(DirectoryEntry entry, string attr)
        {
            var succeeded = DateTime.TryParseExact(TryGetAttribute(entry, attr), "yyyyMMddHHmmss.0Z", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date);

            if (succeeded)
                return date;
            else
                return null;
        }

        private DateTime? TryGetDateTimeAttribute(Novell.Directory.Ldap.LdapEntry entry, string attr)
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

        private DirectoryEntry GetUserByGuid(LdapSettings ldapSettings, string guid)
        {
            using (var connection = new LdapConnection())
            {
                connection.Connect(ldapSettings.Host, 636, LdapSchema.LDAPS);
                connection.TrustAllCertificates();
                connection.SetOption(LdapOption.LDAP_OPT_REFERRALS, 0);
                connection.Bind(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));

                var dn = GetDnFromHost(ldapSettings.Host);
                var objectGUID = GetObjectGuid(guid);
                var filter = $"(&(objectCategory=user)(objectGUID={objectGUID}))";
                var user = (SearchResponse)connection.SendRequest(new SearchRequest(dn, filter, LdapSearchScope.LDAP_SCOPE_SUBTREE));
                return user.Entries.FirstOrDefault();
            }
        }

        //private DirectoryEntry GetUserByDn(LdapSettings ldapSettings, string distinguishedName)
        //{
        //    using (var connection = new LdapConnection())
        //    {
        //        connection.Connect(ldapSettings.Host, 389);
        //        connection.Bind(LdapAuthType.Simple, CreateLdapCredential(ldapSettings));

        //        var dn = GetDnFromHost(ldapSettings.Host);
        //        var filter = $"(&(objectCategory=user)(distinguishedName={distinguishedName}))";
        //        var user = (SearchResponse)connection.SendRequest(new SearchRequest(dn, filter, LdapSearchScope.LDAP_SCOPE_SUBTREE));
        //        return user.Entries.FirstOrDefault();
        //    }
        //}

        #endregion

        public void Dispose()
        {
            _employeeService.Dispose();
            _groupService.Dispose();
            _emailSenderService.Dispose();
        }
    }
}