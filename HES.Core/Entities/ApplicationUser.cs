using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace HES.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public string Culture { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }
    }
}