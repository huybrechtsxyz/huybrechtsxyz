using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Npgsql.Migrations.Platform
{
    /// <inheritdoc />
    public partial class CreatePlatformSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.CreateTable(
                name: "Platform",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Name of the platform."),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Detailed description of the platform."),
                    Provider = table.Column<int>(type: "integer", nullable: false, comment: "The platform's supported automation provider, enabling automated resource management."),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Additional remarks or comments about the platform."),
                    SearchIndex = table.Column<string>(type: "text", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform", x => x.Id);
                },
                comment: "Table storing information about platforms that offer compute resources, including cloud providers like Azure or Google, and on-premise solutions.");

            migrationBuilder.CreateTable(
                name: "SetupUnit",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    UnitType = table.Column<int>(type: "integer", maxLength: 32, nullable: false, comment: "Gets or sets the type of the unit (e.g., Height, Weight, Volume, System, etc.)."),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "A unique code representing the unit, standard across all instances."),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "The unique name of the unit within its type."),
                    Description = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true, comment: "A detailed description of the unit."),
                    IsBase = table.Column<bool>(type: "boolean", nullable: false, comment: "Indicates whether this unit is the base unit for its type."),
                    Precision = table.Column<int>(type: "integer", nullable: false, comment: "Number of decimal places for the unit."),
                    PrecisionType = table.Column<int>(type: "integer", nullable: false, comment: "Determines how values are rounded according to the System.Decimal Rounding enum."),
                    Factor = table.Column<decimal>(type: "numeric(18,10)", precision: 18, scale: 10, nullable: false, comment: "A multiplication factor used to convert this unit to the base unit."),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Additional remarks or comments about the unit."),
                    SearchIndex = table.Column<string>(type: "text", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupUnit", x => x.Id);
                },
                comment: "Represents a measurement unit used for different types such as height, weight, volume, etc.");

            migrationBuilder.CreateTable(
                name: "PlatformProduct",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformInfo entity."),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "The name of the product offered by the platform."),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "A label representing the product, often used for display purposes."),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "A brief description providing details about the product."),
                    Category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Category of the product"),
                    CostDriver = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "The cost driver or factor that influences the pricing of the product."),
                    CostBasedOn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Parameters or metrics on which the cost of the product is based."),
                    Limitations = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: "Limitations or constraints related to the product."),
                    AboutURL = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: "URL linking to additional information about the product."),
                    PricingURL = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: "URL providing pricing information for the product."),
                    PricingTier = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Size or pricing tier associated with the product."),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Additional remarks or notes regarding the product."),
                    SearchIndex = table.Column<string>(type: "text", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformProduct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformProduct_Platform_PlatformInfoId",
                        column: x => x.PlatformInfoId,
                        principalTable: "Platform",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents a product offered on a specific platform, detailing attributes such as the product's name, description, and other relevant metadata.");

            migrationBuilder.CreateTable(
                name: "PlatformRegion",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformInfo."),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "The unique name identifier of the region."),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "A label representing the region, often the location name."),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "A brief description providing additional details about the region."),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Additional remarks or notes regarding the region."),
                    SearchIndex = table.Column<string>(type: "text", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
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
                comment: "Regions supported by the platform, representing data center locations.");

            migrationBuilder.CreateTable(
                name: "PlatformService",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformInfo entity."),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "The name of the service or service offered by the platform."),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "A label representing the service, often used in the user interface."),
                    Category = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "The category or family of the service, helping to classify it among similar offerings."),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "A brief description providing additional details about the service."),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Additional remarks or notes regarding the service."),
                    SearchIndex = table.Column<string>(type: "text", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
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
                },
                comment: "Services offered by the platform, such as compute, storage, or networking resources.");

            migrationBuilder.CreateTable(
                name: "PlatformRate",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformInfo entity."),
                    PlatformProductId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformProduct entity."),
                    PlatformRegionId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformRegion entity."),
                    PlatformServiceId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformService entity."),
                    ServiceName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "The name of the service."),
                    ServiceFamily = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Service family or category."),
                    ProductName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Product name."),
                    SkuName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "SKU name."),
                    MeterName = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Meter name."),
                    RateType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Rate type."),
                    CurrencyCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, comment: "Currency code."),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Rate is valid from."),
                    RetailPrice = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: false, comment: "Retail price."),
                    UnitPrice = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: false, comment: "Unit price."),
                    MininumUnits = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: false, comment: "Tier minimum units."),
                    UnitOfMeasure = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false, comment: "Unit of measure."),
                    IsPrimaryRegion = table.Column<bool>(type: "boolean", nullable: false, comment: "Indicates whether this is the primary rate for the region."),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Additional remarks or comments about the rate."),
                    SearchIndex = table.Column<string>(type: "text", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformRate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformRate_PlatformProduct_PlatformProductId",
                        column: x => x.PlatformProductId,
                        principalTable: "PlatformProduct",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents the pricing rate of a service on a platform, including details such as the currency, price, and validity period.");

            migrationBuilder.CreateTable(
                name: "PlatformRateUnit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformInfo entity."),
                    PlatformProductId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformProduct entity."),
                    PlatformRateId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformProduct entity."),
                    SetupUnitId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key linking to the SetupUnit entity."),
                    UnitFactor = table.Column<decimal>(type: "numeric(12,6)", precision: 12, scale: 6, nullable: false, comment: "Conversion factor for the unit rate, translating platform units to standard units."),
                    DefaultValue = table.Column<decimal>(type: "numeric(12,4)", precision: 12, scale: 4, nullable: false, comment: "Default rate for the unit, representing a base measurement standard."),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Description of the measuring unit, providing additional context for users."),
                    SearchIndex = table.Column<string>(type: "text", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true)
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
                    table.ForeignKey(
                        name: "FK_PlatformRateUnit_SetupUnit_SetupUnitId",
                        column: x => x.SetupUnitId,
                        principalSchema: "dbo",
                        principalTable: "SetupUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Table representing a unit of measurement for a rate within a platform's product offering, translating platform-specific units into standard project metrics.");

            migrationBuilder.CreateIndex(
                name: "IX_Platform_Name",
                table: "Platform",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platform_SearchIndex",
                table: "Platform",
                column: "SearchIndex");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformProduct_PlatformInfoId_Name",
                table: "PlatformProduct",
                columns: new[] { "PlatformInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformProduct_SearchIndex",
                table: "PlatformProduct",
                column: "SearchIndex");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRate_PlatformProductId",
                table: "PlatformRate",
                column: "PlatformProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRate_SearchIndex",
                table: "PlatformRate",
                column: "SearchIndex");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRateUnit_PlatformRateId",
                table: "PlatformRateUnit",
                column: "PlatformRateId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRateUnit_SetupUnitId",
                table: "PlatformRateUnit",
                column: "SetupUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRegion_PlatformInfoId_Name",
                table: "PlatformRegion",
                columns: new[] { "PlatformInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRegion_SearchIndex",
                table: "PlatformRegion",
                column: "SearchIndex");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformService_PlatformInfoId_Name",
                table: "PlatformService",
                columns: new[] { "PlatformInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformService_SearchIndex",
                table: "PlatformService",
                column: "SearchIndex");

            migrationBuilder.CreateIndex(
                name: "IX_SetupUnit_Code",
                schema: "dbo",
                table: "SetupUnit",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupUnit_Name",
                schema: "dbo",
                table: "SetupUnit",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupUnit_SearchIndex",
                schema: "dbo",
                table: "SetupUnit",
                column: "SearchIndex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformRateUnit");

            migrationBuilder.DropTable(
                name: "PlatformRegion");

            migrationBuilder.DropTable(
                name: "PlatformService");

            migrationBuilder.DropTable(
                name: "PlatformRate");

            migrationBuilder.DropTable(
                name: "SetupUnit",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "PlatformProduct");

            migrationBuilder.DropTable(
                name: "Platform");
        }
    }
}
