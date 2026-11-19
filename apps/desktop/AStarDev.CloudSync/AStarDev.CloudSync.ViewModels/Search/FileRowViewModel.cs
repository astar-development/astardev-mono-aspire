using AStarDev.CloudSync.Classification;
using AStarDev.CloudSync.ViewModels.Common;
using AStarDev.Localization;
using CommunityToolkit.Mvvm.Input;

namespace AStarDev.CloudSync.ViewModels.Search;

/// <summary>One file of the search results: its name, size and categories. Choosing it reveals the file in its local folder.</summary>
public sealed class FileRowViewModel : LocalizedObservableObject
{
    private readonly FileSearchRow row;

    internal FileRowViewModel(FileSearchRow row, Func<FileSearchRow, Task> reveal, ILocalizationService localization) : base(localization)
    {
        this.row = row;
        RevealCommand = new AsyncRelayCommand(() => reveal(row));
    }

    /// <summary>The file name.</summary>
    public string Name => row.Name;

    /// <summary>The path below the sync root.</summary>
    public string PathText => row.Path.Value ?? string.Empty;

    /// <summary>The size formatted for the culture, or empty when the size is not known.</summary>
    public string SizeText => row.Size is { } size ? ByteSizeFormatter.Format(size, Localization) : string.Empty;

    /// <summary>The names of the file's categories, separated by commas.</summary>
    public string CategoriesText => string.Join(", ", row.CategoryNames);

    /// <summary>Reveals the file in its local folder.</summary>
    public IAsyncRelayCommand RevealCommand { get; }
}
