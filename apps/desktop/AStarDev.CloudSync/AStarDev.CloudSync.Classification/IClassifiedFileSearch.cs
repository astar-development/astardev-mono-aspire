using AStarDev.FunctionalParadigm;

namespace AStarDev.CloudSync.Classification;

/// <summary>Finds classified files by text, term and category.</summary>
public interface IClassifiedFileSearch
{
    /// <summary>Reads one page of the files that match the query. Only files in the account's current sync baseline are returned.</summary>
    /// <param name="query">What to look for and which page.</param>
    /// <param name="cancellationToken">Cancels the read.</param>
    /// <returns>The page, or <see cref="ClassificationFailure.Unavailable"/> when the database fails.</returns>
    Task<Result<FileSearchPage, ClassificationFailure>> SearchAsync(FileSearchQuery query, CancellationToken cancellationToken);
}
