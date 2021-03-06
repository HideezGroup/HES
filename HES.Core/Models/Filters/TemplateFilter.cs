﻿using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Filters
{
    public class TemplateFilter
    {
        [Display(Name = nameof(Resources.Resource.Display_Name), ResourceType = typeof(Resources.Resource))]
        public string Name { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Urls), ResourceType = typeof(Resources.Resource))]
        public string Urls { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Apps), ResourceType = typeof(Resources.Resource))]
        public string Apps { get; set; }
    }
}