﻿using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.Web.AppSettings
{
    public class LdapSettings
    {
        [Required]
        public string Host { get; set; }
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }

        [Required]
        [Range(1, 180)]
        public int MaxPasswordAge { get; set; } = 28;
    }
}