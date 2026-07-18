using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Configurations;

// Module 5 - Session 2 - Exercise 4: IEntityTypeConfiguration for each entity
// (updated in Exercise 5 with deliberate OnDelete behavior)
public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);

        // Module 5 - Session 2 - Exercise 5: deliberate delete-behavior decision.
        // Restrict: a course with active enrollments cannot be deleted outright.
        // This forces deliberate handling (e.g. archiving enrollments first)
        // instead of silently wiping enrollment history when a course is removed.
        builder.HasOne(e => e.Course)
            .WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cascade: if a student record itself is deleted, their own enrollment
        // history is deleted with them — no orphaned enrollments pointing nowhere.
        builder.HasOne(e => e.Student)
            .WithMany(s => s.Enrollments)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}