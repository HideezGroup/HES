using HES.Core.Attributes;
using HES.Core.Entities;
using HES.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HES.Core.Models.Web.Accounts
{
    public class AccountEditModel
    {
        [Required]
        public string Id { get; set; }

        [Required]
        public string EmployeeId { get; set; }

        [Required]
        [Display(Name = "Account Name")]
        public string Name { get; set; }

        [Display(Name = "URLs (separate by semicolon)")]
        public string Urls { get; set; }

        [Display(Name = "Applications (separate by semicolon)")]
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
        public AccountEditModel Initialize(Account account)
        {
            Id = account.Id;
            EmployeeId = account.EmployeeId;
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
                    Login = account.Login.Split(@"\").LastOrDefault();
                    Domain = account.Login.Split(@"\").FirstOrDefault();
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
        public Account SetNewValue(Account account)
        {
            account.Name = Name;
            account.Urls = Urls;
            account.Apps = Apps;
            account.LoginType = LoginType;
            account.Login = GetLogin();
            account.UpdatedAt = DateTime.UtcNow;

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