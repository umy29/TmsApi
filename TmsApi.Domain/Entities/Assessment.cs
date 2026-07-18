namespace TmsApi.Domain.Entities;

// Module 5 - Session 1 - Extended Exercise (Stretch): Wire Assessment and Certificate into the Database
// A quiz or practical task belonging to one Course.
// Defined here in Exercise 1 but NOT yet wired into TmsDbContext —
// connected as a DbSet in the Extended Exercise.
public class Assessment
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public decimal MaxScore { get; set; }
    public decimal Weight { get; set; } // share of final grade, e.g. 0.30m for 30%

    public int CourseId { get; set; } // foreign key -> Course
    public Course Course { get; set; } = null!;
}