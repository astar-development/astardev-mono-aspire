namespace AStarDev.CloudSync.Classification;

/// <summary>Creates <see cref="FileSearchPage"/> values.</summary>
public static class FileSearchPageFactory
{
    /// <summary>Creates the page, copying the rows.</summary>
    /// <param name="rows">The files of the page.</param>
    /// <param name="hasMore">True when more matching files follow.</param>
    public static FileSearchPage Create(IReadOnlyList<FileSearchRow> rows, bool hasMore) => new([.. rows], hasMore);
}
