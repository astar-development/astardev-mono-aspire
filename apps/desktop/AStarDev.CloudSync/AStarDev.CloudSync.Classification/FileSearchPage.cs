namespace AStarDev.CloudSync.Classification;

/// <summary>One page of file search results.</summary>
public sealed record FileSearchPage
{
    internal FileSearchPage(IReadOnlyList<FileSearchRow> rows, bool hasMore)
    {
        Rows = rows;
        HasMore = hasMore;
    }

    /// <summary>The files of the page.</summary>
    public IReadOnlyList<FileSearchRow> Rows { get; }

    /// <summary>True when more matching files follow this page.</summary>
    public bool HasMore { get; }
}
