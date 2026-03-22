using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebDiary.Migrations
{
    /// <inheritdoc />
    public partial class AddTagsToDiariesAndMuchMore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "diaryGroups",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "diaryGroups",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OwnerId",
                table: "diaries",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Tags",
                table: "diaries",
                type: "text[]",
                nullable: true);

            migrationBuilder.Sql("""
            UPDATE "diaries" AS d
            SET "OwnerId" = g."UserId"
            FROM "diaryGroups" AS g
            WHERE d."GroupId" = g."Id"
              AND d."OwnerId" IS NULL;
            """);

            migrationBuilder.AlterColumn<int>(
                name: "OwnerId",
                table: "diaries",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "entryReferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SourceEntryId = table.Column<int>(type: "integer", nullable: false),
                    ReferencedEntryId = table.Column<int>(type: "integer", nullable: false),
                    ExcerptText = table.Column<string>(type: "text", nullable: true),
                    ExcerptStartIndex = table.Column<int>(type: "integer", nullable: true),
                    ExcerptEndIndex = table.Column<int>(type: "integer", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entryReferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_entryReferences_diaries_ReferencedEntryId",
                        column: x => x.ReferencedEntryId,
                        principalTable: "diaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_entryReferences_diaries_SourceEntryId",
                        column: x => x.SourceEntryId,
                        principalTable: "diaries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groupPermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GroupId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    GrantedByUserId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_groupPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_groupPermissions_diaryGroups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "diaryGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_groupPermissions_users_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_groupPermissions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_diaryGroups_UserId",
                table: "diaryGroups",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_diaries_OwnerId",
                table: "diaries",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_entryReferences_ReferencedEntryId",
                table: "entryReferences",
                column: "ReferencedEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_entryReferences_SourceEntryId",
                table: "entryReferences",
                column: "SourceEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_groupPermissions_GrantedByUserId",
                table: "groupPermissions",
                column: "GrantedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_groupPermissions_GroupId_UserId",
                table: "groupPermissions",
                columns: new[] { "GroupId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_groupPermissions_UserId",
                table: "groupPermissions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_diaries_users_OwnerId",
                table: "diaries",
                column: "OwnerId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_diaryGroups_users_UserId",
                table: "diaryGroups",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_diaries_users_OwnerId",
                table: "diaries");

            migrationBuilder.DropForeignKey(
                name: "FK_diaryGroups_users_UserId",
                table: "diaryGroups");

            migrationBuilder.DropTable(
                name: "entryReferences");

            migrationBuilder.DropTable(
                name: "groupPermissions");

            migrationBuilder.DropIndex(
                name: "IX_diaryGroups_UserId",
                table: "diaryGroups");

            migrationBuilder.DropIndex(
                name: "IX_diaries_OwnerId",
                table: "diaries");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "diaryGroups");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "diaryGroups");

            migrationBuilder.DropColumn(
                name: "OwnerId",
                table: "diaries");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "diaries");
        }
    }
}
