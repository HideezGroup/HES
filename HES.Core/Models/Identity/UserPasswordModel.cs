using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Identity
{
    public class UserPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}