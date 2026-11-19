namespace AStarDev.CloudSync.ViewModels.Layouts;

/// <summary>Creates <see cref="LayoutBPage"/> cases.</summary>
public static class LayoutBPageFactory
{
    /// <summary>Creates the account detail page.</summary>
    public static LayoutBPage CreateAccount() => new LayoutBPage.Account();

    /// <summary>Creates the search page.</summary>
    public static LayoutBPage CreateSearch() => new LayoutBPage.Search();

    /// <summary>Creates the classification page.</summary>
    public static LayoutBPage CreateClassification() => new LayoutBPage.Classification();
}
