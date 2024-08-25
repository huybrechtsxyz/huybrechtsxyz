using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Npgsql.Migrations.Setup
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
