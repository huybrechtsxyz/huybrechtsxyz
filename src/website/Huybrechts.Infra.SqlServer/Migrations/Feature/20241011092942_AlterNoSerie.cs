using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class AlterNoSerie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SetupNoSerie_TenantId_TypeOf_TypeCode_TypeValue",
                table: "SetupNoSerie");

            migrationBuilder.DropColumn(
                name: "TypeCode",
                table: "SetupNoSerie");

            migrationBuilder.RenameColumn(
                name: "Counter",
                table: "SetupNoSerie",
                newName: "LastCounter");

            migrationBuilder.AddColumn<bool>(
                name: "AutomaticReset",
                table: "SetupNoSerie",
                type: "bit",
                nullable: false,
                defaultValue: false,
                comment: "Indicates whether the counter will automatically reset when the number series changes (e.g., at the start of a new year).");

            migrationBuilder.AddColumn<int>(
                name: "Increment",
                table: "SetupNoSerie",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "Specifies the increment value for the counter when generating a new number.");

            migrationBuilder.AddColumn<string>(
                name: "LastPrefix",
                table: "SetupNoSerie",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "Stores the last generated prefix in the series to track the most recent value.");

            migrationBuilder.AddColumn<string>(
                name: "LastValue",
                table: "SetupNoSerie",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "Stores the last generated number in the series to track the most recent value.");

            migrationBuilder.AddColumn<int>(
                name: "StartCounter",
                table: "SetupNoSerie",
                type: "int",
                nullable: false,
                defaultValue: 0,
                comment: "The maximum allowed value for the counter before it resets or stops.");

            migrationBuilder.CreateIndex(
                name: "IX_SetupNoSerie_TenantId_TypeOf_TypeValue",
                table: "SetupNoSerie",
                columns: new[] { "TenantId", "TypeOf", "TypeValue" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SetupNoSerie_TenantId_TypeOf_TypeValue",
                table: "SetupNoSerie");

            migrationBuilder.DropColumn(
                name: "AutomaticReset",
                table: "SetupNoSerie");

            migrationBuilder.DropColumn(
                name: "Increment",
                table: "SetupNoSerie");

            migrationBuilder.DropColumn(
                name: "LastPrefix",
                table: "SetupNoSerie");

            migrationBuilder.DropColumn(
                name: "LastValue",
                table: "SetupNoSerie");

            migrationBuilder.DropColumn(
                name: "StartCounter",
                table: "SetupNoSerie");

            migrationBuilder.RenameColumn(
                name: "LastCounter",
                table: "SetupNoSerie",
                newName: "Counter");

            migrationBuilder.AddColumn<string>(
                name: "TypeCode",
                table: "SetupNoSerie",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "The category on which the number series is based, such as ProjectType or InvoiceType.");

            migrationBuilder.CreateIndex(
                name: "IX_SetupNoSerie_TenantId_TypeOf_TypeCode_TypeValue",
                table: "SetupNoSerie",
                columns: new[] { "TenantId", "TypeOf", "TypeCode", "TypeValue" });
        }
    }
}
