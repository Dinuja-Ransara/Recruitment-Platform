using Meridian.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Meridian.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        builder.Property(o => o.Name).IsRequired().HasMaxLength(200);
        builder.Property(o => o.Industry).HasMaxLength(120);
        builder.Property(o => o.Country).HasMaxLength(100);
        builder.Property(o => o.City).HasMaxLength(100);
        builder.Property(o => o.Website).HasMaxLength(250);
        builder.HasIndex(o => o.Name);
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.Property(d => d.Name).IsRequired().HasMaxLength(150);
        builder.Property(d => d.CostCentre).HasMaxLength(50);

        builder.HasOne(d => d.Organization)
            .WithMany(o => o.Departments)
            .HasForeignKey(d => d.OrganizationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
