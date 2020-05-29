using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JPB.BorannRemapping.Server.Data.AppContext
{
    public partial class System
    {
        public System()
        {
            SystemBody = new HashSet<SystemBody>();
        }

        [Key]
        public long SystemId { get; set; }
        [Required]
        public string Name { get; set; }
        public int ExtRelId { get; set; }
        public long ExtRelId64 { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public int? Explored { get; set; }
        public double? Certainty { get; set; }

        [InverseProperty("IdSystemNavigation")]
        public virtual ICollection<SystemBody> SystemBody { get; set; }
    }
}
