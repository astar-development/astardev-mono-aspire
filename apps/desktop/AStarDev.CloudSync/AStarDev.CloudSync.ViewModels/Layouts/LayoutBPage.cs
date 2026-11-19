namespace AStarDev.CloudSync.ViewModels.Layouts;

/// <summary>What layout B shows beside its sidebar.</summary>
public abstract record LayoutBPage
{
    private LayoutBPage()
    {
    }

    /// <summary>The selected account's detail.</summary>
    public sealed record Account : LayoutBPage
    {
        internal Account()
        {
        }
    }

    /// <summary>The search results: accounts, folders and files.</summary>
    public sealed record Search : LayoutBPage
    {
        internal Search()
        {
        }
    }

    /// <summary>The term list, suggestions and reclassify button.</summary>
    public sealed record Classification : LayoutBPage
    {
        internal Classification()
        {
        }
    }
}
