using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JPB.BorannRemapping.Server.Migrations
{
	public partial class AddOAuthTokens : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.CreateTable("FrontierOAuthToken", columns: table => new
			{
				FrontierId = table.Column<long>(nullable: false)
					.Annotation("SqlServer:Identity", "1, 1"),
				Token = table.Column<string>(),
				RefreshToken = table.Column<string>(),
				TokenValidUntil = table.Column<DateTimeOffset>(),
				IdUser = table.Column<string>(maxLength: 450, nullable: false)
			}, constraints: table =>
			{
				table.PrimaryKey("PK_FrontierId", x => x.FrontierId);
				table.ForeignKey(
					name: "FK_FrontierOAuthToken_AspNetUsers",
					column: x => x.IdUser,
					principalTable: "AspNetUsers",
					principalColumn: "Id",
					onDelete: ReferentialAction.Restrict);
			});

		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
