using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
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
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    Namespace = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Gets or sets the namespace to which the wiki page belongs (e.g., 'UserGuide')."),
                    Page = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false, comment: "Gets or sets the page or URL slug for the wiki page."),
                    Title = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: false, comment: "Gets or sets the title of the wiki page."),
                    Tags = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Keywords or categories for the project"),
                    SearchIndex = table.Column<string>(type: "nvarchar(450)", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Gets or sets the markdown content for the wiki page."),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true, comment: "Gets or sets the concurrency timestamp for the entity.")
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
