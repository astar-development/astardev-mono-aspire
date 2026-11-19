using AStarDev.CloudSync.Accounts;
using AStarDev.CloudSync.Sync;
using AStarDev.CloudSync.ViewModels.Search;

namespace AStarDev.CloudSync.TestHelpers.Ui;

/// <summary>Records the files it was asked to reveal and answers with a fixed outcome.</summary>
public sealed class FakeFileRevealer(bool revealed = true) : IFileRevealer
{
    public List<(AccountId AccountId, RelativeSyncPath Path)> Revealed { get; } = [];

    public Task<bool> RevealAsync(AccountId accountId, RelativeSyncPath path, CancellationToken cancellationToken)
    {
        Revealed.Add((accountId, path));

        return Task.FromResult(revealed);
    }
}
