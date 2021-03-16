using Microsoft.AspNetCore.Components;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Reflection;

namespace HES.Web.Components
{
    public partial class Label<TValue> : HESDomComponentBase
    {
        [Parameter] public RenderFragment Suffix { get; set; }
        [Parameter] public Expression<Func<TValue>> For { get; set; }

        public string DisplayName { get; set; }
        public bool IsRequired { get; set; }

        protected override void OnInitialized()
        {
            var expression = (MemberExpression)For.Body;

            IsRequired = expression.Member.GetCustomAttribute<RequiredAttribute>() != null ? true : false;

            var displayNameAttribute = expression.Member.GetCustomAttribute<DisplayAttribute>();

            if (displayNameAttribute?.ResourceType != null)
            {
                try
                {
                    DisplayName = displayNameAttribute.ResourceType.GetProperty(displayNameAttribute.Name).GetValue(displayNameAttribute.ResourceType).ToString();
                }
                catch (Exception)
                {
                    DisplayName = expression.Member.Name;
                }
                return;
            }

            DisplayName = displayNameAttribute != null ? displayNameAttribute.Name : expression.Member.Name;
        }
    }
}