namespace HES.Core.Models.Identity
{
    public class UserConfirmEmailChangeModel
    {
        public string UserId { get; set; }

        public string Email { get; set; }

        public string Code { get; set; }
    }
}