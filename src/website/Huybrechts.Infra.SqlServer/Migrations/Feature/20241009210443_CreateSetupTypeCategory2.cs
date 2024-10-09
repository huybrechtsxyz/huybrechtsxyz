using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class CreateSetupTypeCategory2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SetupCategory",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    TypeOf = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "The classification or type for the object (e.g., ProjectCategory)."),
                    Category = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "The primary category assigned to the object."),
                    Subcategory = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "The subcategory assigned to the object, dependent on the selected category."),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Additional details or context for the category or subcategory."),
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
                    table.PrimaryKey("PK_SetupCategory", x => x.Id);
                },
                comment: "Defines categories and subcategories for objects in the setup configuration.");

            migrationBuilder.CreateIndex(
                name: "IX_SetupCategory_TenantId_SearchIndex",
                table: "SetupCategory",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupCategory_TenantId_TypeOf_Category_Subcategory",
                table: "SetupCategory",
                columns: new[] { "TenantId", "TypeOf", "Category", "Subcategory" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetupCategory");
        }
    }
}
