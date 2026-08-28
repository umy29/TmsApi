namespace TmsApi.Application.Common;

// Module 7 - Session 1 - Exercise 2, Step 1: domain error value object.
// Code is the stable, machine-readable contract clients can branch on
// (error.Code === 'course_full'); Message is the human-readable detail.
public sealed record EnrollmentError(string Code, string Message)
{
    public static EnrollmentError CourseNotFound(string code) =>
        new("course_not_found", $"Course '{code}' was not found.");

    public static EnrollmentError CourseFull(string title, int capacity) =>
        new("course_full", $"Course '{title}' is full (capacity {capacity}).");

    public static EnrollmentError EnrollmentLimitReached(int studentId, int limit) =>
        new("enrollment_limit_reached", $"Student {studentId} has reached the maximum of {limit} enrollments.");
    public static EnrollmentError AlreadyEnrolled(int studentId, string code) =>
        new("already_enrolled", $"Student {studentId} is already enrolled in {code}.");
}
