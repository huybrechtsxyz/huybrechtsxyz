using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Platform
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
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Name of the platform"),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Description of the platform"),
                    Provider = table.Column<int>(type: "int", nullable: false, comment: "Supported automation providers of the platform"),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Remark about the platform"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform", x => x.Id);
                },
                comment: "Platforms that provide compute resources");

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

            migrationBuilder.CreateTable(
                name: "PlatformService",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "PlatformInfo FK"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Name"),
                    Label = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Label of the service"),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, comment: "Service Category"),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Description"),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Remark"),
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
                name: "IX_PlatformRegion_PlatformInfoId",
                table: "PlatformRegion",
                column: "PlatformInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformService_PlatformInfoId",
                table: "PlatformService",
                column: "PlatformInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformRegion");

            migrationBuilder.DropTable(
                name: "PlatformService");

            migrationBuilder.DropTable(
                name: "Platform");
        }
    }
}
