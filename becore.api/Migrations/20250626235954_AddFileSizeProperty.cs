using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace becore.api.Migrations
{
    /// <inheritdoc />
    public partial class AddFileSizeProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "Size",
                table: "File",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "File");
        }
    }
}
