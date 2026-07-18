using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

// Module 6 - Session 1 - Exercise 3: enrollment service for the REST API.
public class CourseEnrollmentService(TmsDbContext context, ILogger<CourseEnrollmentService> logger)
    : ICourseEnrollmentService
{
  public Task<EnrollmentResponseDto?> GetByIdAsync(int courseId, int id, CancellationToken ct) =>
        context.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == id && e.CourseId == courseId)
            .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
            .FirstOrDefaultAsync(ct);

    public async Task<EnrollmentResponseDto> CreateAsync(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        var enrollment = new Enrollment
        {
            CourseId = courseId,
            StudentId = request.StudentId,
            EnrolledAt = DateTime.UtcNow
        };

        context.Enrollments.Add(enrollment);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Enrolled student {StudentId} in course {CourseId}", request.StudentId, courseId);

        return (await GetByIdAsync(courseId, enrollment.Id, ct))!;
    }

    // Module 6 - Session 3 - Exercise 5, Step 6: list all enrollments for a course.
// Same pattern as GetByIdAsync — AsNoTracking, Where, Select to DTO, ToListAsync.
public Task<List<EnrollmentResponseDto>> GetByCourseAsync(int courseId, CancellationToken ct) =>
    context.Enrollments
        .AsNoTracking()
        .Where(e => e.CourseId == courseId)
        .Select(e => new EnrollmentResponseDto(e.Id, e.CourseId, e.StudentId, e.EnrolledAt))
        .ToListAsync(ct);
}