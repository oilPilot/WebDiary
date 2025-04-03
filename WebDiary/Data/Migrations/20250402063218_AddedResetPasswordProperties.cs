using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebDiary.Migrations
{
    /// <inheritdoc />
    public partial class AddedResetPasswordProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordDateEnd",
                table: "users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "ResetPasswordToken",
                table: "users",
                type: "varbinary(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetPasswordDateEnd",
                table: "users");

            migrationBuilder.DropColumn(
                name: "ResetPasswordToken",
                table: "users");
        }
    }
}
