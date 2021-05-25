using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.ApplicationUsers
{
    public class UserInvitation
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}