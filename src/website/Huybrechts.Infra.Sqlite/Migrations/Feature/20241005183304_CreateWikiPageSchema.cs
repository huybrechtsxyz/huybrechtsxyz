using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Sqlite.Migrations.Feature
{
    /// <inheritdoc />
    public partial class CreateWikiPageSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WikiPage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    Namespace = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Gets or sets the namespace to which the wiki page belongs (e.g., 'UserGuide')."),
                    Page = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false, comment: "Gets or sets the page or URL slug for the wiki page."),
                    Title = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false, comment: "Gets or sets the title of the wiki page."),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Keywords or categories for the project"),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    PreviewText = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false, comment: "Gets or sets the first characters of the markdown content for the wiki page."),
                    Content = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the markdown content for the wiki page."),
                    Rank = table.Column<float>(type: "REAL", nullable: false, comment: "Represents the rank of the search result during specific queries."),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiPage", x => x.Id);
                },
                comment: "Represents a wiki page.");

            migrationBuilder.CreateIndex(
                name: "IX_WikiPage_TenantId_Namespace_Page",
                table: "WikiPage",
                columns: new[] { "TenantId", "Namespace", "Page" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WikiPage_TenantId_SearchIndex",
                table: "WikiPage",
                columns: new[] { "TenantId", "SearchIndex" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WikiPage");
        }
    }
}
