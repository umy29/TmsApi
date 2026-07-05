using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Data;

// Module 5 - Session 1 - Exercise 1, Step 2: Implement TmsDbContext
// EF Core's bridge between the C# domain model and PostgreSQL.
// Only entities registered here as DbSet<T> become actual database tables —
// Assessment and Certificate are deliberately NOT registered yet (see Extended Exercise).
public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
}