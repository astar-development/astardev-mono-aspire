using AStarDev.CloudSync.Accounts;
using AStarDev.CloudSync.Sync;

namespace AStarDev.CloudSync.Classification;

/// <summary>A classified file as a search result.</summary>
public sealed record FileSearchRow
{
    internal FileSearchRow(AccountId accountId, RelativeSyncPath path, string name, long? size, IReadOnlyList<string> categoryNames)
    {
        AccountId = accountId;
        Path = path;
        Name = name;
        Size = size;
        CategoryNames = categoryNames;
    }

    /// <summary>The account.</summary>
    public AccountId AccountId { get; }

    /// <summary>The path below the sync root.</summary>
    public RelativeSyncPath Path { get; }

    /// <summary>The last segment of the path.</summary>
    public string Name { get; }

    /// <summary>The size in bytes, when the baseline has one.</summary>
    public long? Size { get; }

    /// <summary>The distinct names of the categories of the file's terms, in order.</summary>
    public IReadOnlyList<string> CategoryNames { get; }
}
