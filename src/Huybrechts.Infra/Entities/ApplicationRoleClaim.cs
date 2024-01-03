using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection.Emit;

namespace Huybrechts.Infra.Entities;

[Table("IdentityRoleClaim")]
public class ApplicationRoleClaim : IdentityRoleClaim<string>
{
}

public class ApplicationRoleClaimConfiguration : IEntityTypeConfiguration<ApplicationRoleClaim>
{
    public void Configure(EntityTypeBuilder<ApplicationRoleClaim> builder)
    {
        builder.Property<int>("Id").ValueGeneratedOnAdd().HasColumnType("int");
        SqlServerPropertyBuilderExtensions.UseIdentityColumn(builder.Property<int>("Id"));
        builder.Property<string>("ClaimType").HasColumnType("nvarchar(max)");
        builder.Property<string>("ClaimValue").HasColumnType("nvarchar(max)");
        builder.Property<string>("RoleId").IsRequired().HasColumnType("nvarchar(450)");
        builder.HasKey("Id");
        builder.HasIndex("RoleId");
        builder.HasOne<ApplicationRole>().WithMany().HasForeignKey("RoleId").OnDelete(DeleteBehavior.Cascade).IsRequired();
        builder.ToTable("IdentityRoleClaim");
    }
}