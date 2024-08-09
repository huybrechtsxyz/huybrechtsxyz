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
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Name of the platform"),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Description of the platform"),
                    Provider = table.Column<int>(type: "integer", nullable: false, comment: "Supported automation providers of the platform"),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Remark about the platform"),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform", x => x.Id);
                },
                comment: "Platforms that provide compute resources");

            migrationBuilder.CreateTable(
                name: "PlatformProduct",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "PlatformInfo FK"),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Name of the product"),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Label of the product"),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true, comment: "Product category"),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Description"),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Remark"),
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
                });

            migrationBuilder.CreateTable(
                name: "PlatformRegion",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "character varying(26)", nullable: false, comment: "PlatformInfo FK"),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Name of the region"),
                    Label = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Label of the region"),
                    Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Description of the region"),
                    Remark = table.Column<string>(type: "text", nullable: true, comment: "Remark about the region"),
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
                comment: "Support regions of the Platform");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformProduct_PlatformInfoId",
                table: "PlatformProduct",
                column: "PlatformInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRegion_PlatformInfoId",
                table: "PlatformRegion",
                column: "PlatformInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformProduct");

            migrationBuilder.DropTable(
                name: "PlatformRegion");

            migrationBuilder.DropTable(
                name: "Platform");
        }
    }
}
