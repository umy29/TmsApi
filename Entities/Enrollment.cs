using System;

namespace TmsApi.Entities;

// Module 5 - Session 1 - Exercise 1: Configure TmsDbContext and Apply the First Migration
// Join entity linking a Student to a Course, with grade and enrollment date.
public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; } // foreign key -> Student
    public int CourseId { get; set; }  // foreign key -> Course
    public decimal? Grade { get; set; } // nullable: student may still be enrolled (no final grade yet)
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    // Navigation properties back to the related entities
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}