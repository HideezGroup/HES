using HES.Core.Attributes;
using HES.Core.Entities;
using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HES.Core.Models.Web.SharedAccounts
{
    public class SharedAccountEditModel
    {
        public string Id { get; set; }

        [Required]
        [Display(Name = "Account Name")]
        public string Name { get; set; }

        [Display(Name = "Urls")]
        public string Urls { get; set; }

        [Display(Name = "Apps")]
        public string Apps { get; set; }

        [Display(Name = "Login Type")]
        public LoginType LoginType { get; set; }

        [Display(Name = "Login")]
        [ValidateLogin(nameof(LoginType))]
        public string Login { get; set; }

        [Display(Name = "Domain")]
        [ValidateDomain(nameof(LoginType), ErrorMessage = "The Domain field is required.")]
        public string Domain { get; set; }

        /// <summary>
        /// Mapping db entity to updating account model
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public SharedAccountEditModel Initialize(SharedAccount account)
        {
            Id = account.Id;   
            Name = account.Name;
            Urls = account.Urls;
            Apps = account.Apps;
            LoginType = account.LoginType;

            switch (LoginType)
            {
                case LoginType.WebApp:
                    Login = account.Login;
                    break;
                case LoginType.Local:
                    Login = account.Login.Replace(@".\", "");
                    break;
                case LoginType.Domain:
                    Login = Login.Split(@"\").LastOrDefault();
                    Domain = Login.Split(@"\").FirstOrDefault();
                    break;
                case LoginType.AzureAD:
                    Login = account.Login.Replace(@"AzureAD\", "");
                    break;
                case LoginType.Microsoft:
                    Login = account.Login.Replace(@"@\", "");
                    break;
            }

            return this;
        }

        /// <summary>
        /// Mapping account updating model to db entity 
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public SharedAccount SetNewValue(SharedAccount account)
        {
            account.Name = Name;
            account.Urls = Urls;
            account.Apps = Apps;
            account.LoginType = LoginType;
            account.Login = GetLogin();
            return account;
        }

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