using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class AlterPlatformSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasure",
                table: "PlatformRateUnit",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "Unit of measure.");

            migrationBuilder.AlterColumn<string>(
                name: "UnitOfMeasure",
                table: "PlatformRate",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                comment: "Unit of measure.",
                oldClrType: typeof(string),
                oldType: "nvarchar(32)",
                oldMaxLength: 32,
                oldComment: "Unit of measure.");

            migrationBuilder.CreateTable(
                name: "PlatformDefaultUnit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    PlatformInfoId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Foreign key referencing the PlatformInfo."),
                    SetupUnitId = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Foreign key linking to the SetupUnit entity."),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "Unit of measure."),
                    UnitFactor = table.Column<decimal>(type: "decimal(12,6)", precision: 12, scale: 6, nullable: false, comment: "Conversion factor for the unit rate, translating platform units to standard units."),
                    DefaultValue = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: false, comment: "Default rate for the unit, representing a base measurement standard."),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "A brief description providing additional details about the region."),
                    SearchIndex = table.Column<string>(type: "nvarchar(450)", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformDefaultUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformDefaultUnit_Platform_PlatformInfoId",
                        column: x => x.PlatformInfoId,
                        principalTable: "Platform",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlatformDefaultUnit_SetupUnit_SetupUnitId",
                        column: x => x.SetupUnitId,
                        principalTable: "SetupUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents a default unit of measure for a platform, linking to a setup unit and a specific platform.");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDefaultUnit_PlatformInfoId",
                table: "PlatformDefaultUnit",
                column: "PlatformInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDefaultUnit_SearchIndex",
                table: "PlatformDefaultUnit",
                column: "SearchIndex");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDefaultUnit_SetupUnitId",
                table: "PlatformDefaultUnit",
                column: "SetupUnitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformDefaultUnit");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasure",
                table: "PlatformRateUnit");

            migrationBuilder.AlterColumn<string>(
                name: "UnitOfMeasure",
                table: "PlatformRate",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: false,
                comment: "Unit of measure.",
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldComment: "Unit of measure.");
        }
    }
}
