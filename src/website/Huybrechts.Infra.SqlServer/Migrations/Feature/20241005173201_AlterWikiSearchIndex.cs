using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class AlterWikiSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "WikiPage",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                comment: "Gets or sets the markdown content for the wiki page.",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "Gets or sets the markdown content for the wiki page.");

            migrationBuilder.AddColumn<float>(
                name: "Rank",
                table: "WikiPage",
                type: "real",
                nullable: false,
                defaultValue: 0f,
                comment: "Represents the rank of the search result during specific queries.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Rank",
                table: "WikiPage");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "WikiPage",
                type: "nvarchar(max)",
                nullable: true,
                comment: "Gets or sets the markdown content for the wiki page.",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldComment: "Gets or sets the markdown content for the wiki page.");
        }
    }
}
