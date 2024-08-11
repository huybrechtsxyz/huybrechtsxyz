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
            migrationBuilder.CreateTable(
                name: "Platform",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Name of the platform."),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Detailed description of the platform."),
                    Provider = table.Column<int>(type: "integer", nullable: false, comment: "The platform's supported automation provider, enabling automated resource management."),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Additional remarks or comments about the platform."),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform", x => x.Id);
                },
                comment: "Table storing information about platforms that offer compute resources, including cloud providers like Azure or Google, and on-premise solutions.");

            migrationBuilder.CreateTable(
                name: "PlatformProduct",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformInfo entity."),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "The name of the product or service offered by the platform."),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "A label representing the product, often used in the user interface."),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "The category or family of the product, helping to classify it among similar offerings."),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "A brief description providing additional details about the product."),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Additional remarks or notes regarding the product."),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
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
                comment: "Products or services offered by the platform, such as compute, storage, or networking resources.");

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
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
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
                    PlatformRegionId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformRegion entity."),
                    PlatformProductId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Foreign key referencing the PlatformProduct entity."),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "The name of the service offered by the platform."),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "A label representing the service, often used for display purposes."),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "A brief description providing details about the service."),
                    CostDriver = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "The cost driver or factor that influences the pricing of the service."),
                    CostBasedOn = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Parameters or metrics on which the cost of the service is based."),
                    Limitations = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: "Limitations or constraints related to the service."),
                    AboutURL = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: "URL linking to additional information about the service."),
                    PricingURL = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true, comment: "URL providing pricing information for the service."),
                    ServiceId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "Original identifier used to reference the service."),
                    ServiceName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true, comment: "Original name of the service used for external identification."),
                    ServiceFamily = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Service family or category"),
                    Size = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true, comment: "Size or pricing tier associated with the service."),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Additional remarks or notes regarding the service."),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_PlatformProduct_PlatformInfoId_Name",
                table: "PlatformProduct",
                columns: new[] { "PlatformInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRegion_PlatformInfoId_Name",
                table: "PlatformRegion",
                columns: new[] { "PlatformInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformService_PlatformInfoId_Name",
                table: "PlatformService",
                columns: new[] { "PlatformInfoId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformProduct");

            migrationBuilder.DropTable(
                name: "PlatformRegion");

            migrationBuilder.DropTable(
                name: "PlatformService");

            migrationBuilder.DropTable(
                name: "Platform");
        }
    }
}
