﻿// <auto-generated />
using System;
using Huybrechts.App.Features.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Huybrechts.Infra.Npgsql.Migrations.Platform
{
    [DbContext(typeof(PlatformContext))]
    partial class PlatformContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformInfo", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasComment("Platform PK");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasComment("Description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Name");

                    b.Property<string>("Remark")
                        .HasColumnType("text")
                        .HasComment("Remark");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.ToTable("Platform");

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformLocation", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformLocation PK");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasComment("Description");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Label");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Name");

                    b.Property<string>("Remark")
                        .HasColumnType("text")
                        .HasComment("Remark");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.ToTable("PlatformLocation");

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformMeasureDefault", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformMeasureDefault PK");

                    b.Property<decimal>("DefaultValue")
                        .HasPrecision(12, 4)
                        .HasColumnType("numeric(12,4)")
                        .HasComment("Default unit rate");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasComment("Measuring unit description");

                    b.Property<string>("PlatformMeasureUnitId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformMeasureUnit FK");

                    b.Property<string>("PlatformProviderId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformProvider FK");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<decimal>("UnitFactor")
                        .HasPrecision(12, 6)
                        .HasColumnType("numeric(12,6)")
                        .HasComment("Conversion factor");

                    b.Property<string>("UnitOfMeasure")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasComment("Unit of measure");

                    b.HasKey("Id");

                    b.ToTable("PlatformMeasureDefault");

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformMeasureUnit", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformMeasure PK");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Name");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.ToTable("PlatformMeasure");

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRate", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformRate PK");

                    b.Property<string>("CurrencyCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasComment("Currency code");

                    b.Property<bool?>("IsPrimaryRegion")
                        .HasColumnType("boolean")
                        .HasComment("Is primary meter region");

                    b.Property<string>("MeterId")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasComment("Meter id");

                    b.Property<string>("MeterName")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Meter name");

                    b.Property<decimal>("MininumUnits")
                        .HasPrecision(12, 4)
                        .HasColumnType("numeric(12,4)")
                        .HasComment("Tier mininum units");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Name");

                    b.Property<string>("PlatformLocationId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformLocation FK");

                    b.Property<string>("PlatformProviderId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformProvider FK");

                    b.Property<string>("PlatformResourceId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformResource FK");

                    b.Property<string>("PlatformServiceId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformService FK");

                    b.Property<string>("Product")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Product");

                    b.Property<string>("Remark")
                        .HasColumnType("text")
                        .HasComment("Remark");

                    b.Property<decimal>("RetailPrice")
                        .HasPrecision(12, 4)
                        .HasColumnType("numeric(12,4)")
                        .HasComment("Retail price");

                    b.Property<string>("Sku")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Sku name");

                    b.Property<string>("SkuId")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasComment("Sku id");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Type")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Rate type");

                    b.Property<string>("UnitOfMeasure")
                        .HasMaxLength(32)
                        .HasColumnType("character varying(32)")
                        .HasComment("Azure rate unit of measure");

                    b.Property<decimal>("UnitPrice")
                        .HasPrecision(12, 4)
                        .HasColumnType("numeric(12,4)")
                        .HasComment("Unit price");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Rate is valid from");

                    b.HasKey("Id");

                    b.HasIndex("PlatformResourceId");

                    b.ToTable("PlatformRate");

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRateUnit", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformRateUnit PK");

                    b.Property<decimal>("DefaultValue")
                        .HasPrecision(12, 4)
                        .HasColumnType("numeric(12,4)")
                        .HasComment("Default unit rate");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("character varying(200)")
                        .HasComment("Measuring unit description");

                    b.Property<string>("PlatformMeasureUnitId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformMeasureUnit FK");

                    b.Property<string>("PlatformProviderId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformProvider FK");

                    b.Property<string>("PlatformRateId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformRate FK");

                    b.Property<string>("PlatformResourceId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformResource FK");

                    b.Property<string>("PlatformServiceId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformService FK");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<decimal>("UnitFactor")
                        .HasPrecision(12, 6)
                        .HasColumnType("numeric(12,6)")
                        .HasComment("Conversion factor");

                    b.Property<string>("UnitOfMeasure")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)")
                        .HasComment("Unit of measure");

                    b.HasKey("Id");

                    b.HasIndex("PlatformRateId");

                    b.ToTable("PlatformRateUnit");

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformResource", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformResource PK");

                    b.Property<string>("AboutURL")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)")
                        .HasComment("Resource url");

                    b.Property<string>("CostBasedOn")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Cost based on");

                    b.Property<string>("CostDriver")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasComment("Cost driver");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasComment("Description");

                    b.Property<string>("Limitations")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)")
                        .HasComment("Resource limitations");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Name");

                    b.Property<string>("PlatformProviderId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformProvider FK");

                    b.Property<string>("PlatformServiceId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformService FK");

                    b.Property<string>("PricingURL")
                        .HasMaxLength(512)
                        .HasColumnType("character varying(512)")
                        .HasComment("Pricing url");

                    b.Property<string>("Product")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasComment("Product");

                    b.Property<string>("ProductId")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)")
                        .HasComment("Product id");

                    b.Property<string>("Remarks")
                        .HasColumnType("text")
                        .HasComment("Remarks");

                    b.Property<string>("Size")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Resource size");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.HasIndex("PlatformServiceId");

                    b.ToTable("PlatformResource");

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformSearchRate", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformSearchRate PK");

                    b.Property<string>("Category")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasComment("Service category")
                        .HasAnnotation("Relational:JsonPropertyName", "serviceFamily");

                    b.Property<string>("CurrencyCode")
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)")
                        .HasComment("Currency code")
                        .HasAnnotation("Relational:JsonPropertyName", "currencyCode");

                    b.Property<bool?>("IsPrimaryRegion")
                        .HasColumnType("boolean")
                        .HasComment("Is primary meter region")
                        .HasAnnotation("Relational:JsonPropertyName", "isPrimaryMeterRegion");

                    b.Property<string>("Location")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasComment("Location name")
                        .HasAnnotation("Relational:JsonPropertyName", "location");

                    b.Property<string>("MeterId")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasComment("Meter id")
                        .HasAnnotation("Relational:JsonPropertyName", "meterId");

                    b.Property<string>("MeterName")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasComment("Meter name")
                        .HasAnnotation("Relational:JsonPropertyName", "meterName");

                    b.Property<decimal>("MiminumUnits")
                        .HasPrecision(12, 4)
                        .HasColumnType("numeric(12,4)")
                        .HasComment("Tier miminum units")
                        .HasAnnotation("Relational:JsonPropertyName", "tierMinimumUnits");

                    b.Property<string>("PlatformProviderId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformProvider FK");

                    b.Property<string>("Product")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasComment("Product")
                        .HasAnnotation("Relational:JsonPropertyName", "productName");

                    b.Property<string>("ProductId")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasComment("Product id")
                        .HasAnnotation("Relational:JsonPropertyName", "productId");

                    b.Property<string>("Region")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasComment("Region")
                        .HasAnnotation("Relational:JsonPropertyName", "armRegionName");

                    b.Property<decimal>("RetailPrice")
                        .HasPrecision(12, 4)
                        .HasColumnType("numeric(12,4)")
                        .HasComment("Retail price")
                        .HasAnnotation("Relational:JsonPropertyName", "retailPrice");

                    b.Property<string>("Service")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasComment("Service name")
                        .HasAnnotation("Relational:JsonPropertyName", "serviceName");

                    b.Property<string>("ServiceId")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasComment("Service id")
                        .HasAnnotation("Relational:JsonPropertyName", "serviceId");

                    b.Property<string>("Sku")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasComment("Sku name")
                        .HasAnnotation("Relational:JsonPropertyName", "skuName");

                    b.Property<string>("SkuId")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)")
                        .HasComment("Sku id")
                        .HasAnnotation("Relational:JsonPropertyName", "skuId");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("Type")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasComment("Rate type")
                        .HasAnnotation("Relational:JsonPropertyName", "type");

                    b.Property<string>("UnitOfMeasure")
                        .HasMaxLength(30)
                        .HasColumnType("character varying(30)")
                        .HasComment("Azure rate unit of measure")
                        .HasAnnotation("Relational:JsonPropertyName", "unitOfMeasure");

                    b.Property<decimal>("UnitPrice")
                        .HasPrecision(12, 4)
                        .HasColumnType("numeric(12,4)")
                        .HasComment("Unit price")
                        .HasAnnotation("Relational:JsonPropertyName", "unitPrice");

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("timestamp with time zone")
                        .HasComment("Rate is valid from")
                        .HasAnnotation("Relational:JsonPropertyName", "effectiveStartDate");

                    b.HasKey("Id");

                    b.ToTable("PlatformSearchRate");

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformService", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformService PK");

                    b.Property<bool>("Allowed")
                        .HasColumnType("boolean")
                        .HasComment("Is the service allowed?");

                    b.Property<string>("Category")
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)")
                        .HasComment("Service Category");

                    b.Property<string>("Description")
                        .HasMaxLength(256)
                        .HasColumnType("character varying(256)")
                        .HasComment("Description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)")
                        .HasComment("Name");

                    b.Property<string>("PlatformInfoId")
                        .HasColumnType("character varying(26)");

                    b.Property<string>("PlatformMeasureDefaultId")
                        .HasColumnType("character varying(26)");

                    b.Property<string>("PlatformProviderId")
                        .IsRequired()
                        .HasColumnType("character varying(26)")
                        .HasComment("PlatformProvider FK");

                    b.Property<string>("Remark")
                        .HasColumnType("text")
                        .HasComment("Remark");

                    b.Property<string>("TenantId")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.HasKey("Id");

                    b.HasIndex("PlatformInfoId");

                    b.HasIndex("PlatformMeasureDefaultId");

                    b.ToTable("PlatformService");

                    b.HasAnnotation("Finbuckle:MultiTenant", true);
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRate", b =>
                {
                    b.HasOne("Huybrechts.Core.Platform.PlatformResource", null)
                        .WithMany("Rates")
                        .HasForeignKey("PlatformResourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRateUnit", b =>
                {
                    b.HasOne("Huybrechts.Core.Platform.PlatformRate", null)
                        .WithMany("RateUnits")
                        .HasForeignKey("PlatformRateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformResource", b =>
                {
                    b.HasOne("Huybrechts.Core.Platform.PlatformService", null)
                        .WithMany("Resources")
                        .HasForeignKey("PlatformServiceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformService", b =>
                {
                    b.HasOne("Huybrechts.Core.Platform.PlatformInfo", null)
                        .WithMany("Services")
                        .HasForeignKey("PlatformInfoId");

                    b.HasOne("Huybrechts.Core.Platform.PlatformMeasureDefault", null)
                        .WithMany("Services")
                        .HasForeignKey("PlatformMeasureDefaultId");
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformInfo", b =>
                {
                    b.Navigation("Services");
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformMeasureDefault", b =>
                {
                    b.Navigation("Services");
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformRate", b =>
                {
                    b.Navigation("RateUnits");
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformResource", b =>
                {
                    b.Navigation("Rates");
                });

            modelBuilder.Entity("Huybrechts.Core.Platform.PlatformService", b =>
                {
                    b.Navigation("Resources");
                });
#pragma warning restore 612, 618
        }
    }
}
