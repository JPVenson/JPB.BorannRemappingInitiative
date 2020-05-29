using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using JPB.BorannRemapping.Server.Data.AppContext;
using JPB.BubbleSearch;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace JPB.BorannRemapping.Server.Migrations
{
	[DbContext(typeof(AppDbContext))]
	public partial class InitAppDb : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable("System", table => new
			{
				SystemId = table.Column<long>()
					.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
				Name = table.Column<string>(),
				ExtRelId = table.Column<int>(),
				ExtRelId64 = table.Column<long>(),
				X = table.Column<float>(),
				Y = table.Column<float>(),
				Z = table.Column<float>(),
			}, constraints: table =>
			{
				table.PrimaryKey("PK_SystemId", f => f.SystemId);
			});

			migrationBuilder.CreateTable("SystemBody", table => new
			{
				SystemBodyId = table.Column<long>()
					.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
				Name = table.Column<string>(),
				ExtRelId = table.Column<int>(),
				ExtRelId64 = table.Column<long>(),
				BodyId = table.Column<string>(nullable: true),
				Type = table.Column<string>(),
				SubType = table.Column<string>(),
				DistanceToArrival = table.Column<long>(),
				IdSystem = table.Column<long>(),
			}, constraints: table =>
			{
				table.PrimaryKey("PK_SystemBodyId", f => f.SystemBodyId);
				table.ForeignKey("FK_SystemBody_System", f => f.IdSystem, "System", "SystemId");
			});

			migrationBuilder.CreateTable("SystemBodyRing", table => new
			{
				SystemBodyRingId = table.Column<long>()
					.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
				Name = table.Column<string>(),
				Type = table.Column<string>(),
				IdSystemBody = table.Column<long>(),
			}, constraints: table =>
			{
				table.PrimaryKey("PK_SystemBodyRingId", f => f.SystemBodyRingId);
				table.ForeignKey("FK_SystemBodyRing_SystemBody", f => f.IdSystemBody, "SystemBody", "SystemBodyId");
			});
		}

		//public static IEnumerable<SystemModel> GetSystems()
		//{
		//	SystemBody[] bodies;
		//	using (var systemFs = new FileStream(@"C:\Users\Jean-\Desktop\systemsPopulated.json", FileMode.Open))
		//	{
		//		using (var systemStreamReader = new StreamReader(systemFs))
		//		{
		//			using (var jsonReader = new JsonTextReader(systemStreamReader))
		//			{
		//				var jsonSerializer = new JsonSerializer();
		//				return jsonSerializer.Deserialize<SystemModel[]>(jsonReader);
		//			}
		//		}
		//	}
		//}

		public static IEnumerable<SystemModel> GetTargetSystems()
		{
			SystemBody[] bodies;
			using (var systemFs = new FileStream(@"C:\Users\Jean-\Desktop\usefulSystems.json", FileMode.Open))
			{
				using (var systemStreamReader = new StreamReader(systemFs))
				{
					using (var jsonReader = new JsonTextReader(systemStreamReader))
					{
						var jsonSerializer = new JsonSerializer();
						return jsonSerializer.Deserialize<SystemModel[]>(jsonReader);
					}
				}
			}
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable("System");
			migrationBuilder.DropTable("SystemBody");
			migrationBuilder.DropTable("SystemBodyRing");
		}
	}
}
