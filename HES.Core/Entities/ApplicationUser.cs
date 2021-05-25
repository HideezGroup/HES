using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Culture { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        [NotMapped]
        public string DisplayName => $"{FirstName} {LastName}".Trim();
    }
}