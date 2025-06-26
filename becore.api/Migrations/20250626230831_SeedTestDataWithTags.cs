using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace becore.api.Migrations
{
    /// <inheritdoc />
    public partial class SeedTestDataWithTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Page",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Page");
        }
    }
}
