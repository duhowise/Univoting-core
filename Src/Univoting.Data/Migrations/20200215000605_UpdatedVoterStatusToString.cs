using Microsoft.EntityFrameworkCore.Migrations;

namespace Univoting.Data.Migrations
{
    public partial class UpdatedVoterStatusToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "VotingStatus",
                table: "Voters",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "VotingStatus",
                table: "Voters",
                type: "int",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
