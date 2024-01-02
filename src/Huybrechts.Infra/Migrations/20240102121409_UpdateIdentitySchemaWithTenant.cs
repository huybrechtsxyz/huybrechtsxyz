using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentitySchemaWithTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityUserRole",
                table: "IdentityUserRole");

            migrationBuilder.DropIndex(
                name: "IX_IdentityTenant_Code",
                table: "IdentityTenant");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "IdentityRole");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyToken",
                table: "IdentityTenant",
                newName: "ConcurrencyStamp");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "IdentityTenant",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "IdentityUserRole",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<byte[]>(
                name: "Picture",
                table: "IdentityTenant",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "State",
                table: "IdentityTenant",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "IdentityRole",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityUserRole",
                table: "IdentityUserRole",
                columns: new[] { "UserId", "TenantId", "RoleId" });

            migrationBuilder.CreateTable(
                name: "IdentityUserTenant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    TenantId = table.Column<string>(type: "nvarchar(24)", maxLength: 24, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", rowVersion: true, nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IdentityUserTenant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IdentityUserTenant_IdentityTenant_TenantId",
                        column: x => x.TenantId,
                        principalTable: "IdentityTenant",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IdentityUserTenant_IdentityUser_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "IdentityUser",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IdentityUserTenant_IdentityUser_UserId",
                        column: x => x.UserId,
                        principalTable: "IdentityUser",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityUserRole_TenantId_RoleId",
                table: "IdentityUserRole",
                columns: new[] { "TenantId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "IdentityRole",
                columns: new[] { "TenantId", "NormalizedName" },
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityUserTenant_ApplicationUserId",
                table: "IdentityUserTenant",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityUserTenant_TenantId",
                table: "IdentityUserTenant",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_IdentityUserTenant_UserId_TenantId",
                table: "IdentityUserTenant",
                columns: new[] { "UserId", "TenantId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityRole_IdentityTenant_TenantId",
                table: "IdentityRole",
                column: "TenantId",
                principalTable: "IdentityTenant",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_IdentityUserRole_IdentityTenant_TenantId",
                table: "IdentityUserRole",
                column: "TenantId",
                principalTable: "IdentityTenant",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRole_IdentityTenant_TenantId",
                table: "IdentityRole");

            migrationBuilder.DropForeignKey(
                name: "FK_IdentityUserRole_IdentityTenant_TenantId",
                table: "IdentityUserRole");

            migrationBuilder.DropTable(
                name: "IdentityUserTenant");

            migrationBuilder.DropPrimaryKey(
                name: "PK_IdentityUserRole",
                table: "IdentityUserRole");

            migrationBuilder.DropIndex(
                name: "IX_IdentityUserRole_TenantId_RoleId",
                table: "IdentityUserRole");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "IdentityRole");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "IdentityUserRole");

            migrationBuilder.DropColumn(
                name: "Picture",
                table: "IdentityTenant");

            migrationBuilder.DropColumn(
                name: "State",
                table: "IdentityTenant");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "IdentityRole");

            migrationBuilder.RenameColumn(
                name: "ConcurrencyStamp",
                table: "IdentityTenant",
                newName: "ConcurrencyToken");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "IdentityTenant",
                newName: "Code");

            migrationBuilder.AddPrimaryKey(
                name: "PK_IdentityUserRole",
                table: "IdentityUserRole",
                columns: new[] { "UserId", "RoleId" });

            migrationBuilder.CreateIndex(
                name: "IX_IdentityTenant_Code",
                table: "IdentityTenant",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "IdentityRole",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");
        }
    }
}
