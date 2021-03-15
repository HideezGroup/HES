using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Identity
{
    public class UserPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}