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
                        .HasComment("Description of the platform");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Name of the platform");

                    b.Property<int>("Provider")
                        .HasColumnType("int")
                        .HasComment("Supported automation providers of the platform");

                    b.Property<string>("Remark")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Remark about the platform");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.ToTable("Platform", t =>
                        {
                            t.HasComment("Platforms that provide compute resources");
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
                        .HasComment("Product category");

                    b.Property<DateTime>("CreatedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Date time created");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)")
                        .HasComment("Description");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Label of the product");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Name of the product");

                    b.Property<string>("PlatformInfoId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("PlatformInfo FK");

                    b.Property<string>("Remark")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Remark");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("PlatformInfoId");

                    b.ToTable("PlatformProduct");

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
                        .HasComment("Description of the region");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Label of the region");

                    b.Property<DateTime?>("ModifiedDT")
                        .HasColumnType("datetime2")
                        .HasComment("Modified time created");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)")
                        .HasComment("Name of the region");

                    b.Property<string>("PlatformInfoId")
                        .IsRequired()
                        .HasColumnType("nvarchar(26)")
                        .HasComment("PlatformInfo FK");

                    b.Property<string>("Remark")
                        .HasColumnType("nvarchar(max)")
                        .HasComment("Remark about the region");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("TimeStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("PlatformInfoId");

                    b.ToTable("PlatformRegion", t =>
                        {
                            t.HasComment("Support regions of the Platform");
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

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRegion", b =>
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
