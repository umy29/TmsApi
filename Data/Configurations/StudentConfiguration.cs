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

        // Module 5 - Session 3 - Exercise 8: Npgsql maps IsRowVersion() to
        // PostgreSQL's built-in xmin system column — no new physical column
        // is created, EF just adopts xmin as the tracked concurrency token.
        builder.Property(s => s.Version).IsRowVersion();
    }
}