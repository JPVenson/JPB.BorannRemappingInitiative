using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JPB.BorannRemapping.Server.Data.AppContext
{
    public partial class SystemEval
    {
        [Key]
        public long SubmissionId { get; set; }
        public bool State { get; set; }
        public string Comment { get; set; }
        public long IdSystemBody { get; set; }
        [Required]
        [StringLength(450)]
        public string IdSubmittingUser { get; set; }
        public DateTimeOffset DateOfCreation { get; set; }
        public string ProveImage { get; set; }
        [Required]
        public bool? IsReviewed { get; set; }
        public string ReviewComment { get; set; }

        [ForeignKey(nameof(IdSubmittingUser))]
        [InverseProperty(nameof(AspNetUsers.SystemEval))]
        public virtual AspNetUsers IdSubmittingUserNavigation { get; set; }
        [ForeignKey(nameof(IdSystemBody))]
        [InverseProperty(nameof(SystemBody.SystemEval))]
        public virtual SystemBody IdSystemBodyNavigation { get; set; }
    }
}
