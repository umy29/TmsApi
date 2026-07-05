namespace TmsApi.Entities;

// Module 5 - Session 1 - Exercise 1: Configure TmsDbContext and Apply the First Migration
// Persistence-ready Student entity (mutable, surrogate int key) —
// evolved from the immutable TmsCore model (Module 1) for EF Core tracking.
public class Student
{
    public int Id { get; set; } // surrogate primary key — used by foreign keys
    public required string RegistrationNumber { get; set; } // natural key — uniqueness enforced in Session 2
    public required string Name { get; set; }
    public decimal GPA { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation property: one student can have many enrollments
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}