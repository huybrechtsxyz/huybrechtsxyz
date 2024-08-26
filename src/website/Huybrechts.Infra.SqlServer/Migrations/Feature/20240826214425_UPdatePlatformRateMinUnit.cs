using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class UPdatePlatformRateMinUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MininumUnits",
                table: "PlatformRate",
                newName: "MinimumUnits");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SetupUnit",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                comment: "The unique name of the unit within its type.",
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldComment: "The unique name of the unit within its type.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MinimumUnits",
                table: "PlatformRate",
                newName: "MininumUnits");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "SetupUnit",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                comment: "The unique name of the unit within its type.",
                oldClrType: typeof(string),
                oldType: "nvarchar(128)",
                oldMaxLength: 128,
                oldComment: "The unique name of the unit within its type.");
        }
    }
}
