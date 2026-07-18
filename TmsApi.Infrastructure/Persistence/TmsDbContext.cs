using Microsoft.EntityFrameworkCore;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence;

// Module 5 - Session 1 - Exercise 1, Step 2: Implement TmsDbContext
public class TmsDbContext(DbContextOptions<TmsDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Certificate> Certificates => Set<Certificate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TmsDbContext).Assembly);
    }

    // Module 5 - Session 3 - Exercise 8: automatically stamp the LastUpdated
    // shadow property on every Student that's added or modified, so no
    // controller needs to remember to set it manually.
    public override int SaveChanges()
    {
        UpdateAuditShadowProperties();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditShadowProperties();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateAuditShadowProperties()
    {
        foreach (var entry in ChangeTracker.Entries<Student>()
                     .Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            entry.Property("LastUpdated").CurrentValue = DateTime.UtcNow;
        }
    }
}