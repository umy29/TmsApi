using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

// Module 5 - Session 2 - Exercise 4: IEntityTypeConfiguration for each entity
// (updated in Module 6 - Session 1 - Before You Begin: Code max length changed
// from 20 to 10, unique index on Code added — this is what makes the
// duplicate-code check in Exercise 3 a real business rule, not just a
// DB-level courtesy reject)
public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.Code).IsUnique();

        builder.HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId);
    }
}