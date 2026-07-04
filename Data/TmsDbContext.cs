using Microsoft.EntityFrameworkCore;
using TmsApi.Entities;

namespace TmsApi.Data;

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