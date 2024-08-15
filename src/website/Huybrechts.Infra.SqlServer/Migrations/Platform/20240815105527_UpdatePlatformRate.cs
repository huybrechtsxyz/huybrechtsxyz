using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Platform
{
    /// <inheritdoc />
    public partial class UpdatePlatformRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformRate",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Foreign key referencing the PlatformInfo entity."),
                    PlatformServiceId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Foreign key referencing the PlatformService entity."),
                    PlatformRegionId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Foreign key referencing the PlatformRegion entity."),
                    PlatformProductId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Foreign key referencing the PlatformProduct entity."),
                    ServiceName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "The name of the service."),
                    ServiceFamily = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Service family or category."),
                    ProductName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Product name."),
                    SkuName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "SKU name."),
                    MeterName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Meter name."),
                    RateType = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Rate type."),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "Currency code."),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Rate is valid from."),
                    RetailPrice = table.Column<decimal>(type: "decimal(12,6)", precision: 12, scale: 6, nullable: false, comment: "Retail price."),
                    UnitPrice = table.Column<decimal>(type: "decimal(12,6)", precision: 12, scale: 6, nullable: false, comment: "Unit price."),
                    MininumUnits = table.Column<decimal>(type: "decimal(12,6)", precision: 12, scale: 6, nullable: false, comment: "Tier minimum units."),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, comment: "Unit of measure."),
                    IsPrimaryRegion = table.Column<bool>(type: "bit", nullable: false, comment: "Indicates whether this is the primary rate for the region."),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Additional remarks or comments about the rate."),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformRate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformRate_PlatformService_PlatformServiceId",
                        column: x => x.PlatformServiceId,
                        principalTable: "PlatformService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRate_PlatformServiceId",
                table: "PlatformRate",
                column: "PlatformServiceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformRate");
        }
    }
}
