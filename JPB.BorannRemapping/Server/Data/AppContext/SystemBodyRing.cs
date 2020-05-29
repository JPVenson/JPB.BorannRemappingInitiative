using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JPB.BorannRemapping.Server.Data.AppContext
{
    public partial class SystemBodyRing
    {
        [Key]
        public long SystemBodyRingId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Type { get; set; }
        public long IdSystemBody { get; set; }

        [ForeignKey(nameof(IdSystemBody))]
        [InverseProperty(nameof(SystemBody.SystemBodyRing))]
        public virtual SystemBody IdSystemBodyNavigation { get; set; }
    }
}
