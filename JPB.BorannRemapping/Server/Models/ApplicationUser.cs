using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JPB.BorannRemapping.Server.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string CommanderName { get; set; }
	}
}
