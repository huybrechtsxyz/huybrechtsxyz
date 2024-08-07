using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Sqlite.Migrations.Platform
{
    /// <inheritdoc />
    public partial class CreatePlatformSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Platform",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Primary Key"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Name"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Description"),
                    Provider = table.Column<int>(type: "INTEGER", nullable: false, comment: "What is the supported provider for this platform"),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Remark"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformLocation",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Primary Key"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Name"),
                    Label = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Label"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Description"),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Remark"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformLocation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformMeasure",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Primary Key"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Name"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformMeasure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformMeasureDefault",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Primary Key"),
                    PlatformMeasureUnitId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformMeasureUnit FK"),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, comment: "Unit of measure"),
                    UnitFactor = table.Column<decimal>(type: "TEXT", precision: 12, scale: 6, nullable: false, comment: "Conversion factor"),
                    DefaultValue = table.Column<decimal>(type: "TEXT", precision: 12, scale: 4, nullable: false, comment: "Default unit rate"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, comment: "Measuring unit description"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformMeasureDefault", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformSearchRate",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformSearchRate PK"),
                    PlatformProviderId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformProvider FK"),
                    ValidFrom = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Rate is valid from"),
                    ServiceId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, comment: "Service id"),
                    Service = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, comment: "Service name"),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, comment: "Service category"),
                    Region = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, comment: "Region"),
                    Location = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, comment: "Location name"),
                    CurrencyCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true, comment: "Currency code"),
                    RetailPrice = table.Column<decimal>(type: "TEXT", precision: 12, scale: 4, nullable: false, comment: "Retail price"),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 12, scale: 4, nullable: false, comment: "Unit price"),
                    MiminumUnits = table.Column<decimal>(type: "TEXT", precision: 12, scale: 4, nullable: false, comment: "Tier miminum units"),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true, comment: "Azure rate unit of measure"),
                    ProductId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, comment: "Product id"),
                    Product = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, comment: "Product"),
                    MeterId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, comment: "Meter id"),
                    MeterName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, comment: "Meter name"),
                    SkuId = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true, comment: "Sku id"),
                    Sku = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, comment: "Sku name"),
                    Type = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, comment: "Rate type"),
                    IsPrimaryRegion = table.Column<bool>(type: "INTEGER", nullable: true, comment: "Is primary meter region"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSearchRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformService",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformInfo FK"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Name"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Description"),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Remark"),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true, comment: "Service Category"),
                    Allowed = table.Column<bool>(type: "INTEGER", nullable: false, comment: "Is the service allowed?"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformService_Platform_PlatformInfoId",
                        column: x => x.PlatformInfoId,
                        principalTable: "Platform",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatformResource",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformInfo FK"),
                    PlatformServiceId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformService FK"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Name"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Description"),
                    CostDriver = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Cost driver"),
                    CostBasedOn = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Cost based on"),
                    Limitations = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true, comment: "Resource limitations"),
                    AboutURL = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true, comment: "Resource url"),
                    PricingURL = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true, comment: "Pricing url"),
                    ProductId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, comment: "Product id"),
                    Product = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, comment: "Product"),
                    Size = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Resource size"),
                    Remarks = table.Column<string>(type: "TEXT", nullable: true, comment: "Remarks"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformResource", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformResource_PlatformService_PlatformServiceId",
                        column: x => x.PlatformServiceId,
                        principalTable: "PlatformService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatformRate",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformInfo FK"),
                    PlatformServiceId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformService FK"),
                    PlatformResourceId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformResource FK"),
                    PlatformLocationId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformLocation FK"),
                    CurrencyCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, comment: "Currency code"),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Name"),
                    ValidFrom = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Rate is valid from"),
                    RetailPrice = table.Column<decimal>(type: "TEXT", precision: 12, scale: 4, nullable: false, comment: "Retail price"),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 12, scale: 4, nullable: false, comment: "Unit price"),
                    MininumUnits = table.Column<decimal>(type: "TEXT", precision: 12, scale: 4, nullable: false, comment: "Tier mininum units"),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true, comment: "Azure rate unit of measure"),
                    Product = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Product"),
                    MeterId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, comment: "Meter id"),
                    MeterName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Meter name"),
                    SkuId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, comment: "Sku id"),
                    Sku = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Sku name"),
                    Type = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Rate type"),
                    IsPrimaryRegion = table.Column<bool>(type: "INTEGER", nullable: true, comment: "Is primary meter region"),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Remark"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformRate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformRate_PlatformResource_PlatformResourceId",
                        column: x => x.PlatformResourceId,
                        principalTable: "PlatformResource",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlatformRateUnit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformInfo FK"),
                    PlatformServiceId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformService FK"),
                    PlatformResourceId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformResource FK"),
                    PlatformRateId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformRate FK"),
                    PlatformMeasureUnitId = table.Column<string>(type: "TEXT", nullable: false, comment: "PlatformMeasureUnit FK"),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", maxLength: 30, nullable: false, comment: "Unit of measure"),
                    UnitFactor = table.Column<decimal>(type: "TEXT", precision: 12, scale: 6, nullable: false, comment: "Conversion factor"),
                    DefaultValue = table.Column<decimal>(type: "TEXT", precision: 12, scale: 4, nullable: false, comment: "Default unit rate"),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, comment: "Measuring unit description"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformRateUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformRateUnit_PlatformRate_PlatformRateId",
                        column: x => x.PlatformRateId,
                        principalTable: "PlatformRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRate_PlatformResourceId",
                table: "PlatformRate",
                column: "PlatformResourceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRateUnit_PlatformRateId",
                table: "PlatformRateUnit",
                column: "PlatformRateId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformResource_PlatformServiceId",
                table: "PlatformResource",
                column: "PlatformServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformService_PlatformInfoId",
                table: "PlatformService",
                column: "PlatformInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformLocation");

            migrationBuilder.DropTable(
                name: "PlatformMeasure");

            migrationBuilder.DropTable(
                name: "PlatformMeasureDefault");

            migrationBuilder.DropTable(
                name: "PlatformRateUnit");

            migrationBuilder.DropTable(
                name: "PlatformSearchRate");

            migrationBuilder.DropTable(
                name: "PlatformRate");

            migrationBuilder.DropTable(
                name: "PlatformResource");

            migrationBuilder.DropTable(
                name: "PlatformService");

            migrationBuilder.DropTable(
                name: "Platform");
        }
    }
}
