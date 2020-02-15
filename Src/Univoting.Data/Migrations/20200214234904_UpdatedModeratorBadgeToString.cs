using Microsoft.EntityFrameworkCore.Migrations;

namespace Univoting.Data.Migrations
{
    public partial class UpdatedModeratorBadgeToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Badge",
                table: "Moderators",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Badge",
                table: "Moderators",
                type: "int",
                nullable: false,
                oldClrType: typeof(string));
        }
    }
}
