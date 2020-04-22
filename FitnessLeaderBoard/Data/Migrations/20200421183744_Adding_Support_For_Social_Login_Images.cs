using Microsoft.EntityFrameworkCore.Migrations;

namespace FitnessLeaderBoard.Data.Migrations
{
    public partial class Adding_Support_For_Social_Login_Images : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageLink",
                schema: "flb",
                table: "Leaderboard",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImageLink",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageLink",
                schema: "flb",
                table: "Leaderboard");

            migrationBuilder.DropColumn(
                name: "ImageLink",
                table: "AspNetUsers");
        }
    }
}
