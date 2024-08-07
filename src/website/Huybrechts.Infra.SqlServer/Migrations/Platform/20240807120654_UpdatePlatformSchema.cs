using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Platform
{
    /// <inheritdoc />
    public partial class UpdatePlatformSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AlterTable(
                name: "Platform",
                comment: "Platforms that provide compute resources");

            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "Platform",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Remark about the platform",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Remark");

            migrationBuilder.AlterColumn<int>(
                name: "Provider",
                table: "Platform",
                type: "int",
                nullable: false,
                comment: "Supported automation providers of the platform",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "What is the supported provider for this platform");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Platform",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                comment: "Name of the platform",
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldComment: "Name");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Platform",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                comment: "Description of the platform",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true,
                oldComment: "Description");

            migrationBuilder.CreateTable(
                name: "PlatformRegion",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformInfo FK"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Name of the region"),
                    Label = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Label of the region"),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Description of the region"),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Remark about the region"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformRegion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformRegion_Platform_PlatformInfoId",
                        column: x => x.PlatformInfoId,
                        principalTable: "Platform",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Support regions of the Platform");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRegion_PlatformInfoId",
                table: "PlatformRegion",
                column: "PlatformInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformRegion");

            migrationBuilder.AlterTable(
                name: "Platform",
                oldComment: "Platforms that provide compute resources");

            migrationBuilder.AlterColumn<string>(
                name: "Remark",
                table: "Platform",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Remark",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Remark about the platform");

            migrationBuilder.AlterColumn<int>(
                name: "Provider",
                table: "Platform",
                type: "int",
                nullable: false,
                comment: "What is the supported provider for this platform",
                oldClrType: typeof(int),
                oldType: "int",
                oldComment: "Supported automation providers of the platform");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Platform",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                comment: "Name",
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldComment: "Name of the platform");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Platform",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true,
                comment: "Description",
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256,
                oldNullable: true,
                oldComment: "Description of the platform");

            migrationBuilder.CreateTable(
                name: "PlatformLocation",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Description"),
                    Label = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Label"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Name"),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Remark"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformLocation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformMeasure",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Name"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformMeasure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformMeasureDefault",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    DefaultValue = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false, comment: "Default unit rate"),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Measuring unit description"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    PlatformMeasureUnitId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformMeasureUnit FK"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UnitFactor = table.Column<decimal>(type: "decimal(12,6)", precision: 12, scale: 6, nullable: false, comment: "Conversion factor"),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, comment: "Unit of measure")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformMeasureDefault", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformSearchRate",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformSearchRate PK"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Service category"),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, comment: "Currency code"),
                    IsPrimaryRegion = table.Column<bool>(type: "bit", nullable: true, comment: "Is primary meter region"),
                    Location = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "Location name"),
                    MeterId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "Meter id"),
                    MeterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Meter name"),
                    MiminumUnits = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false, comment: "Tier miminum units"),
                    PlatformProviderId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformProvider FK"),
                    Product = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Product"),
                    ProductId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "Product id"),
                    Region = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "Region"),
                    RetailPrice = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false, comment: "Retail price"),
                    Service = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Service name"),
                    ServiceId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "Service id"),
                    Sku = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Sku name"),
                    SkuId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, comment: "Sku id"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Rate type"),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true, comment: "Azure rate unit of measure"),
                    UnitPrice = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false, comment: "Unit price"),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Rate is valid from")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSearchRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformService",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    Allowed = table.Column<bool>(type: "bit", nullable: false, comment: "Is the service allowed?"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Service Category"),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Description"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Name"),
                    PlatformInfoId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformInfo FK"),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Remark"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
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
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    AboutURL = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, comment: "Resource url"),
                    CostBasedOn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "Cost based on"),
                    CostDriver = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Cost driver"),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Description"),
                    Limitations = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, comment: "Resource limitations"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Name"),
                    PlatformInfoId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformInfo FK"),
                    PlatformServiceId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformService FK"),
                    PricingURL = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, comment: "Pricing url"),
                    Product = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Product"),
                    ProductId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Product id"),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Remarks"),
                    Size = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "Resource size"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
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
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, comment: "Currency code"),
                    IsPrimaryRegion = table.Column<bool>(type: "bit", nullable: true, comment: "Is primary meter region"),
                    MeterId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Meter id"),
                    MeterName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "Meter name"),
                    MininumUnits = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false, comment: "Tier mininum units"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Name"),
                    PlatformInfoId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformInfo FK"),
                    PlatformLocationId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformLocation FK"),
                    PlatformResourceId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformResource FK"),
                    PlatformServiceId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformService FK"),
                    Product = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "Product"),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Remark"),
                    RetailPrice = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false, comment: "Retail price"),
                    Sku = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "Sku name"),
                    SkuId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Sku id"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "Rate type"),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true, comment: "Azure rate unit of measure"),
                    UnitPrice = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false, comment: "Unit price"),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Rate is valid from")
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
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    DefaultValue = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false, comment: "Default unit rate"),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false, comment: "Measuring unit description"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    PlatformInfoId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformInfo FK"),
                    PlatformMeasureUnitId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformMeasureUnit FK"),
                    PlatformRateId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformRate FK"),
                    PlatformResourceId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformResource FK"),
                    PlatformServiceId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformService FK"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false),
                    UnitFactor = table.Column<decimal>(type: "decimal(12,6)", precision: 12, scale: 6, nullable: false, comment: "Conversion factor"),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, comment: "Unit of measure")
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
    }
}
