namespace TmsApi.Domain.Entities;

// Module 5 - Session 1 - Exercise 1: Configure TmsDbContext and Apply the First Migration
// (updated in Session 3 - Exercise 8 with concurrency token, Exercise 9 with soft-delete flag)
public class Student
{
    public int Id { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string Name { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;

    // Module 5 - Session 3 - Exercise 9: soft-delete flag.
    // Filtered out of normal queries via HasQueryFilter in StudentConfiguration;
    // IgnoreQueryFilters() bypasses it for admin/restore scenarios.
    public bool IsDeleted { get; set; } = false;

    public uint Version { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}