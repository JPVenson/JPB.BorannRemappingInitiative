using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JPB.BorannRemapping.Server.Migrations
{
	public partial class ExpandSubmissionData : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<string>(table: "SystemEval", name: "ProveImage", nullable: true);

		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
