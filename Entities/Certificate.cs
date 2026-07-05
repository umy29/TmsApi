using System;

namespace TmsApi.Entities;

// Module 5 - Session 1 - Extended Exercise (Stretch): Wire Assessment and Certificate into the Database
// Issued to one Student for completing one Course.
// Defined here in Exercise 1 but NOT yet wired into TmsDbContext —
// connected as a DbSet in the Extended Exercise.
public class Certificate
{
    public int Id { get; set; } // surrogate primary key
    public required string SerialNumber { get; set; } // natural key — human-readable
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public int StudentId { get; set; } // foreign key -> Student
    public int CourseId { get; set; }  // foreign key -> Course
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}