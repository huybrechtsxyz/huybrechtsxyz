using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Platform
{
    /// <inheritdoc />
    public partial class CreatePlatformService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlatformService",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformInfo FK"),
                    PlatformRegionId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformRegion FK"),
                    PlatformProductId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformProduct FK"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Name of the service"),
                    Label = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Label of the service"),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Description of the service"),
                    CostDriver = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Cost driver"),
                    CostBasedOn = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "Cost is based on what parameters for the service"),
                    Limitations = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, comment: "Limitations of the service"),
                    AboutURL = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, comment: "About Link"),
                    PricingURL = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true, comment: "Pricing url"),
                    ServiceId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Original id of the service"),
                    ServiceName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true, comment: "Original name of the service"),
                    Size = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true, comment: "Size/pricing tier of the service"),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Remarks about the service"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
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

            migrationBuilder.CreateIndex(
                name: "IX_PlatformService_PlatformInfoId",
                table: "PlatformService",
                column: "PlatformInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformService");
        }
    }
}
