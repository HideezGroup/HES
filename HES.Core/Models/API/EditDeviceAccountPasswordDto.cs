﻿using System.ComponentModel.DataAnnotations;

namespace HES.Core.Models.API
{
    public class EditDeviceAccountPasswordDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
        public string Password { get; set; }
        [Required]
        public string DeviceId { get; set; }
    }
}