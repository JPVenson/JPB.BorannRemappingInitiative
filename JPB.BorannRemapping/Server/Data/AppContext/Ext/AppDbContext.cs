using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace JPB.BorannRemapping.Server.Data.AppContext
{
	public partial class AppDbContext
	{
		partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
		{
		}
	}
}
