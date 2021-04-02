using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.AppUsers
{
    public class Invitation
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}