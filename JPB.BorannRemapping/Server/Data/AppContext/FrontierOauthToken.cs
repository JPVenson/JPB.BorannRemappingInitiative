using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JPB.BorannRemapping.Server.Data.AppContext
{
    [Table("FrontierOAuthToken")]
    public partial class FrontierOauthToken
    {
        [Key]
        public long FrontierId { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
        public DateTimeOffset TokenValidUntil { get; set; }
        [Required]
        [StringLength(450)]
        public string IdUser { get; set; }

        [ForeignKey(nameof(IdUser))]
        [InverseProperty(nameof(AspNetUsers.FrontierOauthToken))]
        public virtual AspNetUsers IdUserNavigation { get; set; }
    }
}
