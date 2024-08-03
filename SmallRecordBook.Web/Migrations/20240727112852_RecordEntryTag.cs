using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmallRecordBook.Web.Migrations
{
    /// <inheritdoc />
    public partial class RecordEntryTag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecordEntryTags",
                columns: table => new
                {
                    RecordEntryTagId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RecordEntryId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tag = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordEntryTags", x => x.RecordEntryTagId);
                    table.ForeignKey(
                        name: "FK_RecordEntryTags_RecordEntries_RecordEntryId",
                        column: x => x.RecordEntryId,
                        principalTable: "RecordEntries",
                        principalColumn: "RecordEntryId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecordEntryTags_RecordEntryId",
                table: "RecordEntryTags",
                column: "RecordEntryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecordEntryTags");
        }
    }
}
