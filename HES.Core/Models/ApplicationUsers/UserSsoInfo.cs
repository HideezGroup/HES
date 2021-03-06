﻿namespace HES.Core.Models.ApplicationUsers
{
    public class UserSsoInfo
    {
        public bool IsSsoEnabled { get; set; }
        public string UserEmail { get; set; }
        public string UserRole { get; set; }
        public string SecurityKeyName { get; set; }
        public string ExternalId { get; set; }
        public bool AllowPasswordlessByU2F { get; set; }
    }
}