using Microsoft.Extensions.DependencyInjection;

public class EnrollmentWorker
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EnrollmentWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ProcessBatch()
    {
        // Create a short-lived scope
        using var scope = _scopeFactory.CreateScope();

        // Resolve the scoped service inside the scope
        var enrollmentService =
            scope.ServiceProvider.GetRequiredService<IEnrollmentService>();

        // Use the service
        await enrollmentService.EnrollAsync("S-001", "CS-101");

        // When this method exits, the scope and all scoped services are disposed.
    }
}