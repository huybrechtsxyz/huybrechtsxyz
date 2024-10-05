using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class CreateWikiSearchIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF CAST(SERVERPROPERTY('Edition') AS NVARCHAR(100)) NOT LIKE '%Express%' BEGIN
                    CREATE FULLTEXT CATALOG WikiCatalog WITH ACCENT_SENSITIVITY = OFF;
                END
                ELSE BEGIN
                    PRINT 'Skipping full-text index creation because this is LocalDB.';
                END;
            ", suppressTransaction: true);

            migrationBuilder.Sql(@"
                IF CAST(SERVERPROPERTY('Edition') AS NVARCHAR(100)) NOT LIKE '%Express%' BEGIN
                    CREATE FULLTEXT INDEX ON WikiPage(Namespace, Page, Title, Tags, Content) KEY INDEX Id ON WikiCatalog WITH CHANGE_TRACKING AUTO;
                END
                ELSE BEGIN
                    PRINT 'Skipping full-text index creation because this is LocalDB.';
                END;
            ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF CAST(SERVERPROPERTY('Edition') AS NVARCHAR(100)) NOT LIKE '%Express%' BEGIN
                    DROP FULLTEXT INDEX ON WikiPage;
                END
                ELSE BEGIN
                    PRINT 'Skipping full-text index deletion because this is LocalDB.';
                END;
            ", suppressTransaction: true);

            migrationBuilder.Sql(@"
                IF CAST(SERVERPROPERTY('Edition') AS NVARCHAR(100)) NOT LIKE '%Express%' BEGIN
                    DROP FULLTEXT CATALOG WikiCatalog;
                END
                ELSE BEGIN
                    PRINT 'Skipping full-text index deletion because this is LocalDB.';
                END;
            ", suppressTransaction: true);
        }
    }
}
