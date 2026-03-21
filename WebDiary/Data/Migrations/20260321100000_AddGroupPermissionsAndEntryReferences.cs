using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace WebDiary.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupPermissionsAndEntryReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create GroupPermission table
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
                        name: "FK_groupPermissions_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_groupPermissions_users_GrantedByUserId",
                        column: x => x.GrantedByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create EntryReference table
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

            // Create index on groupPermissions to enforce uniqueness
            migrationBuilder.CreateIndex(
                name: "IX_groupPermissions_GroupId_UserId",
                table: "groupPermissions",
                columns: new[] { "GroupId", "UserId" },
                unique: true);

            // Create foreign key index on groupPermissions.GrantedByUserId
            migrationBuilder.CreateIndex(
                name: "IX_groupPermissions_GrantedByUserId",
                table: "groupPermissions",
                column: "GrantedByUserId");

            // Create foreign key index on groupPermissions.UserId  
            migrationBuilder.CreateIndex(
                name: "IX_groupPermissions_UserId",
                table: "groupPermissions",
                column: "UserId");

            // Create indexes for entryReferences
            migrationBuilder.CreateIndex(
                name: "IX_entryReferences_ReferencedEntryId",
                table: "entryReferences",
                column: "ReferencedEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_entryReferences_SourceEntryId",
                table: "entryReferences",
                column: "SourceEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entryReferences");

            migrationBuilder.DropTable(
                name: "groupPermissions");
        }
    }
}
