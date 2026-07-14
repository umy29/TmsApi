namespace TmsApi.Dtos;

// Module 6 - Session 2 - Exercise 4, Part A: pagination output contract.
// TotalPages, HasPrevious, HasNext are computed properties — included in
// the JSON automatically via their public getters. Angular reads these
// to render pagination controls without a second round-trip.
public record PagedResponse<T>
{
    public required IReadOnlyList<T> Items { get; init; }
    public required int TotalCount { get; init; }
    public required int Page { get; init; }
    public required int PageSize { get; init; }

    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < TotalPages;
}