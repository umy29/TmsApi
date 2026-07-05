using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.RegistrationNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property<DateTime>("LastUpdated");

        builder.Property(s => s.Version).IsRowVersion();

        // Module 5 - Session 3 - Exercise 9: soft-delete query filter.
        // Normal queries automatically exclude deleted students;
        // use IgnoreQueryFilters() explicitly for admin/restore scenarios.
        builder.HasQueryFilter(s => !s.IsDeleted);
    }
}