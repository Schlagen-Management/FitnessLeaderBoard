using Microsoft.EntityFrameworkCore.Migrations;

namespace FitnessLeaderBoard.Data.Migrations
{
    public partial class UpdatedLeaderboardInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AllTimeStepCount",
                schema: "flb",
                table: "Leaderboard",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Initials",
                schema: "flb",
                table: "Leaderboard",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllTimeStepCount",
                schema: "flb",
                table: "Leaderboard");

            migrationBuilder.DropColumn(
                name: "Initials",
                schema: "flb",
                table: "Leaderboard");
        }
    }
}
