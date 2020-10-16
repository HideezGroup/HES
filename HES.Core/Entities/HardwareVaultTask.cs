using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class HardwareVaultTask
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string Password { get; set; }
        public string OtpSecret { get; set; }
        public TaskOperation Operation { get; set; }
        public DateTime CreatedAt { get; set; }
        public uint Timestamp { get; set; }
        public string HardwareVaultId { get; set; }
        public string AccountId { get; set; }

        [ForeignKey("HardwareVaultId")]
        public HardwareVault HardwareVault { get; set; }

        [ForeignKey("AccountId")]
        public Account Account { get; set; }
    }

    public enum TaskOperation
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 3,
        Primary = 6,
        Profile = 7,
    }
}