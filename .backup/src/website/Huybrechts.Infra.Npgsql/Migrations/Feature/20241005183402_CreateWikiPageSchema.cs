﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Npgsql.Migrations.Feature
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
                    Id = table.Column<string>(type: "character varying(26)", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    Namespace = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Gets or sets the namespace to which the wiki page belongs (e.g., 'UserGuide')."),
                    Page = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false, comment: "Gets or sets the page or URL slug for the wiki page."),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false, comment: "Gets or sets the title of the wiki page."),
                    Tags = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true, comment: "Keywords or categories for the project"),
                    SearchIndex = table.Column<string>(type: "text", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    PreviewText = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: "Gets or sets the first characters of the markdown content for the wiki page."),
                    Content = table.Column<string>(type: "text", nullable: false, comment: "Gets or sets the markdown content for the wiki page."),
                    Rank = table.Column<float>(type: "real", nullable: false, comment: "Represents the rank of the search result during specific queries."),
                    TenantId = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true, comment: "Gets or sets the concurrency timestamp for the entity.")
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
