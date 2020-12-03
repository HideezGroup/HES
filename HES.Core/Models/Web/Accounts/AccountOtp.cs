using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.Accounts
{
    public class AccountOtp
    {
        [Display(Name = "OTP Secret")]
        public string OtpSecret { get; set; }
    }
}