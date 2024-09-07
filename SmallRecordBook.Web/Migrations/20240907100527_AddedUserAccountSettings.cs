using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmallRecordBook.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserAccountSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAccountSettings",
                columns: table => new
                {
                    UserAccountSettingId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserAccountId = table.Column<int>(type: "INTEGER", nullable: false),
                    SettingName = table.Column<string>(type: "TEXT", nullable: false),
                    SettingValue = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastUpdateDateTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedDateTime = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAccountSettings", x => x.UserAccountSettingId);
                    table.ForeignKey(
                        name: "FK_UserAccountSettings_UserAccounts_UserAccountId",
                        column: x => x.UserAccountId,
                        principalTable: "UserAccounts",
                        principalColumn: "UserAccountId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAccountSettings_UserAccountId",
                table: "UserAccountSettings",
                column: "UserAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAccountSettings");
        }
    }
}
