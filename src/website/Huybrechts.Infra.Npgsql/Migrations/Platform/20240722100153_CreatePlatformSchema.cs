using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Huybrechts.Infra.Npgsql.Migrations.Platform
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
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "Platform PK")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Name"),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Description"),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Remark"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformLocation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "PlatformLocation PK")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Name"),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Label"),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Description"),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Remark"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformLocation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformMeasure",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "PlatformMeasure PK")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Name"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformMeasure", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformMeasureDefault",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "PlatformMeasureDefault PK")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlatformProviderId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformProvider FK"),
                    PlatformMeasureUnitId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformMeasureUnit FK"),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, comment: "Unit of measure"),
                    UnitFactor = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: false, comment: "Conversion factor"),
                    DefaultValue = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false, comment: "Default unit rate"),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Measuring unit description"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformMeasureDefault", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformSearchRate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "PlatformSearchRate PK")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlatformProviderId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformProvider FK"),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Rate is valid from"),
                    ServiceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Service id"),
                    Service = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Service name"),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Service category"),
                    Region = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Region"),
                    Location = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Location name"),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true, comment: "Currency code"),
                    RetailPrice = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false, comment: "Retail price"),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false, comment: "Unit price"),
                    MiminumUnits = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false, comment: "Tier miminum units"),
                    UnitOfMeasure = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true, comment: "Azure rate unit of measure"),
                    ProductId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Product id"),
                    Product = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Product"),
                    MeterId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Meter id"),
                    MeterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Meter name"),
                    SkuId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true, comment: "Sku id"),
                    Sku = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Sku name"),
                    Type = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Rate type"),
                    IsPrimaryRegion = table.Column<bool>(type: "boolean", nullable: true, comment: "Is primary meter region"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformSearchRate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlatformService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "PlatformService PK")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlatformProviderId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformProvider FK"),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Name"),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Description"),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Remark"),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Service Category"),
                    Allowed = table.Column<bool>(type: "boolean", nullable: false, comment: "Is the service allowed?"),
                    PlatformInfoId = table.Column<int>(type: "integer", nullable: true),
                    PlatformMeasureDefaultId = table.Column<int>(type: "integer", nullable: true),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformService_PlatformMeasureDefault_PlatformMeasureDefau~",
                        column: x => x.PlatformMeasureDefaultId,
                        principalTable: "PlatformMeasureDefault",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PlatformService_Platform_PlatformInfoId",
                        column: x => x.PlatformInfoId,
                        principalTable: "Platform",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlatformResource",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "PlatformResource PK")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlatformProviderId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformProvider FK"),
                    PlatformServiceId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformService FK"),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Name"),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Description"),
                    CostDriver = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Cost driver"),
                    CostBasedOn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Cost based on"),
                    Limitations = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: "Resource limitations"),
                    AboutURL = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: "Resource url"),
                    PricingURL = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: "Pricing url"),
                    ProductId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "Product id"),
                    Product = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "Product"),
                    Size = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Resource size"),
                    Remarks = table.Column<string>(type: "text", nullable: true, comment: "Remarks"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "PlatformRate PK")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlatformProviderId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformProvider FK"),
                    PlatformServiceId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformService FK"),
                    PlatformResourceId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformResource FK"),
                    PlatformLocationId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformLocation FK"),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Currency code"),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Name"),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Rate is valid from"),
                    RetailPrice = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false, comment: "Retail price"),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false, comment: "Unit price"),
                    MininumUnits = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false, comment: "Tier mininum units"),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true, comment: "Azure rate unit of measure"),
                    Product = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Product"),
                    MeterId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "Meter id"),
                    MeterName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Meter name"),
                    SkuId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "Sku id"),
                    Sku = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Sku name"),
                    Type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Rate type"),
                    IsPrimaryRegion = table.Column<bool>(type: "boolean", nullable: true, comment: "Is primary meter region"),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Remark"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false, comment: "PlatformRateUnit PK")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlatformProviderId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformProvider FK"),
                    PlatformServiceId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformService FK"),
                    PlatformResourceId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformResource FK"),
                    PlatformRateId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformRate FK"),
                    PlatformMeasureUnitId = table.Column<int>(type: "integer", nullable: false, comment: "PlatformMeasureUnit FK"),
                    UnitOfMeasure = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, comment: "Unit of measure"),
                    UnitFactor = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: false, comment: "Conversion factor"),
                    DefaultValue = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false, comment: "Default unit rate"),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Measuring unit description"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_PlatformService_PlatformMeasureDefaultId",
                table: "PlatformService",
                column: "PlatformMeasureDefaultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformLocation");

            migrationBuilder.DropTable(
                name: "PlatformMeasure");

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
                name: "PlatformMeasureDefault");

            migrationBuilder.DropTable(
                name: "Platform");
        }
    }
}
