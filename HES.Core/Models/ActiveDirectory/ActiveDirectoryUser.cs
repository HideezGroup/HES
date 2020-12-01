using HES.Core.Entities;
using HES.Core.Models.Web.Accounts;
using System.Collections.Generic;

namespace HES.Core.Models.ActiveDirectory
{
    public class ActiveDirectoryUser
    {
        public Employee Employee { get; set; }
        public PersonalAccount Account { get; set; }
        public List<Group> Groups { get; set; }
        public bool Checked { get; set; }
    }
}