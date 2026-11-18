namespace AStarDev.CloudSync.Classification;

/// <summary>What to look for in classified files. Free text and each filter list narrow the result: a file must match the text, any selected term and any selected category. An empty list or blank text does not filter.</summary>
public sealed record FileSearchQuery
{
    internal FileSearchQuery(string text, IReadOnlyList<TermId> termIds, IReadOnlyList<CategoryId> categoryIds, int offset, int limit)
    {
        Text = text;
        TermIds = termIds;
        CategoryIds = categoryIds;
        Offset = offset;
        Limit = limit;
    }

    /// <summary>Matches the path, the text of a term of the file or the name of its category, ignoring case.</summary>
    public string Text { get; }

    /// <summary>A file must have a link to one of these terms, when any are given.</summary>
    public IReadOnlyList<TermId> TermIds { get; }

    /// <summary>A file must have a term in one of these categories, when any are given.</summary>
    public IReadOnlyList<CategoryId> CategoryIds { get; }

    /// <summary>The number of matching files to skip.</summary>
    public int Offset { get; }

    /// <summary>The most files to return.</summary>
    public int Limit { get; }
}
