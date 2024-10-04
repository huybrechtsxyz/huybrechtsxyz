using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Npgsql.Migrations.Feature
{
    /// <inheritdoc />
    public partial class CreateWikiSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "tsv_english",
                table: "WikiPage",
                type: "tsvector",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "tsv_dutch",
                table: "WikiPage",
                type: "tsvector",
                nullable: true);

            migrationBuilder.Sql(
                @"CREATE INDEX IX_WikiPage_FT_English ON WikiPage USING gin (to_tsvector('english', ""Content""));");

            migrationBuilder.Sql(
                @"CREATE INDEX IX_WikiPage_FT_Dutch ON WikiPage USING gin (to_tsvector('dutch', ""Content""));");

            migrationBuilder.Sql(
                @"UPDATE WikiPage 
                  SET tsv_english = to_tsvector('english', ""Content""),
                      tsv_dutch = to_tsvector('dutch', ""Content"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_WikiPage_FT_English", table: "WikiPage");

            migrationBuilder.DropIndex(name: "IX_WikiPage_FT_Dutch", table: "WikiPage");

            migrationBuilder.DropColumn(name: "tsv_english", table: "WikiPage");

            migrationBuilder.DropColumn(name: "tsv_dutch", table: "WikiPage");
        }
    }
}
