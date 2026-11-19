namespace AStarDev.CloudSync.Search;

/// <summary>Creates the <see cref="RevealCommand"/> of each operating system.</summary>
public static class RevealCommandFactory
{
    /// <summary>Creates the command of the running operating system.</summary>
    /// <param name="fullPath">The full path of the file.</param>
    public static RevealCommand Create(string fullPath) =>
        OperatingSystem.IsWindows() ? CreateForWindows(fullPath) : OperatingSystem.IsMacOS() ? CreateForMac(fullPath) : CreateForLinux(fullPath);

    /// <summary>Creates the command that opens the file's folder. The Linux file managers share no way to select a file, so the folder opens.</summary>
    /// <param name="fullPath">The full path of the file.</param>
    public static RevealCommand CreateForLinux(string fullPath) => new("xdg-open", [Path.GetDirectoryName(fullPath) ?? fullPath]);

    /// <summary>Creates the command that opens Explorer with the file selected.</summary>
    /// <param name="fullPath">The full path of the file.</param>
    public static RevealCommand CreateForWindows(string fullPath) => new("explorer.exe", [$"/select,{fullPath}"]);

    /// <summary>Creates the command that opens Finder with the file selected.</summary>
    /// <param name="fullPath">The full path of the file.</param>
    public static RevealCommand CreateForMac(string fullPath) => new("open", ["-R", fullPath]);
}
