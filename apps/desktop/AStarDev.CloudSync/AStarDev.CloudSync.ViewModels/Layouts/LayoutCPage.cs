namespace AStarDev.CloudSync.ViewModels.Layouts;

/// <summary>What layout C shows between its command bar and its accounts bar.</summary>
public abstract record LayoutCPage
{
    private LayoutCPage()
    {
    }

    /// <summary>The sync pair list and the live activity.</summary>
    public sealed record Dashboard : LayoutCPage
    {
        internal Dashboard()
        {
        }
    }

    /// <summary>The search results: accounts, folders and files.</summary>
    public sealed record Search : LayoutCPage
    {
        internal Search()
        {
        }
    }

    /// <summary>The term list, suggestions and reclassify button.</summary>
    public sealed record Classification : LayoutCPage
    {
        internal Classification()
        {
        }
    }
}
