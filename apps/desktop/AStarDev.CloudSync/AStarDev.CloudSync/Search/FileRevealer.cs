using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using AStarDev.CloudSync.Accounts;
using AStarDev.CloudSync.Sync;
using AStarDev.CloudSync.ViewModels.Search;
using AStarDev.FunctionalParadigm;

namespace AStarDev.CloudSync.Search;

/// <summary>Shows a synced file in the file manager: the account's sync root joined with the file's path, handed to the operating system's reveal command. An unknown account or a command that cannot start is <see langword="false"/>, never an exception.</summary>
[ExcludeFromCodeCoverage(Justification = "Starts a real process; the commands it runs are tested in RevealCommandFactory.")]
public sealed class FileRevealer(ISyncRootReader roots, IFileSystem fileSystem) : IFileRevealer
{
    /// <inheritdoc />
    public async Task<bool> RevealAsync(AccountId accountId, RelativeSyncPath path, CancellationToken cancellationToken)
    {
        var read = await roots.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var root = read.Match(all => all.FirstOrDefault(candidate => candidate.AccountId == accountId), _ => null);

        return root is not null && Start(RevealCommandFactory.Create(fileSystem.Path.Combine(root.Root.Directory.FullName, path.Value ?? string.Empty)));
    }

    private static bool Start(RevealCommand command)
    {
        var start = new ProcessStartInfo(command.Program) { UseShellExecute = false, CreateNoWindow = true };
        foreach (var argument in command.Arguments)
            start.ArgumentList.Add(argument);

        return Try.Run(() => Launch(start)).Match(started => started, _ => false);
    }

    private static bool Launch(ProcessStartInfo start)
    {
        using var process = Process.Start(start);

        return process is not null;
    }
}
