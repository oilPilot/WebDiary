using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDiary.Migrations
{
    /// <inheritdoc />
    public partial class AddedValidationProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResetPasswordToken",
                table: "users",
                newName: "ActionToken");

            migrationBuilder.RenameColumn(
                name: "ResetPasswordDateEnd",
                table: "users",
                newName: "ActionDateEnd");

            migrationBuilder.AddColumn<bool>(
                name: "IsValidated",
                table: "users",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsValidated",
                table: "users");

            migrationBuilder.RenameColumn(
                name: "ActionToken",
                table: "users",
                newName: "ResetPasswordToken");

            migrationBuilder.RenameColumn(
                name: "ActionDateEnd",
                table: "users",
                newName: "ResetPasswordDateEnd");
        }
    }
}
