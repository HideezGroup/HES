using HES.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace HES.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateLogin : RequiredAttribute
    {
        private string PropertyName { get; set; }

        public ValidateLogin(string propertyName)
        {
            PropertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            object instance = context.ObjectInstance;
            var prop = instance.GetType().GetProperty(PropertyName).GetValue(instance)?.ToString();
            LoginType currentType = (LoginType)Enum.Parse(typeof(LoginType), prop, true);

            var currentValue = value?.ToString();

            if (string.IsNullOrWhiteSpace(currentValue))
            {
                switch (currentType)
                {
                    case LoginType.WebApp:
                        return new ValidationResult("The Login field is required.", new[] { context.MemberName });
                    case LoginType.Local:
                        return new ValidationResult("The User Name field is required.", new[] { context.MemberName });
                    case LoginType.Domain:
                        return new ValidationResult("The User Logon Name field is required.", new[] { context.MemberName });
                    case LoginType.AzureAD:
                        return new ValidationResult("The User Name field is required.", new[] { context.MemberName });
                    case LoginType.Microsoft:
                        return new ValidationResult("The Login field is required.", new[] { context.MemberName });
                }
            }

            if (currentType == LoginType.Microsoft)
            {
                try
                {
                    var addr = new MailAddress(currentValue);
                    var isValid = addr.Address == currentValue;
                    if (!isValid)
                    {
                        return new ValidationResult("Login field is not a valid e-mail address.", new[] { context.MemberName });
                    }
                }
                catch
                {
                    return new ValidationResult("Login field is not a valid e-mail address.", new[] { context.MemberName });
                }
            }

            return ValidationResult.Success;
        }
    }
}