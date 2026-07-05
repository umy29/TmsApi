namespace TmsApi.Entities;

// Module 5 - Session 1 - Exercise 1: Configure TmsDbContext and Apply the First Migration
// (updated in Session 3 - Exercise 8 with a row-version concurrency token)
public class Student
{
    public int Id { get; set; }
    public required string RegistrationNumber { get; set; }
    public required string Name { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;

    // Module 5 - Session 3 - Exercise 8: concurrency token.
    // Prevents two staff members from silently overwriting each other's edits —
    // configured via IsRowVersion() in StudentConfiguration.
    public uint Version { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}