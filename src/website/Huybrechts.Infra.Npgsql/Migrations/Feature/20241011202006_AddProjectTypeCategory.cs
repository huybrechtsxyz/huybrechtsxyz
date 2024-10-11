using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Npgsql.Migrations.Feature
{
    /// <inheritdoc />
    public partial class AddProjectTypeCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Project",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "The general category of the project, used for higher-level classification, such as Construction, Research, or Technology.");

            migrationBuilder.AddColumn<string>(
                name: "ProjectKind",
                table: "Project",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "The specific kind of the project within the project type, such as Mobile App, Web App, or Database Development.");

            migrationBuilder.AddColumn<string>(
                name: "ProjectOrigin",
                table: "Project",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "The origin of the project, indicating if it's internal, external, or customer-driven.");

            migrationBuilder.AddColumn<string>(
                name: "ProjectType",
                table: "Project",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "The type of the project, such as Infrastructure, Software Development, or Research.");

            migrationBuilder.AddColumn<string>(
                name: "Subcategory",
                table: "Project",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "",
                comment: "The specific subcategory of the project, providing more detailed classification, such as AI Development or Cybersecurity.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "ProjectKind",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "ProjectOrigin",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "ProjectType",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "Subcategory",
                table: "Project");
        }
    }
}
