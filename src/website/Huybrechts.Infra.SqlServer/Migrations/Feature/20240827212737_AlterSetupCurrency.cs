using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class AlterSetupCurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetupCurrencies_SetupCountries_SetupCountryId",
                table: "SetupCurrencies");

            migrationBuilder.DropIndex(
                name: "IX_SetupCurrencies_SetupCountryId",
                table: "SetupCurrencies");

            migrationBuilder.DropColumn(
                name: "SetupCountryId",
                table: "SetupCurrencies");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SetupCountryId",
                table: "SetupCurrencies",
                type: "nvarchar(26)",
                nullable: false,
                defaultValue: "",
                comment: "Foreign key referencing the SetupCountry.");

            migrationBuilder.CreateIndex(
                name: "IX_SetupCurrencies_SetupCountryId",
                table: "SetupCurrencies",
                column: "SetupCountryId");

            migrationBuilder.AddForeignKey(
                name: "FK_SetupCurrencies_SetupCountries_SetupCountryId",
                table: "SetupCurrencies",
                column: "SetupCountryId",
                principalTable: "SetupCountries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
