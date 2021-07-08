namespace HES.Core.Models.ApplicationUsers
{
    public class UserSsoSettings
    {
        public string ExternalId { get; set; }
        public bool AllowPasswordlessByU2F { get; set; }
    }
}