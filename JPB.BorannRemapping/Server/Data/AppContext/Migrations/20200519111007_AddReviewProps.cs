using Microsoft.EntityFrameworkCore.Migrations;

namespace JPB.BorannRemapping.Server.Migrations
{
	public partial class AddReviewProps : Migration
	{
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.AddColumn<bool>("IsReviewed", "SystemEval", defaultValue: false);
			migrationBuilder.AddColumn<string>("ReviewComment", "SystemEval", 
				nullable: true, 
				defaultValue: null);

		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
