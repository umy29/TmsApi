namespace TmsApi.Entities;

// Module 5 - Session 1 - Exercise 1: Configure TmsDbContext and Apply the First Migration
public class Course
{
    public int Id { get; set; } // surrogate primary key
    public required string Code { get; set; } // natural key — e.g. "CS-101"
    public required string Title { get; set; }
    public int Capacity { get; set; }

    // Navigation property: one course can have many enrollments
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}