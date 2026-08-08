namespace TmsApi.Application.Hubs;

public interface ITranscriptNotifier
{
    Task NotifyTranscriptReady(string studentId, string reportId, string downloadUrl, CancellationToken ct);
}
