using Microsoft.EntityFrameworkCore;
using TmsApi.Data;
using TmsApi.Dtos;
using TmsApi.Entities;

namespace TmsApi.Services;

// Module 6 - Session 1 - Exercise 1: First REST Controller
// (updated in Exercise 2: project to DTOs at the query layer)
public class CourseService(TmsDbContext context, ILogger<CourseService> logger) : ICourseService
{
    // AsNoTracking(): read paths never need change tracking, saves CPU/memory.
    // Select(...) projection: EF translates c.Enrollments.Count into a SQL
    // COUNT(*) subquery — we never load every enrollment row into memory.
    public Task<CourseResponseDto?> GetByIdAsync(int id, CancellationToken ct) =>
        context.Courses
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CourseResponseDto(
                c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
            .FirstOrDefaultAsync(ct);

    public async Task<CourseResponseDto> CreateAsync(CreateCourseRequest request, CancellationToken ct)
    {
        var course = new Course
        {
            Code = request.Code,
            Title = request.Title,
            MaxCapacity = request.MaxCapacity
        };

        context.Courses.Add(course);
        await context.SaveChangesAsync(ct);

        logger.LogInformation("Created course {CourseId} ({Code})", course.Id, course.Code);

        // Re-query through GetByIdAsync so the response uses the same
        // projection — the null! is safe since we just inserted and saved.
        return (await GetByIdAsync(course.Id, ct))!;
    }

    // Module 6 - Session 1 - Exercise 3: duplicate-code business rule check.
// AnyAsync translates to SELECT EXISTS (SELECT 1 ... LIMIT 1) —
// stops at the first matching row, doesn't count everything.
public Task<bool> CodeExistsAsync(string code, CancellationToken ct) =>
    context.Courses.AsNoTracking().AnyAsync(c => c.Code == code, ct);

    // Module 6 - Session 2 - Exercise 4, Part C: paginated, filtered, sorted query.
// Order matters: filter, THEN count (before Skip/Take, or you get the count
// of the page instead of the total), THEN sort, THEN skip/take, THEN project.
public async Task<PagedResponse<CourseResponseDto>> GetCoursesAsync(PagedRequest request, CancellationToken ct)
{
    IQueryable<Course> query = context.Courses.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(request.Search))
    {
        // ILike is PostgreSQL's case-insensitive LIKE — "fund" finds
        // "Web Development Fundamentals" without case-sensitivity surprises.
        query = query.Where(c =>
            EF.Functions.ILike(c.Title, $"%{request.Search}%") ||
            EF.Functions.ILike(c.Code, $"%{request.Search}%"));
    }

    // Count BEFORE paging: one SELECT COUNT(*). Counting after Skip/Take
    // would give the count of the page, not the total.
    var totalCount = await query.CountAsync(ct);

    // Whitelist OrderBy values — never let an arbitrary string into the
    // LINQ tree. Unknown values silently fall back to "Title".
    IQueryable<Course> sortedQuery = request.OrderBy switch
    {
        "Code" => request.Descending
            ? query.OrderByDescending(c => c.Code)
            : query.OrderBy(c => c.Code),
        "MaxCapacity" => request.Descending
            ? query.OrderByDescending(c => c.MaxCapacity)
            : query.OrderBy(c => c.MaxCapacity),
        _ => request.Descending
            ? query.OrderByDescending(c => c.Title)
            : query.OrderBy(c => c.Title),
    };

    var items = await sortedQuery
        .Skip((request.Page - 1) * request.PageSize)
        .Take(request.PageSize)
        .Select(c => new CourseResponseDto(c.Id, c.Code, c.Title, c.MaxCapacity, c.Enrollments.Count))
        .ToListAsync(ct);

    return new PagedResponse<CourseResponseDto>
    {
        Items = items,
        TotalCount = totalCount,
        Page = request.Page,
        PageSize = request.PageSize
    };
}
}