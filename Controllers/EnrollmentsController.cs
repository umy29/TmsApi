using Microsoft.AspNetCore.Mvc;
using TmsApi.Dtos;
using TmsApi.Services;

namespace TmsApi.Controllers;

// Module 6 - Session 1 - Exercise 3: nested resource route under courses.
[ApiController]
[Route("api/courses/{courseId:int}/enrollments")]
public class EnrollmentsController(
    ICourseService courseService,
    ICourseEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet("{id:int}", Name = nameof(GetEnrollment))]
    public async Task<IActionResult> GetEnrollment(int courseId, int id, CancellationToken ct)
    {
        var enrollment = await enrollmentService.GetByIdAsync(courseId, id, ct);
        return enrollment is not null ? Ok(enrollment) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> EnrollStudent(int courseId, EnrollStudentRequest request, CancellationToken ct)
    {
        // Module 6 - Session 1 - Exercise 3: order matters — 404 before 409.
        // A client posting into a course that does not exist deserves the 404,
        // not a 409 about a course that was never there.
        var course = await courseService.GetByIdAsync(courseId, ct);
        if (course is null) return NotFound();

        if (course.EnrollmentCount >= course.MaxCapacity)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Course is full",
                Detail = $"Course '{course.Title}' has reached its maximum capacity of {course.MaxCapacity}.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var enrollment = await enrollmentService.CreateAsync(courseId, request, ct);
        return CreatedAtAction(nameof(GetEnrollment), new { courseId, id = enrollment.Id }, enrollment);
    }

    [HttpGet(Name = "ListCourseEnrollments")]
public async Task<IActionResult> GetEnrollments(int courseId, CancellationToken ct)
{
    var course = await courseService.GetByIdAsync(courseId, ct);
    if (course is null) return NotFound();

    return Ok(await enrollmentService.GetByCourseAsync(courseId, ct));
}
}