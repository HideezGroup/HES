namespace HES.Core.Models.AppUsers
{
    public class UserSsoInfo
    {
        public bool IsSsoEnabled { get; set; }
        public string UserEmail { get; set; }
        public string UserRole { get; set; }
        public string SecurityKeyName { get; set; }
    }
}