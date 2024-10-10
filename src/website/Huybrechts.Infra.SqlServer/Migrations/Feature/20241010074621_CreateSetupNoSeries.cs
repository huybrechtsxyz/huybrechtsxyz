using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class CreateSetupNoSeries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SetupNoSerie",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    TypeOf = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "Type of number series, such as ProjectNumber or InvoiceNumber."),
                    TypeCode = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "The category on which the number series is based, such as ProjectType or InvoiceType."),
                    TypeValue = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "Specific value within the type code, such as 'Sales' or 'Development' for project types."),
                    Format = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Format string defining how the number is generated, including placeholders like YYYY-MM-###."),
                    Counter = table.Column<int>(type: "int", nullable: false, comment: "The current sequential number for this series."),
                    Maximum = table.Column<int>(type: "int", nullable: false, comment: "The maximum allowed value for the counter before it resets or stops."),
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

            migrationBuilder.CreateIndex(
                name: "IX_SetupNoSerie_TenantId_SearchIndex",
                table: "SetupNoSerie",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupNoSerie_TenantId_TypeOf_TypeCode_TypeValue",
                table: "SetupNoSerie",
                columns: new[] { "TenantId", "TypeOf", "TypeCode", "TypeValue" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetupNoSerie");
        }
    }
}
