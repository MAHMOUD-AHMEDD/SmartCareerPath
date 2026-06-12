using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCareerPath.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameMessageTimestampColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Messages",
                newName: "message_timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "message_timestamp",
                table: "Messages",
                newName: "Timestamp");
        }
    }
}
