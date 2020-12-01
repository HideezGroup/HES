using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class EditAccountPasswordDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public string Password { get; set; }
    }
}