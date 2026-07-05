using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Entities;

namespace TmsApi.Data.Configurations;

// Module 5 - Session 2 - Exercise 4: IEntityTypeConfiguration for each entity
// (updated in Session 3 - Exercise 8 with shadow audit property + concurrency token)
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

        // Module 5 - Session 3 - Exercise 8: shadow property.
        // Exists in the database and EF model, but NOT on the Student class —
        // keeps audit concerns out of the domain entity.
        builder.Property<DateTime>("LastUpdated");
    }
}