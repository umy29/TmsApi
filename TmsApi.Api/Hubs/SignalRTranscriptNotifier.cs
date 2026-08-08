using Microsoft.AspNetCore.SignalR;
using TmsApi.Application.Hubs;

namespace TmsApi.Api.Hubs;

public class SignalRTranscriptNotifier(IHubContext<TmsHub, ITmsHubClient> hubContext)
    : ITranscriptNotifier
{
    public async Task NotifyTranscriptReady(
        string studentId, string reportId, string downloadUrl, CancellationToken ct)
    {
        await hubContext.Clients
            .Group(GroupNames.Student(studentId))
            .ReceiveTranscriptReady(reportId, downloadUrl);
    }
}
