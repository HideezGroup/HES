using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HES.Core.Entities
{
    public class WorkstationHardwareVaultPair
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        public string HardwareVaultId { get; set; }
        public string WorkstationId { get; set; }
        public int LockProximity { get; set; }
        public int UnlockProximity { get; set; }
        public int LockTimeout { get; set; }

        [ForeignKey("HardwareVaultId")]
        public HardwareVault HardwareVault { get; set; }

        [ForeignKey("WorkstationId")]
        public Workstation Workstation { get; set; }
    }
}