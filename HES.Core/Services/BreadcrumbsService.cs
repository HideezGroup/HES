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
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Settings },
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_DataProtection }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetDashboard()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Dashboard }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetAdministrators()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Administrators }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetEmployees()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Employees }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetTemplates()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Templates }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetEmployeeDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Employees", Content = Resources.Resource.Breadcrumbs_Employees },
                new Breadcrumb () { Active = true, Content = name}
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetHardwareVaults()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_HardwareVaults }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetGroups()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Groups }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetGroupDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Groups", Content = Resources.Resource.Breadcrumbs_Groups },
                new Breadcrumb () { Active = true, Content = name}
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetLicenseOrders()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Settings },
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_LicenseOrders }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetHardwareVaultProfiles()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Settings },
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_HardwareVaultAccessProfiles }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetSharedAccounts()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_SharedAccounts }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetAuditWorkstationEvents()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Audit },
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_WorkstationEvents }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetAuditWorkstationSessions()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Audit },
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_WorkstationSessions }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetAuditSummaries()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Audit },
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Summaries }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetParameters()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Settings },
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Parameters }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetOrgStructure()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Settings },
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_OrgStructure }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetWorkstations()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Workstations }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetWorkstationDetails(string name)
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = false, Link= "/Workstations", Content = Resources.Resource.Breadcrumbs_Workstations },
                new Breadcrumb () { Active = true, Content = name}
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetProfile()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Profile }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }

        public async Task SetAlarm()
        {
            Breadcrumbs = new List<Breadcrumb>()
            {
                new Breadcrumb () { Active = true, Content = Resources.Resource.Breadcrumbs_Alarm }
            };

            if (OnSet != null)
                await OnSet.Invoke(Breadcrumbs);
        }
    }
}