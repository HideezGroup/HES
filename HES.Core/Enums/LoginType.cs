using System.ComponentModel.DataAnnotations;

namespace HES.Core.Enums
{
    public enum LoginType
    {
        /// <summary>
        /// Any login
        /// </summary>
        [Display(Name = "Web-Sites and Applications")]
        WebApp,
        /// <summary>
        /// Local login for windows starts with .\
        /// </summary>
        [Display(Name = "Local Windows Account")]
        Local,
        /// <summary>
        /// Domain login for windows contains domain username separated by \
        /// </summary>
        [Display(Name = "AD Domain Account")]
        Domain,
        /// <summary>
        /// Azure login for Azure Active Directory starts with AzureAD\
        /// </summary>
        [Display(Name = "Azure AD Account")]
        AzureAD,
        /// <summary>
        /// Microsoft login for Microsoft Accounts starts with @\
        /// </summary>
        [Display(Name = "Microsoft Account")]
        Microsoft
    }
}