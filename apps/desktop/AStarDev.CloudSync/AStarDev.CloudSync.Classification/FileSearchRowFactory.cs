using AStarDev.CloudSync.Accounts;
using AStarDev.CloudSync.Sync;

namespace AStarDev.CloudSync.Classification;

/// <summary>Creates <see cref="FileSearchRow"/> values.</summary>
public static class FileSearchRowFactory
{
    /// <summary>Creates the row, taking the name from the last segment of the path and copying the category names.</summary>
    /// <param name="accountId">The account.</param>
    /// <param name="path">The path below the sync root.</param>
    /// <param name="size">The size in bytes, when known.</param>
    /// <param name="categoryNames">The distinct category names.</param>
    public static FileSearchRow Create(AccountId accountId, RelativeSyncPath path, long? size, IReadOnlyList<string> categoryNames) =>
        new(accountId, path, NameOf(path.Value ?? string.Empty), size, [.. categoryNames]);

    private static string NameOf(string path) => path[(path.LastIndexOf('/') + 1)..];
}
