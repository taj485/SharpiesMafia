using Microsoft.EntityFrameworkCore.Migrations;

namespace SharpiesMafia.Migrations
{
    public partial class addingVoteCountColumnAttempt2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "vote_count",
                table: "Users",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "vote_count",
                table: "Users");
        }
    }
}
