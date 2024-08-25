using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Sqlite.Migrations.Setup
{
    /// <inheritdoc />
    public partial class CreateSetupUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SetupUnit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Primary Key"),
                    UnitType = table.Column<int>(type: "INTEGER", maxLength: 32, nullable: false, comment: "Gets or sets the type of the unit (e.g., Height, Weight, Volume, System, etc.)."),
                    Code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, comment: "A unique code representing the unit, standard across all instances."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, comment: "The unique name of the unit within its type."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true, comment: "A detailed description of the unit."),
                    IsBase = table.Column<bool>(type: "INTEGER", nullable: false, comment: "Indicates whether this unit is the base unit for its type."),
                    Precision = table.Column<int>(type: "INTEGER", nullable: false, comment: "Number of decimal places for the unit."),
                    PrecisionType = table.Column<int>(type: "INTEGER", nullable: false, comment: "Determines how values are rounded according to the System.Decimal Rounding enum."),
                    Factor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 10, nullable: false, comment: "A multiplication factor used to convert this unit to the base unit."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Additional remarks or comments about the unit."),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupUnit", x => x.Id);
                },
                comment: "Represents a measurement unit used for different types such as height, weight, volume, etc.");

            migrationBuilder.CreateIndex(
                name: "IX_SetupUnit_Code",
                table: "SetupUnit",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupUnit_Name",
                table: "SetupUnit",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupUnit_SearchIndex",
                table: "SetupUnit",
                column: "SearchIndex");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SetupUnit");
        }
    }
}
