using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Culture { get; set; }

        public virtual ICollection<ApplicationUserRole> UserRoles { get; set; }

        [NotMapped]
        [Display(Name = "Name")]
        public string DisplayName => $"{FirstName} {LastName}".Trim();
    }
}