using Microsoft.EntityFrameworkCore;
using TmsApi.Application.Interfaces;
using TmsApi.Domain.Entities;

namespace TmsApi.Infrastructure.Persistence.Repositories;

// Module 7 - Session 1 - Exercise 2: EF-backed enrollment repository.
public class EnrollmentRepository(TmsDbContext context) : IEnrollmentRepository
{
    public Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct) =>
        context.Enrollments
            .AnyAsync(e => e.StudentId == studentId && e.Course.Code == courseCode, ct);

    public async Task AddAsync(Enrollment enrollment, CancellationToken ct)
    {
        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);
    }

    public Task<List<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct) =>
        context.Enrollments
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId)
            .ToListAsync(ct);
}
