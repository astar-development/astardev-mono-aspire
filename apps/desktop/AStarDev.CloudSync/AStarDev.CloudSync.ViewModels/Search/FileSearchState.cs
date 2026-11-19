namespace AStarDev.CloudSync.ViewModels.Search;

/// <summary>Where the Files section of the search is.</summary>
public abstract record FileSearchState
{
    private FileSearchState()
    {
    }

    /// <summary>Nothing to look for: the search text is blank and no chip is selected.</summary>
    public sealed record Idle : FileSearchState;

    /// <summary>The files are being read.</summary>
    public sealed record Loading : FileSearchState;

    /// <summary>The files were read.</summary>
    public sealed record Loaded : FileSearchState;

    /// <summary>The files could not be read.</summary>
    public sealed record Failed : FileSearchState;
}
