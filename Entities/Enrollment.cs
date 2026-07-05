using System;

namespace TmsApi.Entities;

// Module 5 - Session 1 - Exercise 1: Configure TmsDbContext and Apply the First Migration
// (updated in Session 3 - Exercise 6 with IsArchived flag, used by Exercise 9's bulk archive)
public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public decimal? Grade { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Module 5 - Session 3 - Exercise 6: deliberate schema change,
    // set true once an enrollment is archived (see Exercise 9's bulk archive).
    public bool IsArchived { get; set; } = false;

    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}