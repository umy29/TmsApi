using Asp.Versioning;
using TmsApi.Domain.Entities;
using TmsApi.Api.Filters;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

// Module 6 - Session 2 - Exercise 4, Part D: register the audit filter globally.
// Add<T>() (generic overload) lets DI resolve ILogger<AuditLogFilter> —
// Add(new AuditLogFilter(...)) would force manual construction, unnecessary.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
    });

builder.Services.AddAuthorization();

// Exercise 2 registrations
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Exercise 6: ProblemDetails
builder.Services.AddProblemDetails();

// Exercise 7: OpenAPI
builder.Services.AddOpenApi();

// Module 5 - Session 3 - Exercise 8: register a factory so we can create
// independent DbContext instances on demand — needed to correctly simulate
// two separate "sessions" for the concurrency-conflict test below.
builder.Services.AddDbContextFactory<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging());

// Also register a scoped TmsDbContext built from the factory, so existing
// controllers that inject TmsDbContext directly keep working unchanged.
builder.Services.AddScoped<TmsDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<TmsDbContext>>().CreateDbContext());

    // Module 6 - Session 1 - Exercise 1, Step 2: register ICourseService scoped —
// same lifetime as TmsDbContext (fresh per request), since CourseService
// depends on it. Singleton would capture the DbContext forever and crash
// the second request; transient would allocate a new CourseService pointlessly.
builder.Services.AddScoped<ICourseService, CourseService>();

// Module 6 - Session 1 - Exercise 3: register scoped, matches TmsDbContext's lifetime.
builder.Services.AddScoped<ICourseEnrollmentService, CourseEnrollmentService>();

// Module 7 - Session 1 - Exercise 1, Step 1: Configure versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapGet("/api/enrollments/worker-smoke", async (EnrollmentWorker worker) =>
{
    await worker.ProcessBatch();
    return Results.Ok("processed");
});

app.MapGet("/api/error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing");
});

// Module 5 - Session 1 - Exercise 2, Step 2: Write an Auto-Seeder
// Populates the database with sample data on startup, but only if it's empty —
// prevents duplicate rows on every restart.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();

    // Database.Migrate() (not EnsureCreated()) respects migration history,
    // so future `dotnet ef database update` calls keep working correctly.
    context.Database.Migrate();

    if (!context.Students.Any())
    {
        var students = new List<Student>
        {
            new() { RegistrationNumber = "TMS-2026-0001", Name = "Alice Smith", GPA = 3.8m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0002", Name = "Bob Jones", GPA = 2.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0003", Name = "Charlie Brown", GPA = 3.4m, IsActive = false },
            new() { RegistrationNumber = "TMS-2026-0004", Name = "Diana Prince", GPA = 3.9m, IsActive = true },
            new() { RegistrationNumber = "TMS-2026-0005", Name = "Evan Wright", GPA = 2.5m, IsActive = true }
        };
        context.Students.AddRange(students);

        var courses = new List<Course>
{
    new() { Code = "CS-101", Title = "Introduction to Computer Science", MaxCapacity = 30 },
    new() { Code = "CS-201", Title = "Data Structures and Algorithms", MaxCapacity = 25 },
    new() { Code = "MAT-101", Title = "Calculus I", MaxCapacity = 40 }
};
        context.Courses.AddRange(courses);

        // Save students/courses first so they get real Id values,
        // needed below to build the enrollment foreign keys.
        context.SaveChanges();

        var enrollments = new List<Enrollment>
        {
            new() { StudentId = students[0].Id, CourseId = courses[0].Id, Grade = 4.0m },
            new() { StudentId = students[0].Id, CourseId = courses[1].Id, Grade = 3.6m },
            new() { StudentId = students[1].Id, CourseId = courses[0].Id, Grade = 2.8m },
            new() { StudentId = students[3].Id, CourseId = courses[1].Id, Grade = 3.9m }
        };
        context.Enrollments.AddRange(enrollments);
        context.SaveChanges();
    }
}

// Module 6 - Session 2 - Before You Begin: run the deterministic seeder,
// Development only. IsDevelopment() gate matters — this should never run
// against production data owned by the operations team.
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seedContext = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(seedContext);
}
app.Run();