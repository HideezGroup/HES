using HES.Core.Entities;
using HES.Core.Models.Accounts;
using System.Collections.Generic;

namespace HES.Core.Models.ActiveDirectory
{
    public class ActiveDirectoryUser
    {
        public Employee Employee { get; set; }
        public AccountAddModel Account { get; set; }
        public List<Group> Groups { get; set; }
        public bool Checked { get; set; }
    }
}