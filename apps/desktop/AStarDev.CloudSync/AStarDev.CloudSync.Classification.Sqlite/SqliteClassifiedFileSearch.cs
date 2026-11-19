using System.Text.Json;
using AStarDev.CloudSync.Accounts;
using AStarDev.CloudSync.Sync;
using AStarDev.FunctionalParadigm;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AStarDev.CloudSync.Classification.Sqlite;

/// <summary>Searches the files that have links, joined to the <c>SyncBaseline</c> table of the same database file for the size and to drop files no longer in the baseline. One grouped query per page: it asks for one row more than the limit to know whether more follow. Filter lists travel as one JSON parameter each, so no SQLite parameter limit applies. Text matches by substring, ASCII case-insensitive for term text.</summary>
/// <param name="dbContextFactory">Creates a context per operation.</param>
/// <param name="logger">Reports reads that fail, by exception type only.</param>
internal sealed class SqliteClassifiedFileSearch(IDbContextFactory<ClassificationDbContext> dbContextFactory, ILogger<SqliteClassifiedFileSearch> logger) : IClassifiedFileSearch
{
    private const string Location = nameof(SqliteClassifiedFileSearch);

    /// <inheritdoc />
    public Task<Result<FileSearchPage, ClassificationFailure>> SearchAsync(FileSearchQuery query, CancellationToken cancellationToken) =>
        GuardedStore.RunAsync(logger, Location, () => ReadAsync(query, cancellationToken), cancellationToken);

    private static string GuidText(Guid value) => value.ToString("D").ToUpperInvariant();

    private async Task<Result<FileSearchPage, ClassificationFailure>> ReadAsync(FileSearchQuery query, CancellationToken cancellationToken)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var terms = JsonSerializer.Serialize(query.TermIds.Select(term => GuidText(term.Value)).ToArray());
        var categories = JsonSerializer.Serialize(query.CategoryIds.Select(category => GuidText(category.Value)).ToArray());
        var text = query.Text.ToUpperInvariant();
        var take = query.Limit + 1;
        var found = await dbContext.Database.SqlQuery<FoundFile>(
            $"""
            SELECT l."AccountId" AS "AccountId", MIN(l."Path") AS "Path", b."Size" AS "Size", json_group_array(DISTINCT c."Name") AS "Categories"
            FROM "FileLinks" l
            JOIN "Terms" t ON t."Id" = l."TermId"
            JOIN "Categories" c ON c."Id" = t."CategoryId"
            JOIN "SyncBaseline" b ON b."AccountId" = l."AccountId" AND b."PathKey" = l."PathKey" AND b."IsFolder" = 0
            GROUP BY l."AccountId", l."PathKey"
            HAVING (json_array_length({terms}) = 0 OR SUM(t."Id" IN (SELECT "value" FROM json_each({terms}))) > 0)
              AND (json_array_length({categories}) = 0 OR SUM(c."Id" IN (SELECT "value" FROM json_each({categories}))) > 0)
              AND ({text} = '' OR instr(l."PathKey", {text}) > 0 OR SUM(instr(upper(t."Text"), {text}) > 0 OR instr(c."NameKey", {text}) > 0) > 0)
            ORDER BY l."PathKey", l."AccountId"
            LIMIT {take} OFFSET {query.Offset}
            """).ToListAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success<FileSearchPage, ClassificationFailure>(FileSearchPageFactory.Create([.. found.Take(query.Limit).Select(ToRow)], found.Count > query.Limit));
    }

    private static FileSearchRow ToRow(FoundFile file) =>
        FileSearchRowFactory.Create(
            AccountIdFactory.Create(file.AccountId),
            RelativeSyncPathFactory.Create(file.Path).Match(path => path, _ => throw TermMapping.Corrupt()),
            file.Size,
            [.. (JsonSerializer.Deserialize<string[]>(file.Categories) ?? []).Order(StringComparer.OrdinalIgnoreCase)]);

    private sealed class FoundFile
    {
        public Guid AccountId { get; set; }

        public string Path { get; set; } = string.Empty;

        public long? Size { get; set; }

        public string Categories { get; set; } = "[]";
    }
}
