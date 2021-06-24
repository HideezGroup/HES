using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Accounts
{
    public class AccountOtp
    {
        [Display(Name = nameof(Resources.Resource.Display_OtpSecret), ResourceType = typeof(Resources.Resource))]
        public string OtpSecret { get; set; }
    }
}