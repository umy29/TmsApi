using Asp.Versioning;
using TmsApi.Domain.Entities;
using TmsApi.Api.Filters;
using TmsApi.Application.Interfaces;
using TmsApi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using TmsApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Scalar.AspNetCore;
using MediatR;
using FluentValidation;
using TmsApi.Application.Enrollments.Commands;
using TmsApi.Application.Behaviors;
using TmsApi.Api.ExceptionHandlers;
using Microsoft.Extensions.Caching.Hybrid;
using System.Threading.RateLimiting;
using System.Threading.Channels;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Resilience;
using Microsoft.Extensions.Http.Resilience;
using Polly.DependencyInjection;
using Polly;
using Microsoft.AspNetCore.Antiforgery;
using TmsApi.Api.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
});
// Module 11 - Session 3 - Exercise 5: resource-based authorization policy
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("CanEditCourse", policy =>
        policy.Requirements.Add(new TmsApi.Api.Authorization.CourseInstructorRequirement()));
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    TmsApi.Api.Authorization.CourseInstructorHandler>();

// Module 11 - Session 2 - Exercise 3: JWT Bearer authentication
builder.Services.AddScoped<TmsApi.Infrastructure.Services.TokenService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();

// Exercise 2 registrations
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddSingleton<IEnrollmentService, EnrollmentService>();

// Module 7 - Session 3 - Exercise 5: transcript pipeline
builder.Services.AddSingleton(Channel.CreateBounded<TmsApi.Application.Transcripts.TranscriptRequest>(
    new System.Threading.Channels.BoundedChannelOptions(100)
    {
        FullMode = System.Threading.Channels.BoundedChannelFullMode.Wait
    }));
builder.Services.AddSingleton<TmsApi.Infrastructure.Transcripts.ITranscriptStatusStore, TmsApi.Infrastructure.Transcripts.InMemoryTranscriptStatusStore>();
builder.Services.AddHostedService<TmsApi.Infrastructure.Workers.TranscriptWorker>();

// Module 7 - Session 3 - Exercise 6: SignalR
builder.Services.AddSignalR();
builder.Services.AddSingleton<TmsApi.Application.Hubs.ITranscriptNotifier, TmsApi.Api.Hubs.SignalRTranscriptNotifier>();

builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Module 7 - Session 1 - Exercise 2, Step 8: MediatR + behaviors (Logging FIRST, then Validation)
// Module 11 - Session 1 - Exercise 2, Step 3: ASP.NET Core Identity
builder.Services.AddIdentityCore<TmsApi.Domain.Entities.TmsUser>(options =>
{
    // Enterprise Password Policy
    options.Password.RequiredLength = 12;
    options.Password.RequireUppercase = true;
    options.Password.RequireDigit = true;
    options.Password.RequireNonAlphanumeric = true;
    // Brute-Force Lockout Protection
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
.AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
.AddEntityFrameworkStores<TmsApi.Infrastructure.Persistence.TmsDbContext>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(EnrollStudentHandler).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(EnrollStudentValidator).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

// Module 7 - Session 1 - Exercise 2, Step 8: global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Module 10 - Session 2 - Exercise 2: antiforgery for XSRF protection
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
});
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(2)
    };
});

// Module 7 - Session 2 - Exercise 4: tier-aware rate limiting
// Module 10 - Session 1 - Exercise 1: named CORS policy from appsettings
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:4200" };
builder.Services.AddCors(options =>
{
    options.AddPolicy("TmsClient", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        var (partitionKey, tier) = ApiKeyResolver.Resolve(httpContext);
        return tier switch
        {
            ApiKeyTier.Paid => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"paid:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 200,
                    TokensPerPeriod = 100,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }),
            ApiKeyTier.Free => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"free:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 30,
                    TokensPerPeriod = 10,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }),
            _ => RateLimitPartition.GetTokenBucketLimiter(
                partitionKey: $"anon:{partitionKey}",
                factory: _ => new TokenBucketRateLimiterOptions
                {
                    TokenLimit = 10,
                    TokensPerPeriod = 5,
                    ReplenishmentPeriod = TimeSpan.FromSeconds(10),
                    QueueLimit = 0,
                    AutoReplenishment = true
                })
        };
    });
    options.AddConcurrencyLimiter("transcripts", opt =>
    {
        opt.PermitLimit = 5;
        opt.QueueLimit = 20;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = "10";
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ts))
            retryAfter = ((int)ts.TotalSeconds).ToString();
        context.HttpContext.Response.Headers.RetryAfter = retryAfter;
        context.HttpContext.Response.ContentType = "application/problem+json";
        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Title = "Rate limit exceeded",
            Detail = $"Too many requests. Retry after {retryAfter} seconds.",
            Status = StatusCodes.Status429TooManyRequests,
            Type = "https://tms.local/errors/rate_limit_exceeded"
        }, ct);
    };
});

// Module 7 - Session 4 - Exercise 8: Polly resilience pipeline
builder.Services.AddResiliencePipeline("certificate-api", pipeline =>
{
    pipeline
        .AddTimeout(TimeSpan.FromSeconds(5))
        .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            MinimumThroughput = 10,
            SamplingDuration = TimeSpan.FromSeconds(30),
            BreakDuration = TimeSpan.FromSeconds(15),
            ShouldHandle = new Polly.PredicateBuilder()
                .Handle<HttpRequestException>()
                .Handle<Polly.Timeout.TimeoutRejectedException>(),
            OnOpened = args => { Console.WriteLine("Circuit OPENED"); return ValueTask.CompletedTask; },
            OnClosed = args => { Console.WriteLine("Circuit CLOSED"); return ValueTask.CompletedTask; }
        })
        .AddRetry(new Polly.Retry.RetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(500),
            BackoffType = Polly.DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = new Polly.PredicateBuilder()
                .Handle<HttpRequestException>()
                .Handle<Polly.Timeout.TimeoutRejectedException>(),
            OnRetry = args =>
            {
                Console.WriteLine($"Retry #{args.AttemptNumber} after {args.RetryDelay.TotalMilliseconds:F0}ms");
                return ValueTask.CompletedTask;
            }
        });
});
builder.Services.AddHttpClient<ICertificateService, TmsApi.Infrastructure.ExternalServices.CertificateService>((sp, client) =>
{
    var baseUrl = sp.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>().GetValue<string>("TmsApi:PublicBaseUrl") ?? "http://localhost:5286";
    client.BaseAddress = new Uri(baseUrl);
});

// Exercise 7: OpenAPI
builder.Services.AddOpenApi();

// Module 7 - Session 4 - Exercise 9: health checks
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("alive"), tags: ["live"])
    .AddNpgSql(builder.Configuration.GetConnectionString("TmsDatabase")!, name: "postgres", tags: ["ready"]);

// Module 7 - Session 4 - Exercise 9: OpenTelemetry
const string ServiceName = "tms-api";
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName: ServiceName, serviceVersion: "1.0.0"))
    .WithTracing(t => t
        .AddSource(ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(m => m
        .AddMeter(ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter());

// Module 7 - Session 4 - Exercise 9: structured JSON logging
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
    options.JsonWriterOptions = new System.Text.Json.JsonWriterOptions { Indented = false };
});

builder.Services.AddDbContextFactory<TmsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("TmsDatabase"))
        .LogTo(Console.WriteLine, LogLevel.Information)
        .EnableSensitiveDataLogging());

builder.Services.AddScoped<TmsDbContext>(sp =>
    sp.GetRequiredService<IDbContextFactory<TmsDbContext>>().CreateDbContext());

builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<ICourseEnrollmentService, CourseEnrollmentService>();

// Module 7 - Session 1 - Exercise 2: register repositories for the CQRS handlers.
builder.Services.AddScoped<TmsApi.Application.Interfaces.ICourseRepository, TmsApi.Infrastructure.Persistence.Repositories.CourseRepository>();
builder.Services.AddScoped<TmsApi.Application.Interfaces.IEnrollmentRepository, TmsApi.Infrastructure.Persistence.Repositories.EnrollmentRepository>();
builder.Services.AddScoped<ICachedCourseService, CachedCourseService>();

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

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<TmsApi.Api.Middleware.V1DeprecationMiddleware>();

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseHttpsRedirection();

app.UseCors("TmsClient");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

// Module 10 - Session 2 - Exercise 2: issue readable XSRF-TOKEN cookie
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true ||
        context.Request.Cookies.ContainsKey("tms_auth"))
    {
        var antiforgery = context.RequestServices
            .GetRequiredService<IAntiforgery>();
        var tokens = antiforgery.GetAndStoreTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokens.RequestToken!,
            new CookieOptions
            {
                HttpOnly = false,
                Secure = !builder.Environment.IsDevelopment(),
                SameSite = SameSiteMode.Strict
            });
    }
    await next(context);
});

app.MapControllers();

// Module 7 - Session 4 - Exercise 9: health probes
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
}).DisableRateLimiting();
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).DisableRateLimiting();

// Module 7 - Session 4 - Exercise 8: lab-only fake certificate service
var attempts = 0;
app.MapPost("/fake/certificates", async () =>
{
    var n = Interlocked.Increment(ref attempts);
    if (n % 7 == 0) { await Task.Delay(TimeSpan.FromSeconds(20)); return Results.Ok(new { Status = "issued", Attempt = n }); }
    if (n % 3 != 0) return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    if (n % 11 == 0) return Results.BadRequest(new { error = "validation_failed" });
    return Results.Ok(new { Status = "issued", Attempt = n });
}).WithTags("lab-fixtures");
app.MapHub<TmsApi.Api.Hubs.TmsHub>("/hubs/tms").RequireCors("TmsClient");

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

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
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

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var seedContext = scope.ServiceProvider.GetRequiredService<TmsDbContext>();
    await DataSeeder.SeedAsync(seedContext);
}

app.Run();










