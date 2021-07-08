using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HES.Core.Services;

namespace HES.Core.Entities
{
    public class Workstation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string Name { get; set; }

        public string Domain { get; set; }

        public string ClientVersion { get; set; }

        [Display(Name = nameof(Resources.Resource.Display_Department), ResourceType = typeof(Resources.Resource))]
        public string DepartmentId { get; set; }

        public string OS { get; set; }

        public string IP { get; set; }

        public DateTime LastSeen { get; set; }

        public bool Approved { get; set; }

        public bool RFID { get; set; }


        public List<WorkstationHardwareVaultPair> WorkstationProximityVaults { get; set; }
        public List<WorkstationEvent> WorkstationEvents { get; set; }
        public List<WorkstationSession> WorkstationSessions { get; set; }


        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }

        [NotMapped]
        public bool IsOnline => Id != null ? RemoteWorkstationConnectionsService.IsWorkstationConnected(Id) : false;
    }
}