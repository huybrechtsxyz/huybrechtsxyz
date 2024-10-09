using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class CreateSetupTypeCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SetupType",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    TypeOf = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "The classification or type for the object (e.g., ProjectType, ProjectKind)."),
                    Name = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "The code or value representing this type (e.g., X00 - X01)."),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "A detailed description providing additional context or information about the type."),
                    SearchIndex = table.Column<string>(type: "nvarchar(450)", nullable: true, comment: "Stores normalized, concatenated values for efficient searching."),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true, comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupType", x => x.Id);
                },
                comment: "Defines various object types and codes within the setup configuration.");

            migrationBuilder.CreateIndex(
                name: "IX_SetupType_TenantId_SearchIndex",
                table: "SetupType",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupType_TenantId_TypeOf_Name",
                table: "SetupType",
                columns: new[] { "TenantId", "TypeOf", "Name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetupType");
        }
    }
}
