using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Sqlite.Migrations.Feature
{
    /// <inheritdoc />
    public partial class CreateWikiSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NO FULL TEXT SEARCH INDEX ON SQLITE
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // NO FULL TEXT SEARCH INDEX ON SQLITE
        }
    }
}
