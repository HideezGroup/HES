using Microsoft.AspNetCore.Identity;

namespace HES.Core.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
    }
}