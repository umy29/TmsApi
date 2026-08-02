using TmsApi.Domain.Entities;

namespace TmsApi.Application.Interfaces;

// Module 7 - Session 1 - Exercise 2: repository contract for
// EnrollStudentHandler and GetStudentScheduleHandler.
public interface IEnrollmentRepository
{
    Task<bool> ExistsAsync(int studentId, string courseCode, CancellationToken ct);
    Task AddAsync(Enrollment enrollment, CancellationToken ct);

    // Must include Course so the schedule query can read Course.Code/Title
    // without a second round trip.
    Task<List<Enrollment>> GetByStudentIdAsync(int studentId, CancellationToken ct);
}
