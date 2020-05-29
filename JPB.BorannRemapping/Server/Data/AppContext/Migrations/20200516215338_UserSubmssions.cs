using System;
using JPB.BorannRemapping.Server.Data.AppContext;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JPB.BorannRemapping.Server.Migrations
{
	public partial class UserSubmssions : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			 migrationBuilder.AddColumn<double>(
				name: "Certainty",
				table: "System",
				nullable: true);

			migrationBuilder.AddColumn<int>(
				name: "Explored",
				table: "System",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "CommanderName",
				table: "AspNetUsers",
				nullable: false,
				defaultValue: "");

			migrationBuilder.CreateTable(
				name: "SystemEval",
				columns: table => new
				{
					SubmissionId = table.Column<long>(nullable: false)
						.Annotation("SqlServer:Identity", "1, 1"),
					State = table.Column<bool>(nullable: false),
					Comment = table.Column<string>(nullable: true),
					IdSystemBody = table.Column<long>(nullable: false),
					IdSubmittingUser = table.Column<string>(maxLength: 450, nullable: false),
					DateOfCreation = table.Column<DateTimeOffset>(nullable: false)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_SystemEvalId", x => x.SubmissionId);
					table.ForeignKey(
						name: "FK_SystemEval_AspNetUsers",
						column: x => x.IdSubmittingUser,
						principalTable: "AspNetUsers",
						principalColumn: "Id",
						onDelete: ReferentialAction.Restrict);
					table.ForeignKey(
						name: "FK_SystemEval_SystemBody",
						column: x => x.IdSystemBody,
						principalTable: "SystemBody",
						principalColumn: "SystemBodyId",
						onDelete: ReferentialAction.Restrict);
				});

			migrationBuilder.CreateIndex(
				name: "IX_SystemEval_IdSubmittingUser",
				table: "SystemEval",
				column: "IdSubmittingUser");

			migrationBuilder.CreateIndex(
				name: "IX_SystemEval_IdSystemBody",
				table: "SystemEval",
				column: "IdSystemBody");

			//migrationBuilder.CreateTable("SystemEval", table => new
			//{
			//	SubmissionId = table.Column<long>()
			//		.Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
			//	State = table.Column<bool>(),
			//	Comment = table.Column<string>(nullable: true),
			//	IdSystemBody = table.Column<long>(),
			//	IdSubmittingUser = table.Column<string>(nullable: false, maxLength: 450),
			//	DateOfCreation = table.Column<DateTimeOffset>()
			//}, constraints: table =>
			//{
			//	table.PrimaryKey("PK_SystemEvalId", f => f.SubmissionId);
			//	table.ForeignKey("FK_SystemEval_SystemBody", f => f.IdSystemBody, "SystemBody", "SystemBodyId");
			//	table.ForeignKey("FK_SystemEval_AspNetUsers", f => f.IdSubmittingUser, "AspNetUsers", "Id");
			//});

			//migrationBuilder.AddColumn<Data.AppContext.System>("Explored", "System", type: "int", nullable: true);
			//migrationBuilder.AddColumn<Data.AppContext.System>("Certainty", "System", type: "float", nullable: true);
			//migrationBuilder.AddColumn<AspNetUsers>("CommanderName", "AspNetUsers", type: "NVARCHAR(MAX)");
		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "SystemEval");

			migrationBuilder.DropColumn(
				name: "Certainty",
				table: "System");

			migrationBuilder.DropColumn(
				name: "Explored",
				table: "System");

			migrationBuilder.DropColumn(
				name: "CommanderName",
				table: "AspNetUsers");
		}
	}
}
