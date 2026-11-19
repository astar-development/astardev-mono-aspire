namespace AStarDev.CloudSync.ViewModels.Layouts;

/// <summary>Creates <see cref="LayoutCPage"/> cases.</summary>
public static class LayoutCPageFactory
{
    /// <summary>Creates the dashboard page.</summary>
    public static LayoutCPage CreateDashboard() => new LayoutCPage.Dashboard();

    /// <summary>Creates the search page.</summary>
    public static LayoutCPage CreateSearch() => new LayoutCPage.Search();

    /// <summary>Creates the classification page.</summary>
    public static LayoutCPage CreateClassification() => new LayoutCPage.Classification();
}
