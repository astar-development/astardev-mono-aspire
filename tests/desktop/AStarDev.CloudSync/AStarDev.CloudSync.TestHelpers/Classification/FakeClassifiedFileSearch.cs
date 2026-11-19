using AStarDev.CloudSync.Classification;
using AStarDev.FunctionalParadigm;

namespace AStarDev.CloudSync.TestHelpers.Classification;

/// <summary>Answers every search with the next queued result and records the queries it was asked.</summary>
public sealed class FakeClassifiedFileSearch : IClassifiedFileSearch
{
    private readonly Queue<Result<FileSearchPage, ClassificationFailure>> results = new();

    public List<FileSearchQuery> Queries { get; } = [];

    public FakeClassifiedFileSearch Returning(FileSearchPage page)
    {
        results.Enqueue(Result.Success<FileSearchPage, ClassificationFailure>(page));

        return this;
    }

    public FakeClassifiedFileSearch Failing(ClassificationFailure failure)
    {
        results.Enqueue(Result.Failure<FileSearchPage, ClassificationFailure>(failure));

        return this;
    }

    public Task<Result<FileSearchPage, ClassificationFailure>> SearchAsync(FileSearchQuery query, CancellationToken cancellationToken)
    {
        Queries.Add(query);

        return Task.FromResult(results.Count > 0 ? results.Dequeue() : Result.Success<FileSearchPage, ClassificationFailure>(FileSearchPageFactory.Create([], false)));
    }
}
