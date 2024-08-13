using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Platform
{
    /// <inheritdoc />
    public partial class UpdatePlatformSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlatformProductId",
                table: "PlatformService");

            migrationBuilder.DropColumn(
                name: "PlatformRegionId",
                table: "PlatformService");

            migrationBuilder.DropColumn(
                name: "ServiceFamily",
                table: "PlatformService");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "PlatformService");

            migrationBuilder.DropColumn(
                name: "ServiceName",
                table: "PlatformService");

            migrationBuilder.RenameColumn(
                name: "Size",
                table: "PlatformService",
                newName: "PricingTier");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "PlatformService",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                comment: "Category of the service");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "PlatformService");

            migrationBuilder.RenameColumn(
                name: "PricingTier",
                table: "PlatformService",
                newName: "Size");

            migrationBuilder.AddColumn<string>(
                name: "PlatformProductId",
                table: "PlatformService",
                type: "nvarchar(26)",
                nullable: false,
                defaultValue: "",
                comment: "Foreign key referencing the PlatformProduct entity.");

            migrationBuilder.AddColumn<string>(
                name: "PlatformRegionId",
                table: "PlatformService",
                type: "nvarchar(26)",
                nullable: false,
                defaultValue: "",
                comment: "Foreign key referencing the PlatformRegion entity.");

            migrationBuilder.AddColumn<string>(
                name: "ServiceFamily",
                table: "PlatformService",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true,
                comment: "Service family or category");

            migrationBuilder.AddColumn<string>(
                name: "ServiceId",
                table: "PlatformService",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "Original identifier used to reference the service.");

            migrationBuilder.AddColumn<string>(
                name: "ServiceName",
                table: "PlatformService",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                comment: "Original name of the service used for external identification.");
        }
    }
}
