using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace becore.api.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePageIconsToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert text columns to UUID with proper casting
            migrationBuilder.Sql(@"
                ALTER TABLE ""Page"" 
                ALTER COLUMN ""WideIcon"" TYPE uuid USING (
                    CASE 
                        WHEN ""WideIcon"" ~ '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$' 
                        THEN ""WideIcon""::uuid 
                        ELSE NULL 
                    END
                )
            ");

            migrationBuilder.Sql(@"
                ALTER TABLE ""Page"" 
                ALTER COLUMN ""QuadIcon"" TYPE uuid USING (
                    CASE 
                        WHEN ""QuadIcon"" ~ '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$' 
                        THEN ""QuadIcon""::uuid 
                        ELSE NULL 
                    END
                )
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "WideIcon",
                table: "Page",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "QuadIcon",
                table: "Page",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
