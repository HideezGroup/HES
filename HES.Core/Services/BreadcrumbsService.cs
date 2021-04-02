using HES.Core.Interfaces;
using HES.Core.Models.Breadcrumb;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HES.Core.Services
{
    public class BreadcrumbsService : IBreadcrumbsService
    {
        public event Func<List<Breadcrumb>, Task> OnSet;
        public List<Breadcrumb> Breadcrumbs { get; set; }

        public async Task SetDataProtection()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Settings" },
                new Breadcrumb () { Active = true, Content = "Data Protection" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetDashboard()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Dashboard" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetAdministrators()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Administrators" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetEmployees()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Employees" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetTemplates()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Templates" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetEmployeeDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Employees", Content = "Employees" },
                new Breadcrumb () { Active = true, Content = name}
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetHardwareVaults()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Hardware Vaults" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetGroups()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Groups" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetGroupDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Groups", Content = "Groups" },
                new Breadcrumb () { Active = true, Content = name}
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetLicenseOrders()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Settings" },
                new Breadcrumb () { Active = true, Content = "License Orders" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetHardwareVaultProfiles()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Settings" },
                new Breadcrumb () { Active = true, Content = "Hardware Vault Access Profiles" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetSharedAccounts()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Shared Accounts" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetAuditWorkstationEvents()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Audit" },
                new Breadcrumb () { Active = true, Content = "Workstation Events" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetAuditWorkstationSessions()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Audit" },
                new Breadcrumb () { Active = true, Content = "Workstation Sessions" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetAuditSummaries()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Audit" },
                new Breadcrumb () { Active = true, Content = "Summaries" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetParameters()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Settings" },
                new Breadcrumb () { Active = true, Content = "Parameters" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetOrgStructure()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Settings" },
                new Breadcrumb () { Active = true, Content = "OrgStructure" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetWorkstations()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Workstations" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetWorkstationDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Workstations", Content = "Workstations" },
                new Breadcrumb () { Active = true, Content = name}
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetTwoFactorAuthentication()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Two Factor Authentication" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetShowRecoveryCodes()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Show Recovery Codes" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetResetAuthenticator()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Reset Authenticator" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetProfile()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Profile" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetPersonalData()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Personal Data" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetGenerateRecoveryCodes()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Generate Recovery Codes" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetEnableAuthenticator()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Enable Authenticator" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetDisable2fa()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Disable 2FA" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetDeletePersonalData()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Delete Personal Data" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetAlarm()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = "Alarm" }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }
    }
}