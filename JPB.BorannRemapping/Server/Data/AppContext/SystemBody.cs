using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JPB.BorannRemapping.Server.Data.AppContext
{
    public partial class SystemBody
    {
        public SystemBody()
        {
            SystemBodyRing = new HashSet<SystemBodyRing>();
            SystemEval = new HashSet<SystemEval>();
        }

        [Key]
        public long SystemBodyId { get; set; }
        [Required]
        public string Name { get; set; }
        public int ExtRelId { get; set; }
        public long ExtRelId64 { get; set; }
        public string BodyId { get; set; }
        [Required]
        public string Type { get; set; }
        [Required]
        public string SubType { get; set; }
        public long DistanceToArrival { get; set; }
        public long IdSystem { get; set; }

        [ForeignKey(nameof(IdSystem))]
        [InverseProperty(nameof(System.SystemBody))]
        public virtual System IdSystemNavigation { get; set; }
        [InverseProperty("IdSystemBodyNavigation")]
        public virtual ICollection<SystemBodyRing> SystemBodyRing { get; set; }
        [InverseProperty("IdSystemBodyNavigation")]
        public virtual ICollection<SystemEval> SystemEval { get; set; }
    }
}
