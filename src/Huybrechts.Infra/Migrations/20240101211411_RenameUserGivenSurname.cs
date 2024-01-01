using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RenameUserGivenSurname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastName",
                table: "IdentityUser",
                newName: "Surname");

            migrationBuilder.RenameColumn(
                name: "FirstName",
                table: "IdentityUser",
                newName: "GivenName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Surname",
                table: "IdentityUser",
                newName: "LastName");

            migrationBuilder.RenameColumn(
                name: "GivenName",
                table: "IdentityUser",
                newName: "FirstName");
        }
    }
}
