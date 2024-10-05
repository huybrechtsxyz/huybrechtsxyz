using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Huybrechts.Infra.Sqlite.Migrations.Feature
{
    /// <inheritdoc />
    public partial class CreateProjectPlatformSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Platform",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Name of the platform."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Detailed description of the platform."),
                    Provider = table.Column<int>(type: "INTEGER", nullable: false, comment: "The platform's supported automation provider, enabling automated resource management."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Additional remarks or comments about the platform."),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Platform", x => x.Id);
                },
                comment: "Table storing information about platforms that offer compute resources, including cloud providers like Azure or Google, and on-premise solutions.");

            migrationBuilder.CreateTable(
                name: "Project",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    ParentId = table.Column<string>(type: "TEXT", nullable: true, comment: "The project ID this component unit is part of."),
                    Code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, comment: "Code of the Project."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Name of the Project."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Detailed description of the Project."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Additional remarks or comments about the Project."),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Keywords or categories for the project"),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    State = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, comment: "Gets or sets the current state of the project."),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Gets or sets the reason for the current state of the project."),
                    StartDate = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the start date for the project."),
                    TargetDate = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the target completion date for the project."),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true, comment: "Gets or sets the priority of the project."),
                    Risk = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true, comment: "Gets or sets the risk of the project."),
                    Effort = table.Column<int>(type: "INTEGER", nullable: true, comment: "Gets or sets the effort required for the project."),
                    BusinessValue = table.Column<int>(type: "INTEGER", nullable: true, comment: "Gets or sets the business value of the project."),
                    Rating = table.Column<int>(type: "INTEGER", nullable: true, comment: "Gets or sets the rating of the project, reflecting its priority, quality, or stakeholder approval."),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Project", x => x.Id);
                },
                comment: "Table storing information about Projects that offer compute resources, including cloud providers like Azure or Google, and on-premise solutions.");

            migrationBuilder.CreateTable(
                name: "SetupCurrency",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    Code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupCurrency", x => x.Id);
                },
                comment: "Represents a currency entity with detailed information such as code, name, description, and associated country code.");

            migrationBuilder.CreateTable(
                name: "SetupLanguage",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    Code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    TranslatedName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupLanguage", x => x.Id);
                },
                comment: "Represents a currency entity with detailed information such as code, name, description, and associated country code.");

            migrationBuilder.CreateTable(
                name: "SetupState",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    ObjectType = table.Column<int>(type: "INTEGER", nullable: false),
                    StateType = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupState", x => x.Id);
                },
                comment: "Represents a custom state that can be applied to various objects, such as projects, constraints, requirements, and more.");

            migrationBuilder.CreateTable(
                name: "SetupUnit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    UnitType = table.Column<int>(type: "INTEGER", nullable: false, comment: "Gets or sets the type of the unit (e.g., Length, Mass, Volume, System, etc.)."),
                    Code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, comment: "A unique code representing the unit, standardized across all instances."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "The unique name of the unit within its type."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true, comment: "A detailed description of the unit."),
                    IsBase = table.Column<bool>(type: "INTEGER", nullable: false, comment: "Indicates whether this unit is the base unit for its type."),
                    Precision = table.Column<int>(type: "INTEGER", nullable: false, comment: "Number of decimal places for the unit."),
                    PrecisionType = table.Column<int>(type: "INTEGER", nullable: false, comment: "Determines how values are rounded according to the System.Decimal Rounding enum."),
                    Factor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 10, nullable: false, comment: "A multiplication factor used to convert this unit to the base unit."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Additional remarks or comments about the unit."),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching."),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupUnit", x => x.Id);
                },
                comment: "Represents a measurement unit used for different types such as length, mass, volume, etc.");

            migrationBuilder.CreateTable(
                name: "PlatformProduct",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformInfo entity."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "The name of the product offered by the platform."),
                    Label = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "A label representing the product, often used for display purposes."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "A brief description providing details about the product."),
                    Category = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Category of the product"),
                    CostDriver = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "The cost driver or factor that influences the pricing of the product."),
                    CostBasedOn = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Parameters or metrics on which the cost of the product is based."),
                    Limitations = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true, comment: "Limitations or constraints related to the product."),
                    AboutURL = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true, comment: "URL linking to additional information about the product."),
                    PricingURL = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true, comment: "URL providing pricing information for the product."),
                    PricingTier = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Size or pricing tier associated with the product."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Additional remarks or notes regarding the product."),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformProduct", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformProduct_Platform_PlatformInfoId",
                        column: x => x.PlatformInfoId,
                        principalTable: "Platform",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents a product offered on a specific platform, detailing attributes such as the product's name, description, and other relevant metadata.");

            migrationBuilder.CreateTable(
                name: "PlatformRegion",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformInfo."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "The unique name identifier of the region."),
                    Label = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "A label representing the region, often the location name."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "A brief description providing additional details about the region."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Additional remarks or notes regarding the region."),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformRegion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformRegion_Platform_PlatformInfoId",
                        column: x => x.PlatformInfoId,
                        principalTable: "Platform",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Regions supported by the platform, representing data center locations.");

            migrationBuilder.CreateTable(
                name: "PlatformService",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformInfo entity."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "The name of the service or service offered by the platform."),
                    Label = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "A label representing the service, often used in the user interface."),
                    Category = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "The category or family of the service, helping to classify it among similar offerings."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "A brief description providing additional details about the service."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Additional remarks or notes regarding the service."),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformService", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformService_Platform_PlatformInfoId",
                        column: x => x.PlatformInfoId,
                        principalTable: "Platform",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Services offered by the platform, such as compute, storage, or networking resources.");

            migrationBuilder.CreateTable(
                name: "ProjectDesign",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    ProjectInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the unique identifier for the project associated with this design."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Gets or sets the name of the project design."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Gets or sets the description of the project design."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Additional remarks or comments about the project design."),
                    Tags = table.Column<string>(type: "TEXT", nullable: true, comment: "Keywords or categories for the design"),
                    State = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, comment: "Gets or sets the current state of the project design."),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Gets or sets the reason for the current state of the design."),
                    Environment = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "The environment in which the project design is implemented."),
                    Version = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true, comment: "Design version"),
                    Dependencies = table.Column<string>(type: "TEXT", nullable: true, comment: "List of dependencies for the design"),
                    Priority = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true, comment: "Gets or sets the priority of the project."),
                    Risk = table.Column<string>(type: "TEXT", maxLength: 32, nullable: true, comment: "Gets or sets the risk of the project."),
                    Rating = table.Column<int>(type: "INTEGER", nullable: true, comment: "Gets or sets the rating of the project, reflecting its priority, quality, or stakeholder approval."),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectDesign", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectDesign_Project_ProjectInfoId",
                        column: x => x.ProjectInfoId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents a specific design or solution proposal for a project.");

            migrationBuilder.CreateTable(
                name: "ProjectQuantity",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    ProjectInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the unique identifier for the project associated with this bill of quantities."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false, comment: "Gets or sets the name of the bill of quantities."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true, comment: "Gets or sets a description of the bill of quantities."),
                    Remark = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true, comment: "Gets or sets any additional remarks for the bill of quantities."),
                    Tags = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Gets or sets the tags associated with the bill of quantities."),
                    SearchIndex = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Gets or sets the search index for the bill of quantities."),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectQuantity", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectQuantity_Project_ProjectInfoId",
                        column: x => x.ProjectInfoId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents a bill of quantities for a project, detailing the materials, parts, and labor required.");

            migrationBuilder.CreateTable(
                name: "ProjectScenario",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    ProjectInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the unique identifier for the project associated with this scenario."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Gets or sets the name of the scenario scenario."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Gets or sets the description of the project scenario."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Additional remarks or comments about the project scenario."),
                    Tags = table.Column<string>(type: "TEXT", nullable: true, comment: "Keywords or categories for the scenario"),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectScenario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectScenario_Project_ProjectInfoId",
                        column: x => x.ProjectInfoId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents different scenarios used to calculate design components and measures based on varying metrics.");

            migrationBuilder.CreateTable(
                name: "ProjectSimulation",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    ProjectInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the unique identifier for the project associated with this simulation."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Gets or sets the name of the simulation."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Gets or sets the description of the project simulation."),
                    IsCalculating = table.Column<bool>(type: "INTEGER", nullable: false, comment: "Gets or sets if the project simulation is being calculated on this moment."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Additional remarks or comments about the project simulation."),
                    Tags = table.Column<string>(type: "TEXT", nullable: true, comment: "Keywords or categories for the simulation"),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSimulation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectSimulation_Project_ProjectInfoId",
                        column: x => x.ProjectInfoId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents a simulation for a given project, containing various details and configurations related to the project's estimation.");

            migrationBuilder.CreateTable(
                name: "SetupCountry",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    SetupLanguageId = table.Column<string>(type: "TEXT", nullable: true),
                    SetupCurrencyId = table.Column<string>(type: "TEXT", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    TranslatedName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SetupCountry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SetupCountry_SetupCurrency_SetupCurrencyId",
                        column: x => x.SetupCurrencyId,
                        principalTable: "SetupCurrency",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SetupCountry_SetupLanguage_SetupLanguageId",
                        column: x => x.SetupLanguageId,
                        principalTable: "SetupLanguage",
                        principalColumn: "Id");
                },
                comment: "Represents information about different countries, including their codes, names, and associated details.");

            migrationBuilder.CreateTable(
                name: "PlatformDefaultUnit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformInfo."),
                    SetupUnitId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key linking to the SetupUnit entity."),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Unit of measure."),
                    UnitFactor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Conversion factor for the unit rate, translating platform units to standard units."),
                    DefaultValue = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false, comment: "Default rate for the unit, representing a base measurement standard."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "A brief description providing additional details about the region."),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformDefaultUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformDefaultUnit_Platform_PlatformInfoId",
                        column: x => x.PlatformInfoId,
                        principalTable: "Platform",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlatformDefaultUnit_SetupUnit_SetupUnitId",
                        column: x => x.SetupUnitId,
                        principalTable: "SetupUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents a default unit of measure for a platform, linking to a setup unit and a specific platform.");

            migrationBuilder.CreateTable(
                name: "PlatformRate",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformInfo entity."),
                    PlatformProductId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformProduct entity."),
                    PlatformRegionId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformRegion entity."),
                    PlatformServiceId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformService entity."),
                    ServiceName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "The name of the service."),
                    ServiceFamily = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Service family or category."),
                    ProductName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Product name."),
                    SkuName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "SKU name."),
                    MeterName = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Meter name."),
                    RateType = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Rate type."),
                    CurrencyCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, comment: "Currency code."),
                    ValidFrom = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Rate is valid from."),
                    RetailPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Retail price."),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Unit price."),
                    MinimumUnits = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Tier minimum units."),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Unit of measure."),
                    IsPrimaryRegion = table.Column<bool>(type: "INTEGER", nullable: false, comment: "Indicates whether this is the primary rate for the region."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Additional remarks or comments about the rate."),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformRate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformRate_PlatformProduct_PlatformProductId",
                        column: x => x.PlatformProductId,
                        principalTable: "PlatformProduct",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents the pricing rate of a service on a platform, including details such as the currency, price, and validity period.");

            migrationBuilder.CreateTable(
                name: "ProjectComponent",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    ProjectInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the ID of the project this component belongs to."),
                    ProjectDesignId = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the ID of the project design this component belongs to."),
                    ParentId = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the ID of the parent component, if any."),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false, comment: "Gets or sets the sequence order of this component within its parent design or component."),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false, comment: "Gets or sets the name of the component."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Gets or sets the description of the component."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Gets or sets any additional remarks or notes about the component."),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "A normalized search index to optimize searching through components."),
                    ComponentLevel = table.Column<int>(type: "INTEGER", nullable: false, comment: "Specifies the level of the component (e.g., Component, Configuration, Module, Variant)."),
                    VariantType = table.Column<int>(type: "INTEGER", nullable: false, comment: "Specifies the type of variant for this component (Standard, Option, Exceptional)."),
                    Proposal = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Gets or sets the proposal associated with the component."),
                    Account = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Gets or sets the account under which this component is managed."),
                    Organization = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Gets or sets the organization under which this component is managed."),
                    OrganizationalUnit = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Gets or sets the account under which this component is managed."),
                    Location = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Gets or sets the location associated with the component."),
                    Group = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Gets or sets the group to which this component belongs."),
                    Environment = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Gets or sets the environment associated with the component."),
                    Responsible = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "Gets or sets the responsible to which this component belongs."),
                    OwnershipPercentage = table.Column<int>(type: "INTEGER", nullable: false, comment: "Gets or sets the percentage of ownership for this component."),
                    SourceType = table.Column<int>(type: "INTEGER", nullable: false, comment: "Specifies the source type of this component (None, Platform)."),
                    Source = table.Column<string>(type: "TEXT", nullable: true, comment: "Optional field to store the source of this component."),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: true, comment: "Gets or sets the optional ID of the platform information associated with this component."),
                    PlatformProductId = table.Column<string>(type: "TEXT", nullable: true, comment: "Gets or sets the optional ID of the platform product associated with this component."),
                    ProjectComponentId = table.Column<string>(type: "TEXT", nullable: true),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectComponent_ProjectComponent_ProjectComponentId",
                        column: x => x.ProjectComponentId,
                        principalTable: "ProjectComponent",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectComponent_ProjectDesign_ProjectDesignId",
                        column: x => x.ProjectDesignId,
                        principalTable: "ProjectDesign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents a part of the design for a project.");

            migrationBuilder.CreateTable(
                name: "ProjectScenarioUnit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    ProjectInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the ID of the project this scenario metric belongs to."),
                    ProjectScenarioId = table.Column<string>(type: "TEXT", nullable: false),
                    SetupUnitId = table.Column<string>(type: "TEXT", nullable: true, comment: "The ID of the measuring unit used for this component."),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false, comment: "Gets or sets the sequence order of this unit within its parent scenario."),
                    Variable = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    Expression = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Gets or sets any additional remarks or notes about the scenario unit."),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectScenarioUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectScenarioUnit_ProjectScenario_ProjectScenarioId",
                        column: x => x.ProjectScenarioId,
                        principalTable: "ProjectScenario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectScenarioUnit_SetupUnit_SetupUnitId",
                        column: x => x.SetupUnitId,
                        principalTable: "SetupUnit",
                        principalColumn: "Id");
                },
                comment: "Represents a unit of measurement or metric used in a project scenario for calculating values over design components.");

            migrationBuilder.CreateTable(
                name: "PlatformRateUnit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformInfo entity."),
                    PlatformProductId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformProduct entity."),
                    PlatformRateId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformProduct entity."),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Unit of measure."),
                    SetupUnitId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key linking to the SetupUnit entity."),
                    UnitFactor = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Conversion factor for the unit rate, translating platform units to standard units."),
                    DefaultValue = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false, comment: "Default rate for the unit, representing a base measurement standard."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false, comment: "Description of the measuring unit, providing additional context for users."),
                    SearchIndex = table.Column<string>(type: "TEXT", nullable: true, comment: "This field will store the normalized, concatenated values for searching"),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlatformRateUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlatformRateUnit_PlatformRate_PlatformRateId",
                        column: x => x.PlatformRateId,
                        principalTable: "PlatformRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlatformRateUnit_SetupUnit_SetupUnitId",
                        column: x => x.SetupUnitId,
                        principalTable: "SetupUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Table representing a unit of measurement for a rate within a platform's product offering, translating platform-specific units into standard project metrics.");

            migrationBuilder.CreateTable(
                name: "ProjectComponentUnit",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    ProjectInfoId = table.Column<string>(type: "TEXT", nullable: false, comment: "The project ID this component unit is part of."),
                    ProjectDesignId = table.Column<string>(type: "TEXT", nullable: false, comment: "The design ID of the project this component unit belongs to."),
                    ProjectComponentId = table.Column<string>(type: "TEXT", nullable: false, comment: "The ID of the project component to which this unit belongs."),
                    SetupUnitId = table.Column<string>(type: "TEXT", nullable: true, comment: "The ID of the measuring unit used for this component."),
                    ProjectQuantityId = table.Column<string>(type: "TEXT", nullable: true, comment: "Gets or sets the unique identifier for the Project Quantity."),
                    Sequence = table.Column<int>(type: "INTEGER", nullable: false, comment: "Gets or sets the sequence order of this component within its parent design or component unit."),
                    Description = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Gets or sets the description of the unit."),
                    Variable = table.Column<string>(type: "TEXT", maxLength: 128, nullable: true, comment: "The variable name used in the metrics calculations for this component unit."),
                    Category = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, comment: "Category of the component unit (example: forfait, per unit, ...)"),
                    Expression = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "The formula used to calculate the value of the quantity for this component unit."),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Gets or sets the quantity of the unit used."),
                    SalesPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Gets or sets the sales price for a unit."),
                    RetailPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Gets or sets the standard price for a unit."),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Gets or sets the actual price you pay for a unit."),
                    Remark = table.Column<string>(type: "TEXT", nullable: true, comment: "Gets or sets any additional remarks or notes about the component unit."),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectComponentUnit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectComponentUnit_ProjectComponent_ProjectComponentId",
                        column: x => x.ProjectComponentId,
                        principalTable: "ProjectComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectComponentUnit_ProjectQuantity_ProjectQuantityId",
                        column: x => x.ProjectQuantityId,
                        principalTable: "ProjectQuantity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectComponentUnit_SetupUnit_SetupUnitId",
                        column: x => x.SetupUnitId,
                        principalTable: "SetupUnit",
                        principalColumn: "Id");
                },
                comment: "Links a project component to a measuring unit, allowing for cost calculation using metrics.");

            migrationBuilder.CreateTable(
                name: "ProjectSimulationEntry",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the primary key for the entity."),
                    ProjectSimulationId = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectInfoId = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectScenarioId = table.Column<string>(type: "TEXT", nullable: false),
                    ProjectDesignId = table.Column<string>(type: "TEXT", nullable: false, comment: "Gets or sets the ID of the project design this component belongs to."),
                    ProjectComponentId = table.Column<string>(type: "TEXT", nullable: false, comment: "The ID of the project component to which this unit belongs."),
                    ProjectQuantityId = table.Column<string>(type: "TEXT", nullable: true, comment: "Gets or sets the unique identifier for the Project Quantity."),
                    PlatformInfoId = table.Column<string>(type: "TEXT", nullable: true, comment: "Gets or sets the optional ID of the platform information associated with this component."),
                    PlatformProductId = table.Column<string>(type: "TEXT", nullable: true, comment: "Gets or sets the optional ID of the platform product associated with this component."),
                    PlatformRegionId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformRegion entity."),
                    PlatformServiceId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformService entity."),
                    PlatformRateId = table.Column<string>(type: "TEXT", nullable: false, comment: "Foreign key referencing the PlatformProduct entity."),
                    UnitDescription = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true, comment: "Gets or sets the description of the unit."),
                    UnitCategory = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true, comment: "Category of the component unit (example: forfait, per unit, ...)"),
                    CurrencyCode = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false, comment: "Currency code."),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Unit of measure."),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false, comment: "Quantity of service units."),
                    SalesPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Gets or sets the sales price for a unit."),
                    RetailPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Retail price in the specified currency."),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 6, nullable: false, comment: "Unit price per service unit in the specified currency."),
                    OwnershipPercentage = table.Column<int>(type: "INTEGER", nullable: false, comment: "Percentage of ownership for this component or service."),
                    SalesAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false, comment: "Internal retail cost in the specified currency."),
                    RetailAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false, comment: "Internal retail cost in the specified currency."),
                    UnitAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false, comment: "Internal unit cost per service unit in the specified currency."),
                    OwnSalesAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false, comment: "The sales revenue adjusted for the ownership percentage."),
                    OwnRetailAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false, comment: "Ownership-adjusted retail cost in the specified currency."),
                    OwnUnitAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false, comment: "Ownership-adjusted unit cost per service unit in the specified currency."),
                    TenantId = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, comment: "Gets or sets the tenant identifier."),
                    CreatedDT = table.Column<DateTime>(type: "TEXT", nullable: false, comment: "Date time created"),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who created the entity."),
                    ModifiedDT = table.Column<DateTime>(type: "TEXT", nullable: true, comment: "Gets or sets the last modified date and time for the entity."),
                    ModifiedBy = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true, comment: "Gets or sets the ID of the user who last modified the entity."),
                    TimeStamp = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true, defaultValueSql: "CURRENT_TIMESTAMP", comment: "Gets or sets the concurrency timestamp for the entity.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSimulationEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectSimulationEntry_PlatformProduct_PlatformProductId",
                        column: x => x.PlatformProductId,
                        principalTable: "PlatformProduct",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectSimulationEntry_PlatformRate_PlatformRateId",
                        column: x => x.PlatformRateId,
                        principalTable: "PlatformRate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectSimulationEntry_PlatformRegion_PlatformRegionId",
                        column: x => x.PlatformRegionId,
                        principalTable: "PlatformRegion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectSimulationEntry_PlatformService_PlatformServiceId",
                        column: x => x.PlatformServiceId,
                        principalTable: "PlatformService",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectSimulationEntry_Platform_PlatformInfoId",
                        column: x => x.PlatformInfoId,
                        principalTable: "Platform",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectSimulationEntry_ProjectComponent_ProjectComponentId",
                        column: x => x.ProjectComponentId,
                        principalTable: "ProjectComponent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectSimulationEntry_ProjectDesign_ProjectDesignId",
                        column: x => x.ProjectDesignId,
                        principalTable: "ProjectDesign",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectSimulationEntry_ProjectQuantity_ProjectQuantityId",
                        column: x => x.ProjectQuantityId,
                        principalTable: "ProjectQuantity",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProjectSimulationEntry_ProjectScenario_ProjectScenarioId",
                        column: x => x.ProjectScenarioId,
                        principalTable: "ProjectScenario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectSimulationEntry_ProjectSimulation_ProjectSimulationId",
                        column: x => x.ProjectSimulationId,
                        principalTable: "ProjectSimulation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectSimulationEntry_Project_ProjectInfoId",
                        column: x => x.ProjectInfoId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: " Represents a single entry in a project simulation");

            migrationBuilder.CreateIndex(
                name: "IX_Platform_TenantId_Name",
                table: "Platform",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Platform_TenantId_SearchIndex",
                table: "Platform",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDefaultUnit_PlatformInfoId",
                table: "PlatformDefaultUnit",
                column: "PlatformInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDefaultUnit_SetupUnitId",
                table: "PlatformDefaultUnit",
                column: "SetupUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDefaultUnit_TenantId_PlatformInfoId_UnitOfMeasure",
                table: "PlatformDefaultUnit",
                columns: new[] { "TenantId", "PlatformInfoId", "UnitOfMeasure" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformDefaultUnit_TenantId_SearchIndex",
                table: "PlatformDefaultUnit",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformProduct_PlatformInfoId",
                table: "PlatformProduct",
                column: "PlatformInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformProduct_TenantId_PlatformInfoId_Name",
                table: "PlatformProduct",
                columns: new[] { "TenantId", "PlatformInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformProduct_TenantId_SearchIndex",
                table: "PlatformProduct",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRate_PlatformProductId",
                table: "PlatformRate",
                column: "PlatformProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRate_TenantId_PlatformInfoId_PlatformProductId_ValidFrom",
                table: "PlatformRate",
                columns: new[] { "TenantId", "PlatformInfoId", "PlatformProductId", "ValidFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRate_TenantId_SearchIndex",
                table: "PlatformRate",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRateUnit_PlatformRateId",
                table: "PlatformRateUnit",
                column: "PlatformRateId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRateUnit_SetupUnitId",
                table: "PlatformRateUnit",
                column: "SetupUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRateUnit_TenantId_PlatformInfoId_PlatformProductId_PlatformRateId_UnitOfMeasure",
                table: "PlatformRateUnit",
                columns: new[] { "TenantId", "PlatformInfoId", "PlatformProductId", "PlatformRateId", "UnitOfMeasure" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRegion_PlatformInfoId",
                table: "PlatformRegion",
                column: "PlatformInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRegion_TenantId_PlatformInfoId_Name",
                table: "PlatformRegion",
                columns: new[] { "TenantId", "PlatformInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformRegion_TenantId_SearchIndex",
                table: "PlatformRegion",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_PlatformService_PlatformInfoId",
                table: "PlatformService",
                column: "PlatformInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformService_TenantId_PlatformInfoId_Name",
                table: "PlatformService",
                columns: new[] { "TenantId", "PlatformInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformService_TenantId_SearchIndex",
                table: "PlatformService",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Project_TenantId_Code",
                table: "Project",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Project_TenantId_SearchIndex",
                table: "Project",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectComponent_ProjectComponentId",
                table: "ProjectComponent",
                column: "ProjectComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectComponent_ProjectDesignId",
                table: "ProjectComponent",
                column: "ProjectDesignId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectComponent_TenantId_ProjectInfoId_ProjectDesignId_Sequence",
                table: "ProjectComponent",
                columns: new[] { "TenantId", "ProjectInfoId", "ProjectDesignId", "Sequence" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectComponent_TenantId_SearchIndex",
                table: "ProjectComponent",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectComponentUnit_ProjectComponentId",
                table: "ProjectComponentUnit",
                column: "ProjectComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectComponentUnit_ProjectQuantityId",
                table: "ProjectComponentUnit",
                column: "ProjectQuantityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectComponentUnit_SetupUnitId",
                table: "ProjectComponentUnit",
                column: "SetupUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDesign_ProjectInfoId",
                table: "ProjectDesign",
                column: "ProjectInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDesign_TenantId_ProjectInfoId_Name",
                table: "ProjectDesign",
                columns: new[] { "TenantId", "ProjectInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectDesign_TenantId_SearchIndex",
                table: "ProjectDesign",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectQuantity_ProjectInfoId",
                table: "ProjectQuantity",
                column: "ProjectInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectQuantity_TenantId_ProjectInfoId_Name",
                table: "ProjectQuantity",
                columns: new[] { "TenantId", "ProjectInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectQuantity_TenantId_SearchIndex",
                table: "ProjectQuantity",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectScenario_ProjectInfoId",
                table: "ProjectScenario",
                column: "ProjectInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectScenario_TenantId_ProjectInfoId_Name",
                table: "ProjectScenario",
                columns: new[] { "TenantId", "ProjectInfoId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectScenario_TenantId_SearchIndex",
                table: "ProjectScenario",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectScenarioUnit_ProjectScenarioId",
                table: "ProjectScenarioUnit",
                column: "ProjectScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectScenarioUnit_SetupUnitId",
                table: "ProjectScenarioUnit",
                column: "SetupUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulation_ProjectInfoId",
                table: "ProjectSimulation",
                column: "ProjectInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulationEntry_PlatformInfoId",
                table: "ProjectSimulationEntry",
                column: "PlatformInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulationEntry_PlatformProductId",
                table: "ProjectSimulationEntry",
                column: "PlatformProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulationEntry_PlatformRateId",
                table: "ProjectSimulationEntry",
                column: "PlatformRateId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulationEntry_PlatformRegionId",
                table: "ProjectSimulationEntry",
                column: "PlatformRegionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulationEntry_PlatformServiceId",
                table: "ProjectSimulationEntry",
                column: "PlatformServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulationEntry_ProjectComponentId",
                table: "ProjectSimulationEntry",
                column: "ProjectComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulationEntry_ProjectDesignId",
                table: "ProjectSimulationEntry",
                column: "ProjectDesignId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulationEntry_ProjectInfoId",
                table: "ProjectSimulationEntry",
                column: "ProjectInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulationEntry_ProjectQuantityId",
                table: "ProjectSimulationEntry",
                column: "ProjectQuantityId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulationEntry_ProjectScenarioId",
                table: "ProjectSimulationEntry",
                column: "ProjectScenarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSimulationEntry_ProjectSimulationId",
                table: "ProjectSimulationEntry",
                column: "ProjectSimulationId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupCountry_SetupCurrencyId",
                table: "SetupCountry",
                column: "SetupCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupCountry_SetupLanguageId",
                table: "SetupCountry",
                column: "SetupLanguageId");

            migrationBuilder.CreateIndex(
                name: "IX_SetupCountry_TenantId_Code",
                table: "SetupCountry",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupCountry_TenantId_Name",
                table: "SetupCountry",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupCountry_TenantId_SearchIndex",
                table: "SetupCountry",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupCurrency_TenantId_Code",
                table: "SetupCurrency",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupCurrency_TenantId_Name",
                table: "SetupCurrency",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupCurrency_TenantId_SearchIndex",
                table: "SetupCurrency",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupLanguage_TenantId_Code",
                table: "SetupLanguage",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupLanguage_TenantId_Name",
                table: "SetupLanguage",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupLanguage_TenantId_SearchIndex",
                table: "SetupLanguage",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupState_TenantId_ObjectType_Name",
                table: "SetupState",
                columns: new[] { "TenantId", "ObjectType", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupState_TenantId_SearchIndex",
                table: "SetupState",
                columns: new[] { "TenantId", "SearchIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_SetupUnit_TenantId_Code",
                table: "SetupUnit",
                columns: new[] { "TenantId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupUnit_TenantId_Name",
                table: "SetupUnit",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SetupUnit_TenantId_SearchIndex",
                table: "SetupUnit",
                columns: new[] { "TenantId", "SearchIndex" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlatformDefaultUnit");

            migrationBuilder.DropTable(
                name: "PlatformRateUnit");

            migrationBuilder.DropTable(
                name: "ProjectComponentUnit");

            migrationBuilder.DropTable(
                name: "ProjectScenarioUnit");

            migrationBuilder.DropTable(
                name: "ProjectSimulationEntry");

            migrationBuilder.DropTable(
                name: "SetupCountry");

            migrationBuilder.DropTable(
                name: "SetupState");

            migrationBuilder.DropTable(
                name: "SetupUnit");

            migrationBuilder.DropTable(
                name: "PlatformRate");

            migrationBuilder.DropTable(
                name: "PlatformRegion");

            migrationBuilder.DropTable(
                name: "PlatformService");

            migrationBuilder.DropTable(
                name: "ProjectComponent");

            migrationBuilder.DropTable(
                name: "ProjectQuantity");

            migrationBuilder.DropTable(
                name: "ProjectScenario");

            migrationBuilder.DropTable(
                name: "ProjectSimulation");

            migrationBuilder.DropTable(
                name: "SetupCurrency");

            migrationBuilder.DropTable(
                name: "SetupLanguage");

            migrationBuilder.DropTable(
                name: "PlatformProduct");

            migrationBuilder.DropTable(
                name: "ProjectDesign");

            migrationBuilder.DropTable(
                name: "Platform");

            migrationBuilder.DropTable(
                name: "Project");
        }
    }
}
