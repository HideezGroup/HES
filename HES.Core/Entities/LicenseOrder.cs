﻿using System;
using System.ComponentModel.DataAnnotations;

namespace HES.Core.Entities
{
    public class LicenseOrder
    {
        [Key]
        public string Id { get; set; }
        [Required]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }
        public string Note { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        public bool ProlongExistingLicenses { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public OrderStatus OrderStatus { get; set; } = OrderStatus.New;
    }

    public enum OrderStatus
    {
        New,
        Sent,
        Processing,
        Waiting,
        Completed,
        Closed,
        Cancelled
    }
}