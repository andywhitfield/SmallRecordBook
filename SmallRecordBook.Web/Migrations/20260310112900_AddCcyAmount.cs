using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmallRecordBook.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddCcyAmount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Amount",
                table: "RecordEntries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "RecordEntries",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "RecordEntries");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "RecordEntries");
        }
    }
}
