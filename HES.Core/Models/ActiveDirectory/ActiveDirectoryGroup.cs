using HES.Core.Entities;
using HES.Core.Models.Web.Accounts;
using System.Collections.Generic;

namespace HES.Core.Models.ActiveDirectory
{
    public class ActiveDirectoryGroup
    {
        public Group Group { get; set; }
        public List<ActiveDirectoryGroupMembers> Members { get; set; }
        public bool Checked { get; set; }
    }

    public class ActiveDirectoryGroupMembers
    {
        public Employee Employee { get; set; }
        public AccountAddModel Account { get; set; }
    }
}