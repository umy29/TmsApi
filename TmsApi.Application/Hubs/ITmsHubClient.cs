namespace TmsApi.Application.Hubs;

public interface ITmsHubClient
{
    Task ReceiveTranscriptReady(string reportId, string downloadUrl);
    Task ReceiveCourseUpdate(string courseCode, string message);
    Task ReceiveGradePosted(string courseCode, int studentId, decimal grade);

    // Module 9 - Session 3 - Exercise 5: broadcast enrollment status changes
    Task ReceiveEnrollmentStatusUpdated(string enrollmentId, string status);
}
