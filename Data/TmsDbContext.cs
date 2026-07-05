using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Data;

// Module 5 - Session 1 - Exercise 1, Step 2: Implement TmsDbContext
// EF Core's bridge between the C# domain model and PostgreSQL.
// Only entities registered here as DbSet<T> become actual database tables.
public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    // Module 5 - Session 1 - Extended Exercise (Stretch):
    // Wiring Assessment and Certificate into the EF Core model.
    // Adding a DbSet<T> is the one line that pulls a class into the model —
    // before this, Assessment/Certificate were plain C# classes with no backing table.
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
}