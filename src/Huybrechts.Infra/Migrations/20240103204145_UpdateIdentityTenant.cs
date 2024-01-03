using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Description",
                table: "IdentityRole",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "IdentityRole",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "IdentityRole",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: true);

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
                name: "IX_IdentityRole_TenantId_Label",
                table: "IdentityRole",
                columns: new[] { "TenantId", "Label" },
                unique: true,
                filter: "[Label] IS NOT NULL");

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_IdentityRole_IdentityTenant_TenantId",
                table: "IdentityRole");

            migrationBuilder.DropTable(
                name: "IdentityUserTenant");

            migrationBuilder.DropIndex(
                name: "IX_IdentityRole_TenantId_Label",
                table: "IdentityRole");

            migrationBuilder.DropColumn(
                name: "Picture",
                table: "IdentityTenant");

            migrationBuilder.DropColumn(
                name: "State",
                table: "IdentityTenant");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "IdentityRole");

            migrationBuilder.DropColumn(
                name: "Label",
                table: "IdentityRole");

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
