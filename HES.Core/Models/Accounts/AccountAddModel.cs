using HES.Core.Attributes;
using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Accounts
{
    public class AccountAddModel
    {
        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string EmployeeId { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Name), ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_UrlsSeparate), ResourceType = typeof(Resources.Resource))]
        public string Urls { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_AppsSeparate), ResourceType = typeof(Resources.Resource))]
        public string Apps { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LoginType), ResourceType = typeof(Resources.Resource))]
        public LoginType LoginType { get; set; }

        [ValidateLogin(nameof(LoginType), ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Login), ResourceType = typeof(Resources.Resource))]
        public string Login { get; set; }

        [ValidateDomain(nameof(LoginType), ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Domain), ResourceType = typeof(Resources.Resource))]
        public string Domain { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_Password), ResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_ConfirmPassword), ResourceType = typeof(Resources.Resource))]
        [CompareProperty("Password", ErrorMessageResourceName = nameof(Resources.Resource.Validation_ComparePassword), ErrorMessageResourceType = typeof(Resources.Resource))]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_OtpSecret), ResourceType = typeof(Resources.Resource))]
        public string OtpSecret { get; set; }

        public bool UpdateInActiveDirectory { get; set; }

        public string GetLogin()
        {
            return LoginType switch
            {
                LoginType.WebApp => $"{Login}",
                LoginType.Local => $".\\{Login}",
                LoginType.Domain => $"{Domain}\\{Login}",
                LoginType.AzureAD => $"AzureAD\\{Login}",
                LoginType.Microsoft => $"@\\{Login}",
                _ => Login,
            };
        }
    }
}