namespace MinimalClean.Application.Abstractions;

public record PagedResult<T>(IReadOnlyList<T> Items, int PageNumber, int PageSize, int TotalCount);