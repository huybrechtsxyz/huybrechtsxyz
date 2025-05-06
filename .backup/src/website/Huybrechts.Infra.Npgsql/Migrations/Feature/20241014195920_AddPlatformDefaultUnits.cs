using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Npgsql.Migrations.Feature
{
    /// <inheritdoc />
    public partial class AddPlatformDefaultUnits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformDefaultUnit_SetupUnit_SetupUnitId",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropIndex(
                name: "IX_PlatformDefaultUnit_TenantId_PlatformInfoId_UnitOfMeasure",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropIndex(
                name: "IX_PlatformDefaultUnit_TenantId_SearchIndex",
                table: "PlatformDefaultUnit");

            migrationBuilder.AlterColumn<string>(
                name: "UnitOfMeasure",
                table: "PlatformDefaultUnit",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                comment: "Unit of measure.",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldComment: "Unit of measure.");

            migrationBuilder.AlterColumn<string>(
                name: "SetupUnitId",
                table: "PlatformDefaultUnit",
                type: "character varying(26)",
                nullable: true,
                comment: "Foreign key linking to the SetupUnit entity.",
                oldClrType: typeof(string),
                oldType: "character varying(26)",
                oldComment: "Foreign key linking to the SetupUnit entity.");

            migrationBuilder.AddColumn<string>(
                name: "Expression",
                table: "PlatformDefaultUnit",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                comment: "The formula used to calculate the value of the quantity for this unit.");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultPlatformRateUnit",
                table: "PlatformDefaultUnit",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Is this a default for the Platform Rate Unit");

            migrationBuilder.AddColumn<bool>(
                name: "IsDefaultProjectComponentUnit",
                table: "PlatformDefaultUnit",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Is this a default for the Project Component Unit");

            migrationBuilder.AddColumn<string>(
                name: "MeterName",
                table: "PlatformDefaultUnit",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                comment: "Meter name.");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "PlatformDefaultUnit",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                comment: "Product name.");

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "PlatformDefaultUnit",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Gets or sets the sequence order of this unit.");

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "PlatformDefaultUnit",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                comment: "The name of the service.");

            migrationBuilder.AddColumn<string>(
                name: "SkuName",
                table: "PlatformDefaultUnit",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                comment: "SKU name.");

            migrationBuilder.AddColumn<string>(
                name: "Variable",
                table: "PlatformDefaultUnit",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                comment: "The variable name used in the metrics calculations for this unit.");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDefaultUnit_TenantId_PlatformInfoId_SearchIndex",
                table: "PlatformDefaultUnit",
                columns: new[] { "TenantId", "PlatformInfoId", "SearchIndex" });

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformDefaultUnit_SetupUnit_SetupUnitId",
                table: "PlatformDefaultUnit",
                column: "SetupUnitId",
                principalTable: "SetupUnit",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlatformDefaultUnit_SetupUnit_SetupUnitId",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropIndex(
                name: "IX_PlatformDefaultUnit_TenantId_PlatformInfoId_SearchIndex",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropColumn(
                name: "Expression",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropColumn(
                name: "IsDefaultPlatformRateUnit",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropColumn(
                name: "IsDefaultProjectComponentUnit",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropColumn(
                name: "MeterName",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropColumn(
                name: "SkuName",
                table: "PlatformDefaultUnit");

            migrationBuilder.DropColumn(
                name: "Variable",
                table: "PlatformDefaultUnit");

            migrationBuilder.AlterColumn<string>(
                name: "UnitOfMeasure",
                table: "PlatformDefaultUnit",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "Unit of measure.",
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64,
                oldNullable: true,
                oldComment: "Unit of measure.");

            migrationBuilder.AlterColumn<string>(
                name: "SetupUnitId",
                table: "PlatformDefaultUnit",
                type: "character varying(26)",
                nullable: false,
                defaultValue: "",
                comment: "Foreign key linking to the SetupUnit entity.",
                oldClrType: typeof(string),
                oldType: "character varying(26)",
                oldNullable: true,
                oldComment: "Foreign key linking to the SetupUnit entity.");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDefaultUnit_TenantId_PlatformInfoId_UnitOfMeasure",
                table: "PlatformDefaultUnit",
                columns: new[] { "TenantId", "PlatformInfoId", "UnitOfMeasure" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDefaultUnit_TenantId_SearchIndex",
                table: "PlatformDefaultUnit",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.AddForeignKey(
                name: "FK_PlatformDefaultUnit_SetupUnit_SetupUnitId",
                table: "PlatformDefaultUnit",
                column: "SetupUnitId",
                principalTable: "SetupUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
