﻿// <auto-generated />
using System;
using Huybrechts.App.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Feature
{
    [DbContext(typeof(FeatureContext))]
    [Migration("20240826081949_CreatePlatformSchema")]
    partial class CreatePlatformSchema
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.8")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformInfo", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Primary Key");

                    b.Property<DateTime>("CreatedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Date time created");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasComment("Detailed description of the platform.");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Name of the platform.");

                    b.Property<int>("Provider")
                        .HasColumnType("int")
                        .HasComment("The platform's supported automation provider, enabling automated resource management.");

                    b.Property<string>("Remark")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Additional remarks or comments about the platform.");

                    b.Property<string>("SearchIndex")
                        .HasColumnType("nvarchar(450)")
                        .HasComment("This field will store the normalized, concatenated values for searching");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("SearchIndex");

                    b.ToTable("Platform", t =>
                        {
                            t.HasComment("Table storing information about platforms that offer compute resources, including cloud providers like Azure or Google, and on-premise solutions.");
                        });

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformProduct", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Primary Key");

                    b.Property<string>("AboutURL")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)")
                        .HasComment("URL linking to additional information about the product.");

                    b.Property<string>("Category")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Category of the product");

                    b.Property<string>("CostBasedOn")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Parameters or metrics on which the cost of the product is based.");

                    b.Property<string>("CostDriver")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasComment("The cost driver or factor that influences the pricing of the product.");

                    b.Property<DateTime>("CreatedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Date time created");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasComment("A brief description providing details about the product.");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("A label representing the product, often used for display purposes.");

                    b.Property<string>("Limitations")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)")
                        .HasComment("Limitations or constraints related to the product.");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("The name of the product offered by the platform.");

                    b.Property<string>("PlatformInfoId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key referencing the PlatformInfo entity.");

                    b.Property<string>("PricingTier")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Size or pricing tier associated with the product.");

                    b.Property<string>("PricingURL")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)")
                        .HasComment("URL providing pricing information for the product.");

                    b.Property<string>("Remark")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Additional remarks or notes regarding the product.");

                    b.Property<string>("SearchIndex")
                        .HasColumnType("nvarchar(450)")
                        .HasComment("This field will store the normalized, concatenated values for searching");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("SearchIndex");

                    b.HasIndex("PlatformInfoId", "Name")
                        .IsUnique();

                    b.ToTable("PlatformProduct", t =>
                        {
                            t.HasComment("Represents a product offered on a specific platform, detailing attributes such as the product's name, description, and other relevant metadata.");
                        });

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRate", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Primary Key");

                    b.Property<DateTime>("CreatedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Date time created");

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasComment("Currency code.");

                    b.Property<bool>("IsPrimaryRegion")
                        .HasColumnType("bit")
                        .HasComment("Indicates whether this is the primary rate for the region.");

                    b.Property<string>("MeterName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Meter name.");

                    b.Property<decimal>("MininumUnits")
                        .HasPrecision(12, 6)
                        .HasColumnType("decimal(12,6)")
                        .HasComment("Tier minimum units.");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("PlatformInfoId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key referencing the PlatformInfo entity.");

                    b.Property<string>("PlatformProductId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key referencing the PlatformProduct entity.");

                    b.Property<string>("PlatformRegionId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key referencing the PlatformRegion entity.");

                    b.Property<string>("PlatformServiceId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key referencing the PlatformService entity.");

                    b.Property<string>("ProductName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Product name.");

                    b.Property<string>("RateType")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Rate type.");

                    b.Property<string>("Remark")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Additional remarks or comments about the rate.");

                    b.Property<decimal>("RetailPrice")
                        .HasPrecision(12, 6)
                        .HasColumnType("decimal(12,6)")
                        .HasComment("Retail price.");

                    b.Property<string>("SearchIndex")
                        .HasColumnType("nvarchar(450)")
                        .HasComment("This field will store the normalized, concatenated values for searching");

                    b.Property<string>("ServiceFamily")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Service family or category.");

                    b.Property<string>("ServiceName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("The name of the service.");

                    b.Property<string>("SkuName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("SKU name.");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("UnitOfMeasure")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("nvarchar(32)")
                        .HasComment("Unit of measure.");

                    b.Property<decimal>("UnitPrice")
                        .HasPrecision(12, 6)
                        .HasColumnType("decimal(12,6)")
                        .HasComment("Unit price.");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("datetime2")
                        .HasComment("Rate is valid from.");

                    b.HasKey("Id");

                    b.HasIndex("PlatformProductId");

                    b.HasIndex("SearchIndex");

                    b.ToTable("PlatformRate", t =>
                        {
                            t.HasComment("Represents the pricing rate of a service on a platform, including details such as the currency, price, and validity period.");
                        });

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRateUnit", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Primary Key");

                    b.Property<DateTime>("CreatedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Date time created");

                    b.Property<decimal>("DefaultValue")
                        .HasPrecision(12, 4)
                        .HasColumnType("decimal(12,4)")
                        .HasComment("Default rate for the unit, representing a base measurement standard.");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)")
                        .HasComment("Description of the measuring unit, providing additional context for users.");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("PlatformInfoId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key referencing the PlatformInfo entity.");

                    b.Property<string>("PlatformProductId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key referencing the PlatformProduct entity.");

                    b.Property<string>("PlatformRateId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key referencing the PlatformProduct entity.");

                    b.Property<string>("SearchIndex")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("This field will store the normalized, concatenated values for searching");

                    b.Property<string>("SetupUnitId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key linking to the SetupUnit entity.");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<decimal>("UnitFactor")
                        .HasPrecision(12, 6)
                        .HasColumnType("decimal(12,6)")
                        .HasComment("Conversion factor for the unit rate, translating platform units to standard units.");

                    b.HasKey("Id");

                    b.HasIndex("PlatformRateId");

                    b.HasIndex("SetupUnitId");

                    b.ToTable("PlatformRateUnit", t =>
                        {
                            t.HasComment("Table representing a unit of measurement for a rate within a platform's product offering, translating platform-specific units into standard project metrics.");
                        });

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRegion", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Primary Key");

                    b.Property<DateTime>("CreatedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Date time created");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasComment("A brief description providing additional details about the region.");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("A label representing the region, often the location name.");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("The unique name identifier of the region.");

                    b.Property<string>("PlatformInfoId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key referencing the PlatformInfo.");

                    b.Property<string>("Remark")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Additional remarks or notes regarding the region.");

                    b.Property<string>("SearchIndex")
                        .HasColumnType("nvarchar(450)")
                        .HasComment("This field will store the normalized, concatenated values for searching");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("SearchIndex");

                    b.HasIndex("PlatformInfoId", "Name")
                        .IsUnique();

                    b.ToTable("PlatformRegion", t =>
                        {
                            t.HasComment("Regions supported by the platform, representing data center locations.");
                        });

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformService", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Primary Key");

                    b.Property<string>("Category")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("The category or family of the service, helping to classify it among similar offerings.");

                    b.Property<DateTime>("CreatedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Date time created");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasComment("A brief description providing additional details about the service.");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("A label representing the service, often used in the user interface.");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("The name of the service or service offered by the platform.");

                    b.Property<string>("PlatformInfoId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key referencing the PlatformInfo entity.");

                    b.Property<string>("Remark")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Additional remarks or notes regarding the service.");

                    b.Property<string>("SearchIndex")
                        .HasColumnType("nvarchar(450)")
                        .HasComment("This field will store the normalized, concatenated values for searching");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("SearchIndex");

                    b.HasIndex("PlatformInfoId", "Name")
                        .IsUnique();

                    b.ToTable("PlatformService", t =>
                        {
                            t.HasComment("Services offered by the platform, such as compute, storage, or networking resources.");
                        });

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Setup.SetupUnit", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Primary Key");

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)")
                        .HasComment("A unique code representing the unit, standard across all instances.");

                    b.Property<DateTime>("CreatedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Date time created");

                    b.Property<string>("Description")
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)")
                        .HasComment("A detailed description of the unit.");

                    b.Property<decimal>("Factor")
                        .HasPrecision(18, 10)
                        .HasColumnType("decimal(18,10)")
                        .HasComment("A multiplication factor used to convert this unit to the base unit.");

                    b.Property<bool>("IsBase")
                        .HasColumnType("bit")
                        .HasComment("Indicates whether this unit is the base unit for its type.");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasComment("The unique name of the unit within its type.");

                    b.Property<int>("Precision")
                        .HasColumnType("int")
                        .HasComment("Number of decimal places for the unit.");

                    b.Property<int>("PrecisionType")
                        .HasColumnType("int")
                        .HasComment("Determines how values are rounded according to the System.Decimal Rounding enum.");

                    b.Property<string>("Remark")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Additional remarks or comments about the unit.");

                    b.Property<string>("SearchIndex")
                        .HasColumnType("nvarchar(450)")
                        .HasComment("This field will store the normalized, concatenated values for searching");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<int>("UnitType")
                        .HasMaxLength(32)
                        .HasColumnType("int")
                        .HasComment("Gets or sets the type of the unit (e.g., Height, Weight, Volume, System, etc.).");

                    b.HasKey("Id");

                    b.HasIndex("Code")
                        .IsUnique();

                    b.HasIndex("Name")
                        .IsUnique();

                    b.HasIndex("SearchIndex");

                    b.ToTable("SetupUnit", t =>
                        {
                            t.HasComment("Represents a measurement unit used for different types such as height, weight, volume, etc.");
                        });

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformProduct", b =>
                {
                    b.HasOne("Huybrechts.Core.Platform.PlatformInfo", "PlatformInfo")
                        .WithMany()
                        .HasForeignKey("PlatformInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlatformInfo");
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRate", b =>
                {
                    b.HasOne("Huybrechts.Core.Platform.PlatformProduct", "PlatformProduct")
                        .WithMany()
                        .HasForeignKey("PlatformProductId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlatformProduct");
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRateUnit", b =>
                {
                    b.HasOne("Huybrechts.Core.Platform.PlatformRate", "PlatformRate")
                        .WithMany()
                        .HasForeignKey("PlatformRateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Huybrechts.Core.Setup.SetupUnit", "SetupUnit")
                        .WithMany()
                        .HasForeignKey("SetupUnitId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlatformRate");

                    b.Navigation("SetupUnit");
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRegion", b =>
                {
                    b.HasOne("Huybrechts.Core.Platform.PlatformInfo", "PlatformInfo")
                        .WithMany()
                        .HasForeignKey("PlatformInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlatformInfo");
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformService", b =>
                {
                    b.HasOne("Huybrechts.Core.Platform.PlatformInfo", "PlatformInfo")
                        .WithMany()
                        .HasForeignKey("PlatformInfoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlatformInfo");
                });
#pragma warning restore 612, 618
        }
    }
}
