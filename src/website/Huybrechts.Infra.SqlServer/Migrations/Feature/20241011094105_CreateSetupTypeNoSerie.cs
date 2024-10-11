using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class CreateSetupTypeNoSerie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SetupState_TenantId_ObjectType_Name",
                table: "SetupState");

            migrationBuilder.DropColumn(
                name: "ObjectType",
                table: "SetupState");

            migrationBuilder.AddColumn<string>(
                name: "TypeOf",
                table: "SetupState",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "The classification or type for the object (e.g., ProjectType, ProjectKind).");

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

            migrationBuilder.CreateTable(
                name: "SetupNoSerie",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    TypeOf = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "Type of number series, such as ProjectNumber or InvoiceNumber."),
                    TypeValue = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "Specific value within the type code, such as 'Sales' or 'Development' for project types."),
                    Format = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Format string defining how the number is generated, including placeholders like YYYY-MM-###."),
                    StartCounter = table.Column<int>(type: "int", nullable: false, comment: "The maximum allowed value for the counter before it resets or stops."),
                    Increment = table.Column<int>(type: "int", nullable: false, comment: "Specifies the increment value for the counter when generating a new number."),
                    Maximum = table.Column<int>(type: "int", nullable: false, comment: "The maximum allowed value for the counter before it resets or stops."),
                    AutomaticReset = table.Column<bool>(type: "bit", nullable: false, comment: "Indicates whether the counter will automatically reset when the number series changes (e.g., at the start of a new year)."),
                    LastCounter = table.Column<int>(type: "int", nullable: false, comment: "The current sequential number for this series."),
                    LastPrefix = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "Stores the last generated prefix in the series to track the most recent value."),
                    LastValue = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "Stores the last generated number in the series to track the most recent value."),
                    IsDisabled = table.Column<bool>(type: "bit", nullable: false, comment: "Indicates whether the counter is disabled. If true, the counter is inactive."),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Optional description providing more details about the number series."),
                    SearchIndex = table.Column<string>(type: "nvarchar(450)", nullable: true, comment: "A normalized concatenated field used for optimizing search operations."),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true, comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupNoSerie", x => x.Id);
                },
                comment: "Stores configuration for number series, supporting multi-tenancy.");

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
                name: "IX_SetupState_TenantId_TypeOf_Name",
                table: "SetupState",
                columns: new[] { "TenantId", "TypeOf", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupCategory_TenantId_SearchIndex",
                table: "SetupCategory",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupCategory_TenantId_TypeOf_Category_Subcategory",
                table: "SetupCategory",
                columns: new[] { "TenantId", "TypeOf", "Category", "Subcategory" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupNoSerie_TenantId_SearchIndex",
                table: "SetupNoSerie",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupNoSerie_TenantId_TypeOf_TypeValue",
                table: "SetupNoSerie",
                columns: new[] { "TenantId", "TypeOf", "TypeValue" });

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
                name: "SetupCategory");

            migrationBuilder.DropTable(
                name: "SetupNoSerie");

            migrationBuilder.DropTable(
                name: "SetupType");

            migrationBuilder.DropIndex(
                name: "IX_SetupState_TenantId_TypeOf_Name",
                table: "SetupState");

            migrationBuilder.DropColumn(
                name: "TypeOf",
                table: "SetupState");

            migrationBuilder.AddColumn<int>(
                name: "ObjectType",
                table: "SetupState",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SetupState_TenantId_ObjectType_Name",
                table: "SetupState",
                columns: new[] { "TenantId", "ObjectType", "Name" },
                unique: true);
        }
    }
}
