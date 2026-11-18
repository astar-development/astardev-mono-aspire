namespace AStarDev.CloudSync.Classification;

/// <summary>Creates <see cref="FileSearchQuery"/> values.</summary>
public static class FileSearchQueryFactory
{
    /// <summary>Creates the query, copying the lists. A negative offset becomes 0 and a limit below 1 becomes 1.</summary>
    /// <param name="text">The free text, or blank for none.</param>
    /// <param name="termIds">The terms to filter by.</param>
    /// <param name="categoryIds">The categories to filter by.</param>
    /// <param name="offset">The number of matching files to skip.</param>
    /// <param name="limit">The most files to return.</param>
    public static FileSearchQuery Create(string? text, IReadOnlyList<TermId> termIds, IReadOnlyList<CategoryId> categoryIds, int offset, int limit) =>
        new((text ?? string.Empty).Trim(), [.. termIds], [.. categoryIds], Math.Max(offset, 0), Math.Max(limit, 1));
}
