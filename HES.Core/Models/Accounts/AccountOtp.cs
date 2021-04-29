using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Accounts
{
    public class AccountOtp
    {
        [Display(Name = "OTP Secret")]
        public string OtpSecret { get; set; }
    }
}