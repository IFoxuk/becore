using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace becore.api.Migrations
{
    /// <inheritdoc />
    public partial class CreatedPagesScheme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "ContentMaker",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    userId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentMaker", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContentMaker_AspNetUsers_userId",
                        column: x => x.userId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Team",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Team", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AddonPage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    QuadIcon = table.Column<Guid>(type: "uuid", nullable: true),
                    WideIcon = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    teamId = table.Column<Guid>(type: "uuid", nullable: true),
                    contentMakerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddonPage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddonPage_ContentMaker_contentMakerId",
                        column: x => x.contentMakerId,
                        principalTable: "ContentMaker",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_AddonPage_Team_teamId",
                        column: x => x.teamId,
                        principalTable: "Team",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TeamMember",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    teamId = table.Column<Guid>(type: "uuid", nullable: false),
                    position = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMember", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMember_ContentMaker_Id",
                        column: x => x.Id,
                        principalTable: "ContentMaker",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeamMember_Team_teamId",
                        column: x => x.teamId,
                        principalTable: "Team",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BehaviourPage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BehaviourPage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BehaviourPage_AddonPage_Id",
                        column: x => x.Id,
                        principalTable: "AddonPage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourcePage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    resolution = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourcePage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResourcePage_AddonPage_Id",
                        column: x => x.Id,
                        principalTable: "AddonPage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BehaviourFile",
                columns: table => new
                {
                    file = table.Column<Guid>(type: "uuid", nullable: false),
                    behaviourPageId = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    confirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BehaviourFile", x => new { x.file, x.behaviourPageId });
                    table.ForeignKey(
                        name: "FK_BehaviourFile_BehaviourPage_behaviourPageId",
                        column: x => x.behaviourPageId,
                        principalTable: "BehaviourPage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResourceFile",
                columns: table => new
                {
                    file = table.Column<Guid>(type: "uuid", nullable: false),
                    resourcePageId = table.Column<Guid>(type: "uuid", nullable: false),
                    uploaded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    confirmed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResourceFile", x => new { x.file, x.resourcePageId });
                    table.ForeignKey(
                        name: "FK_ResourceFile_ResourcePage_resourcePageId",
                        column: x => x.resourcePageId,
                        principalTable: "ResourcePage",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddonPage_contentMakerId",
                table: "AddonPage",
                column: "contentMakerId");

            migrationBuilder.CreateIndex(
                name: "IX_AddonPage_teamId",
                table: "AddonPage",
                column: "teamId");

            migrationBuilder.CreateIndex(
                name: "IX_BehaviourFile_behaviourPageId",
                table: "BehaviourFile",
                column: "behaviourPageId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentMaker_userId",
                table: "ContentMaker",
                column: "userId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ResourceFile_resourcePageId",
                table: "ResourceFile",
                column: "resourcePageId");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMember_teamId",
                table: "TeamMember",
                column: "teamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BehaviourFile");

            migrationBuilder.DropTable(
                name: "ResourceFile");

            migrationBuilder.DropTable(
                name: "TeamMember");

            migrationBuilder.DropTable(
                name: "BehaviourPage");

            migrationBuilder.DropTable(
                name: "ResourcePage");

            migrationBuilder.DropTable(
                name: "AddonPage");

            migrationBuilder.DropTable(
                name: "ContentMaker");

            migrationBuilder.DropTable(
                name: "Team");

            migrationBuilder.DropColumn(
                name: "File",
                table: "Page");
        }
    }
}
