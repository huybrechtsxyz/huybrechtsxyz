using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Npgsql.Migrations.Feature
{
    /// <inheritdoc />
    public partial class CreateProjectDesign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Project",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                comment: "Keywords or categories for the project");

            migrationBuilder.CreateTable(
                name: "ProjectDesign",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    ProjectInfoId = table.Column<string>(type: "character varying(26)", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Additional remarks or comments about the project design."),
                    Tags = table.Column<string>(type: "text", nullable: true, comment: "Keywords or categories for the design"),
                    State = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, comment: "Gets or sets the current state of the project design."),
                    Reason = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Gets or sets the reason for the current state of the design."),
                    Environment = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "The environment in which the project design is implemented."),
                    Version = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true, comment: "Design version"),
                    Dependencies = table.Column<string>(type: "text", nullable: true, comment: "List of dependencies for the design"),
                    Priority = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true, comment: "Gets or sets the priority of the project."),
                    Risk = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true, comment: "Gets or sets the risk of the project."),
                    Rating = table.Column<int>(type: "integer", nullable: true, comment: "Gets or sets the rating of the project, reflecting its priority, quality, or stakeholder approval."),
                    SearchIndex = table.Column<string>(type: "text", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDesign", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDesign_Project_ProjectInfoId",
                        column: x => x.ProjectInfoId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents a specific design or solution proposal for a project.");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDesign_ProjectInfoId_Name",
                table: "ProjectDesign",
                columns: new[] { "ProjectInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDesign_SearchIndex",
                table: "ProjectDesign",
                column: "SearchIndex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectDesign");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Project");
        }
    }
}
