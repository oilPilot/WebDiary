using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDiary.Migrations
{
    /// <inheritdoc />
    public partial class EmailsDeleted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "users");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
