using AStarDev.CloudSync.Accounts;
using AStarDev.CloudSync.Sync;

namespace AStarDev.CloudSync.ViewModels.Search;

/// <summary>Shows a synced file in the operating system's file manager.</summary>
public interface IFileRevealer
{
    /// <summary>Reveals the file in its local folder.</summary>
    /// <param name="accountId">The account the file belongs to.</param>
    /// <param name="path">The path below the account's sync root.</param>
    /// <param name="cancellationToken">Cancels the work.</param>
    /// <returns><see langword="true"/> when the file manager was opened; <see langword="false"/> when the account has no sync root or the file manager could not be started.</returns>
    Task<bool> RevealAsync(AccountId accountId, RelativeSyncPath path, CancellationToken cancellationToken);
}
