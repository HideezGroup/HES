namespace HES.Core.Models.Filters
{
    public class ApplicationUserFilter
    {
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactorEnabled { get; set; }
    }
}
