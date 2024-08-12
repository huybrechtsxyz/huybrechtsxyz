﻿// <auto-generated />
using System;
using Huybrechts.App.Features.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Huybrechts.Infra.SqlServer.Migrations.Platform
{
    [DbContext(typeof(PlatformContext))]
    partial class PlatformContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
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

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

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

                    b.Property<string>("Category")
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)")
                        .HasComment("The category or family of the product, helping to classify it among similar offerings.");

                    b.Property<DateTime>("CreatedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Date time created");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasComment("A brief description providing additional details about the product.");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("A label representing the product, often used in the user interface.");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("The name of the product or service offered by the platform.");

                    b.Property<string>("PlatformInfoId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("Foreign key referencing the PlatformInfo entity.");

                    b.Property<string>("Remark")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Additional remarks or notes regarding the product.");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("PlatformInfoId", "Name")
                        .IsUnique();

                    b.ToTable("PlatformProduct", t =>
                        {
                            t.HasComment("Products or services offered by the platform, such as compute, storage, or networking resources.");
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

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

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

                    b.Property<string>("AboutURL")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)")
                        .HasComment("URL linking to additional information about the service.");

                    b.Property<string>("CostBasedOn")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Parameters or metrics on which the cost of the service is based.");

                    b.Property<string>("CostDriver")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasComment("The cost driver or factor that influences the pricing of the service.");

                    b.Property<DateTime>("CreatedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Date time created");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasComment("A brief description providing details about the service.");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("A label representing the service, often used for display purposes.");

                    b.Property<string>("Limitations")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)")
                        .HasComment("Limitations or constraints related to the service.");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("The name of the service offered by the platform.");

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

                    b.Property<string>("PricingURL")
                        .HasMaxLength(512)
                        .HasColumnType("nvarchar(512)")
                        .HasComment("URL providing pricing information for the service.");

                    b.Property<string>("Remark")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Additional remarks or notes regarding the service.");

                    b.Property<string>("ServiceFamily")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Service family or category");

                    b.Property<string>("ServiceId")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasComment("Original identifier used to reference the service.");

                    b.Property<string>("ServiceName")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)")
                        .HasComment("Original name of the service used for external identification.");

                    b.Property<string>("Size")
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Size or pricing tier associated with the service.");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("PlatformInfoId", "Name")
                        .IsUnique();

                    b.ToTable("PlatformService");

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
