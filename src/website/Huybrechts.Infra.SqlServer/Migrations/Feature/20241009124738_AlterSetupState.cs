using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class AlterSetupState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SetupState_TenantId_ObjectType_Name",
                table: "SetupState");

            migrationBuilder.DropColumn(
                name: "ObjectType",
                table: "SetupState");

            migrationBuilder.AddColumn<string>(
                name: "TypeOf",
                table: "SetupState",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "The classification or type for the object (e.g., ProjectType, ProjectKind).");

            migrationBuilder.CreateIndex(
                name: "IX_SetupState_TenantId_TypeOf_Name",
                table: "SetupState",
                columns: new[] { "TenantId", "TypeOf", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SetupState_TenantId_TypeOf_Name",
                table: "SetupState");

            migrationBuilder.DropColumn(
                name: "TypeOf",
                table: "SetupState");

            migrationBuilder.AddColumn<int>(
                name: "ObjectType",
                table: "SetupState",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SetupState_TenantId_ObjectType_Name",
                table: "SetupState",
                columns: new[] { "TenantId", "ObjectType", "Name" },
                unique: true);
        }
    }
}
