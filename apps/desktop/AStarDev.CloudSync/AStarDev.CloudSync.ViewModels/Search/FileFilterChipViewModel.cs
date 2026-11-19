using AStarDev.CloudSync.Classification;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AStarDev.CloudSync.ViewModels.Search;

/// <summary>A category or term chip under the search box. Choosing it filters the files; choosing it again clears the filter.</summary>
public sealed class FileFilterChipViewModel : ObservableObject
{
    private bool isSelected;

    internal FileFilterChipViewModel(string label, CategoryId? category, TermId? term, Func<FileFilterChipViewModel, Task> toggle)
    {
        Label = label;
        Category = category;
        Term = term;
        ToggleCommand = new AsyncRelayCommand(() => toggle(this));
    }

    /// <summary>The category or term text.</summary>
    public string Label { get; }

    /// <summary>Whether the chip filters the files.</summary>
    public bool IsSelected
    {
        get => isSelected;
        internal set => SetProperty(ref isSelected, value);
    }

    /// <summary>Selects the chip, or clears it when it is selected.</summary>
    public IAsyncRelayCommand ToggleCommand { get; }

    internal CategoryId? Category { get; }

    internal TermId? Term { get; }
}
