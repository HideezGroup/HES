using HES.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateDomain : ValidationAttribute
    {
        private string PropertyName { get; set; }

        public ValidateDomain(string propertyName)
        {
            PropertyName = propertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            object instance = context.ObjectInstance;           
            var prop = instance.GetType().GetProperty(PropertyName).GetValue(instance)?.ToString();
            LoginType currentType = (LoginType)Enum.Parse(typeof(LoginType), prop, true);

            if (currentType == LoginType.Domain && string.IsNullOrWhiteSpace(value?.ToString()))
            {
                return new ValidationResult(ErrorMessage, new[] { context.MemberName });
            }

            return ValidationResult.Success;
        }               
    }
}