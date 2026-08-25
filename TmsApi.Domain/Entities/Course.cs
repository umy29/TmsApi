namespace TmsApi.Domain.Entities;

// Module 5 - Session 1 - Exercise 1: Configure TmsDbContext and Apply the First Migration
// (updated in Module 6 - Session 1: renamed Capacity -> MaxCapacity)
// (updated in Module 11 - Session 3 - Exercise 5: add InstructorId for resource-based auth)
public class Course
{
    public int Id { get; set; }
    public required string Code { get; set; }
    public required string Title { get; set; }
    public int MaxCapacity { get; set; }
    public string? InstructorId { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
