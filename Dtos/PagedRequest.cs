namespace TmsApi.Dtos;

// Module 6 - Session 2 - Exercise 4, Part A: pagination input contract.
// MaxPageSize is a single source of truth, never inlined as a magic number.
// The PageSize setter clamps both ends: ?pageSize=10000 lands on 50,
// ?pageSize=0 lands on the default 20.
public record PagedRequest
{
    private const int MaxPageSize = 50;
    private int _pageSize = 20;

    public int Page { get; init; } = 1;

    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = value < 1 ? 20 : value > MaxPageSize ? MaxPageSize : value;
    }

    public string? Search { get; init; }
    public string OrderBy { get; init; } = "Title";
    public bool Descending { get; init; }
}