using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.AppUsers
{
    public class VerificationCode
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        public string Code { get; set; }
    }
}