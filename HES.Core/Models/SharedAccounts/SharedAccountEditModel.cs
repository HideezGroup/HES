﻿using HES.Core.Attributes;
using HES.Core.Entities;
using HES.Core.Enums;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace HES.Core.Models.SharedAccounts
{
    public class SharedAccountEditModel
    {
        public string Id { get; set; }

        [Required(ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        [Display(Name = nameof(Resources.Resource.Display_AccountName), ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_UrlsSeparate), ResourceType = typeof(Resources.Resource))]
        public string Urls { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_AppsSeparate), ResourceType = typeof(Resources.Resource))]
        public string Apps { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_LoginType), ResourceType = typeof(Resources.Resource))]
        public LoginType LoginType { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Login), ResourceType = typeof(Resources.Resource))]
        [ValidateLogin(nameof(LoginType), ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
        public string Login { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Domain), ResourceType = typeof(Resources.Resource))]
        [ValidateDomain(nameof(LoginType), ErrorMessageResourceName = nameof(Resources.Resource.Validation_Required), ErrorMessageResourceType = typeof(Resources.Resource))]
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