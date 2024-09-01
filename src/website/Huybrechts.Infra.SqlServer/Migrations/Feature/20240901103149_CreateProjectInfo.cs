using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    /// <inheritdoc />
    public partial class CreateProjectInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetupCountries_SetupCurrencies_SetupCurrencyId",
                table: "SetupCountries");

            migrationBuilder.DropForeignKey(
                name: "FK_SetupCountries_SetupLanguages_SetupLanguageId",
                table: "SetupCountries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SetupLanguages",
                table: "SetupLanguages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SetupCurrencies",
                table: "SetupCurrencies");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SetupCountries",
                table: "SetupCountries");

            migrationBuilder.RenameTable(
                name: "SetupLanguages",
                newName: "SetupLanguage");

            migrationBuilder.RenameTable(
                name: "SetupCurrencies",
                newName: "SetupCurrency");

            migrationBuilder.RenameTable(
                name: "SetupCountries",
                newName: "SetupCountry");

            migrationBuilder.RenameIndex(
                name: "IX_SetupCountries_SetupLanguageId",
                table: "SetupCountry",
                newName: "IX_SetupCountry_SetupLanguageId");

            migrationBuilder.RenameIndex(
                name: "IX_SetupCountries_SetupCurrencyId",
                table: "SetupCountry",
                newName: "IX_SetupCountry_SetupCurrencyId");

            migrationBuilder.AlterTable(
                name: "SetupLanguage",
                comment: "Represents a currency entity with detailed information such as code, name, description, and associated country code.");

            migrationBuilder.AlterTable(
                name: "SetupCurrency",
                comment: "Represents a currency entity with detailed information such as code, name, description, and associated country code.");

            migrationBuilder.AlterTable(
                name: "SetupCountry",
                comment: "Represents information about different countries, including their codes, names, and associated details.");

            migrationBuilder.AlterColumn<string>(
                name: "SearchIndex",
                table: "SetupLanguage",
                type: "nvarchar(450)",
                nullable: true,
                comment: "This field will store the normalized, concatenated values for searching",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "This field will store the normalized, concatenated values for searching");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "SetupLanguage",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SearchIndex",
                table: "SetupCurrency",
                type: "nvarchar(450)",
                nullable: true,
                comment: "This field will store the normalized, concatenated values for searching",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "This field will store the normalized, concatenated values for searching");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "SetupCurrency",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "SearchIndex",
                table: "SetupCountry",
                type: "nvarchar(450)",
                nullable: true,
                comment: "This field will store the normalized, concatenated values for searching",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true,
                oldComment: "This field will store the normalized, concatenated values for searching");

            migrationBuilder.AddColumn<string>(
                name: "TenantId",
                table: "SetupCountry",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SetupLanguage",
                table: "SetupLanguage",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SetupCurrency",
                table: "SetupCurrency",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SetupCountry",
                table: "SetupCountry",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    ParentId = table.Column<string>(type: "nvarchar(26)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, comment: "Code of the Project."),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false, comment: "Name of the Project."),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Detailed description of the Project."),
                    Remark = table.Column<string>(type: "nvarchar(max)", nullable: true, comment: "Additional remarks or comments about the Project."),
                    SearchIndex = table.Column<string>(type: "nvarchar(450)", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    State = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false, comment: "Gets or sets the current state of the project."),
                    Reason = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true, comment: "Gets or sets the reason for the current state of the project."),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Gets or sets the start date for the project."),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Gets or sets the target completion date for the project."),
                    Priority = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true, comment: "Gets or sets the priority of the project."),
                    Risk = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true, comment: "Gets or sets the risk of the project."),
                    Effort = table.Column<int>(type: "int", nullable: true, comment: "Gets or sets the effort required for the project."),
                    BusinessValue = table.Column<int>(type: "int", nullable: true, comment: "Gets or sets the business value of the project."),
                    Rating = table.Column<int>(type: "int", nullable: true, comment: "Gets or sets the rating of the project, reflecting its priority, quality, or stakeholder approval."),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                },
                comment: "Table storing information about Projects that offer compute resources, including cloud providers like Azure or Google, and on-premise solutions.");

            migrationBuilder.CreateTable(
                name: "SetupState",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(26)", nullable: false, comment: "Primary Key"),
                    ObjectType = table.Column<int>(type: "int", nullable: false),
                    StateType = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    SearchIndex = table.Column<string>(type: "nvarchar(450)", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedDT = table.Column<DateTime>(type: "datetime2", nullable: false, comment: "Date time created"),
                    ModifiedDT = table.Column<DateTime>(type: "datetime2", nullable: true, comment: "Modified time created"),
                    TimeStamp = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupState", x => x.Id);
                },
                comment: "Represents a custom state that can be applied to various objects, such as projects, constraints, requirements, and more.");

            migrationBuilder.CreateIndex(
                name: "IX_SetupLanguage_Code",
                table: "SetupLanguage",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupLanguage_Name",
                table: "SetupLanguage",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupLanguage_SearchIndex",
                table: "SetupLanguage",
                column: "SearchIndex");

            migrationBuilder.CreateIndex(
                name: "IX_SetupCurrency_Code",
                table: "SetupCurrency",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupCurrency_Name",
                table: "SetupCurrency",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupCurrency_SearchIndex",
                table: "SetupCurrency",
                column: "SearchIndex");

            migrationBuilder.CreateIndex(
                name: "IX_SetupCountry_Code",
                table: "SetupCountry",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupCountry_Name",
                table: "SetupCountry",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupCountry_SearchIndex",
                table: "SetupCountry",
                column: "SearchIndex");

            migrationBuilder.CreateIndex(
                name: "IX_Project_Code",
                table: "Project",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Project_SearchIndex",
                table: "Project",
                column: "SearchIndex");

            migrationBuilder.CreateIndex(
                name: "IX_SetupState_ObjectType_Name",
                table: "SetupState",
                columns: new[] { "ObjectType", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupState_SearchIndex",
                table: "SetupState",
                column: "SearchIndex");

            migrationBuilder.AddForeignKey(
                name: "FK_SetupCountry_SetupCurrency_SetupCurrencyId",
                table: "SetupCountry",
                column: "SetupCurrencyId",
                principalTable: "SetupCurrency",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SetupCountry_SetupLanguage_SetupLanguageId",
                table: "SetupCountry",
                column: "SetupLanguageId",
                principalTable: "SetupLanguage",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SetupCountry_SetupCurrency_SetupCurrencyId",
                table: "SetupCountry");

            migrationBuilder.DropForeignKey(
                name: "FK_SetupCountry_SetupLanguage_SetupLanguageId",
                table: "SetupCountry");

            migrationBuilder.DropTable(
                name: "Project");

            migrationBuilder.DropTable(
                name: "SetupState");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SetupLanguage",
                table: "SetupLanguage");

            migrationBuilder.DropIndex(
                name: "IX_SetupLanguage_Code",
                table: "SetupLanguage");

            migrationBuilder.DropIndex(
                name: "IX_SetupLanguage_Name",
                table: "SetupLanguage");

            migrationBuilder.DropIndex(
                name: "IX_SetupLanguage_SearchIndex",
                table: "SetupLanguage");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SetupCurrency",
                table: "SetupCurrency");

            migrationBuilder.DropIndex(
                name: "IX_SetupCurrency_Code",
                table: "SetupCurrency");

            migrationBuilder.DropIndex(
                name: "IX_SetupCurrency_Name",
                table: "SetupCurrency");

            migrationBuilder.DropIndex(
                name: "IX_SetupCurrency_SearchIndex",
                table: "SetupCurrency");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SetupCountry",
                table: "SetupCountry");

            migrationBuilder.DropIndex(
                name: "IX_SetupCountry_Code",
                table: "SetupCountry");

            migrationBuilder.DropIndex(
                name: "IX_SetupCountry_Name",
                table: "SetupCountry");

            migrationBuilder.DropIndex(
                name: "IX_SetupCountry_SearchIndex",
                table: "SetupCountry");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SetupLanguage");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SetupCurrency");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "SetupCountry");

            migrationBuilder.RenameTable(
                name: "SetupLanguage",
                newName: "SetupLanguages");

            migrationBuilder.RenameTable(
                name: "SetupCurrency",
                newName: "SetupCurrencies");

            migrationBuilder.RenameTable(
                name: "SetupCountry",
                newName: "SetupCountries");

            migrationBuilder.RenameIndex(
                name: "IX_SetupCountry_SetupLanguageId",
                table: "SetupCountries",
                newName: "IX_SetupCountries_SetupLanguageId");

            migrationBuilder.RenameIndex(
                name: "IX_SetupCountry_SetupCurrencyId",
                table: "SetupCountries",
                newName: "IX_SetupCountries_SetupCurrencyId");

            migrationBuilder.AlterTable(
                name: "SetupLanguages",
                oldComment: "Represents a currency entity with detailed information such as code, name, description, and associated country code.");

            migrationBuilder.AlterTable(
                name: "SetupCurrencies",
                oldComment: "Represents a currency entity with detailed information such as code, name, description, and associated country code.");

            migrationBuilder.AlterTable(
                name: "SetupCountries",
                oldComment: "Represents information about different countries, including their codes, names, and associated details.");

            migrationBuilder.AlterColumn<string>(
                name: "SearchIndex",
                table: "SetupLanguages",
                type: "nvarchar(max)",
                nullable: true,
                comment: "This field will store the normalized, concatenated values for searching",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true,
                oldComment: "This field will store the normalized, concatenated values for searching");

            migrationBuilder.AlterColumn<string>(
                name: "SearchIndex",
                table: "SetupCurrencies",
                type: "nvarchar(max)",
                nullable: true,
                comment: "This field will store the normalized, concatenated values for searching",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true,
                oldComment: "This field will store the normalized, concatenated values for searching");

            migrationBuilder.AlterColumn<string>(
                name: "SearchIndex",
                table: "SetupCountries",
                type: "nvarchar(max)",
                nullable: true,
                comment: "This field will store the normalized, concatenated values for searching",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true,
                oldComment: "This field will store the normalized, concatenated values for searching");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SetupLanguages",
                table: "SetupLanguages",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SetupCurrencies",
                table: "SetupCurrencies",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SetupCountries",
                table: "SetupCountries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SetupCountries_SetupCurrencies_SetupCurrencyId",
                table: "SetupCountries",
                column: "SetupCurrencyId",
                principalTable: "SetupCurrencies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_SetupCountries_SetupLanguages_SetupLanguageId",
                table: "SetupCountries",
                column: "SetupLanguageId",
                principalTable: "SetupLanguages",
                principalColumn: "Id");
        }
    }
}
