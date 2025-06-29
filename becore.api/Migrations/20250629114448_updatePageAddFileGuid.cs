using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace becore.api.Migrations
{
    /// <inheritdoc />
    public partial class updatePageAddFileGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "File",
                table: "Page",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "File",
                table: "Page");
        }
    }
}
