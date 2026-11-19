namespace AStarDev.CloudSync.ViewModels.Search;

/// <summary>Creates <see cref="FileSearchState"/> cases.</summary>
public static class FileSearchStateFactory
{
    /// <summary>Creates the <see cref="FileSearchState.Idle"/> case.</summary>
    public static FileSearchState CreateIdle() => new FileSearchState.Idle();

    /// <summary>Creates the <see cref="FileSearchState.Loading"/> case.</summary>
    public static FileSearchState CreateLoading() => new FileSearchState.Loading();

    /// <summary>Creates the <see cref="FileSearchState.Loaded"/> case.</summary>
    public static FileSearchState CreateLoaded() => new FileSearchState.Loaded();

    /// <summary>Creates the <see cref="FileSearchState.Failed"/> case.</summary>
    public static FileSearchState CreateFailed() => new FileSearchState.Failed();
}
