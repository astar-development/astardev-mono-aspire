using AStarDev.CloudSync.Classification;
using AStarDev.FunctionalParadigm;
using AStarDev.Localization;
using CommunityToolkit.Mvvm.Input;

namespace AStarDev.CloudSync.ViewModels.Search;

/// <summary>The Files section of the search: the classified files matching the search text and the selected category and term chips, 200 at a time. Nothing is listed while the text is blank and no chip is selected.</summary>
public sealed class FileSearchViewModel : LocalizedObservableObject
{
    /// <summary>The number of files read at a time.</summary>
    public const int PageSize = 200;

    private readonly IClassifiedFileSearch search;
    private readonly ICategoryStore categoryStore;
    private readonly IFileRevealer revealer;
    private IReadOnlyList<FileFilterChipViewModel> categoryChips = [];
    private FileFilterChipViewModel? termChip;
    private IReadOnlyList<FileRowViewModel> rows = [];
    private FileSearchState state = FileSearchStateFactory.CreateIdle();
    private string text = string.Empty;
    private bool hasMore;
    private bool revealFailed;
    private int generation;

    /// <summary>Creates the section; it lists nothing until there is text or a chip.</summary>
    /// <param name="search">Reads the matching files.</param>
    /// <param name="categoryStore">Reads the categories shown as chips.</param>
    /// <param name="revealer">Shows a file in its local folder.</param>
    /// <param name="localization">Supplies the text and culture.</param>
    public FileSearchViewModel(IClassifiedFileSearch search, ICategoryStore categoryStore, IFileRevealer revealer, ILocalizationService localization) : base(localization)
    {
        this.search = search;
        this.categoryStore = categoryStore;
        this.revealer = revealer;
        LoadCommand = new AsyncRelayCommand(LoadAsync);
        ShowMoreCommand = new AsyncRelayCommand(ShowMoreAsync);
    }

    /// <summary>Where reading the files is.</summary>
    public FileSearchState State => state;

    /// <summary>The files read so far.</summary>
    public IReadOnlyList<FileRowViewModel> Rows => rows;

    /// <summary>The selected term chip, when there is one, followed by one chip per category.</summary>
    public IReadOnlyList<FileFilterChipViewModel> Chips => termChip is null ? categoryChips : [termChip, .. categoryChips];

    /// <summary>Whether the Files section is shown: the text is not blank or a chip is selected.</summary>
    public bool IsActive => state is not FileSearchState.Idle;

    /// <summary>Whether files are listed.</summary>
    public bool HasRows => rows.Count > 0;

    /// <summary>Whether the files are being read.</summary>
    public bool IsLoading => state is FileSearchState.Loading;

    /// <summary>Whether reading failed.</summary>
    public bool IsFailed => state is FileSearchState.Failed;

    /// <summary>Whether reading worked and nothing matches.</summary>
    public bool IsEmpty => state is FileSearchState.Loaded && rows.Count == 0;

    /// <summary>Whether more matching files follow the ones listed.</summary>
    public bool HasMore => hasMore;

    /// <summary>The heading above the files.</summary>
    public string HeadingText => Localization.GetLocal("Search.Files");

    /// <summary>Shown while the files are being read.</summary>
    public string LoadingText => Localization.GetLocal("Search.Files.Loading");

    /// <summary>Shown when no file matches.</summary>
    public string EmptyText => Localization.GetLocal("Search.Files.Empty");

    /// <summary>Shown when reading failed.</summary>
    public string FailedText => Localization.GetLocal("Search.Files.Failed");

    /// <summary>The label of the button that reads the next files.</summary>
    public string ShowMoreText => Localization.GetLocal("Search.Files.ShowMore");

    /// <summary>The localised error of the last reveal, or empty.</summary>
    public string RevealErrorText => revealFailed ? Localization.GetLocal("Search.Files.RevealFailed") : string.Empty;

    /// <summary>Whether the last reveal failed.</summary>
    public bool HasRevealError => revealFailed;

    /// <summary>Reads the categories and searches again; run it each time the search page is shown.</summary>
    public IAsyncRelayCommand LoadCommand { get; }

    /// <summary>Reads the next files and adds them to the list.</summary>
    public IAsyncRelayCommand ShowMoreCommand { get; }

    /// <summary>Sets the free text and searches again.</summary>
    /// <param name="newText">The text; blank for none.</param>
    /// <returns>A task that completes once the search has run.</returns>
    public Task SetTextAsync(string newText)
    {
        text = newText.Trim();

        return SearchAsync(false);
    }

    /// <summary>Filters by one term, replacing any term filter, and searches again.</summary>
    /// <param name="termId">The term.</param>
    /// <param name="termText">The term text shown on its chip.</param>
    /// <returns>A task that completes once the search has run.</returns>
    public Task SelectTermAsync(TermId termId, string termText)
    {
        termChip = new FileFilterChipViewModel(termText, null, termId, chip => ToggleAsync(chip)) { IsSelected = true };
        OnPropertyChanged(nameof(Chips));

        return SearchAsync(false);
    }

    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        var categories = await categoryStore.GetAllAsync(cancellationToken).ConfigureAwait(true);
        categoryChips = categories.Match(Chipped, _ => categoryChips);
        OnPropertyChanged(nameof(Chips));
        await SearchAsync(false).ConfigureAwait(true);
    }

    private IReadOnlyList<FileFilterChipViewModel> Chipped(IReadOnlyList<ClassificationCategory> categories)
    {
        var selected = categoryChips.Where(chip => chip.IsSelected).Select(chip => chip.Category).ToHashSet();

        return [.. categories.OrderBy(category => category.Name, StringComparer.Create(Localization.CurrentCulture, true)).Select(category => new FileFilterChipViewModel(category.Name, category.Id, null, chip => ToggleAsync(chip)) { IsSelected = selected.Contains(category.Id) })];
    }

    private Task ToggleAsync(FileFilterChipViewModel chip)
    {
        if (chip.Term is not null)
        {
            termChip = null;
            OnPropertyChanged(nameof(Chips));
        }
        else
        {
            chip.IsSelected = !chip.IsSelected;
        }

        return SearchAsync(false);
    }

    private Task ShowMoreAsync(CancellationToken cancellationToken) => SearchAsync(true);

    private async Task SearchAsync(bool append)
    {
        var current = ++generation;
        var termIds = termChip?.Term is { } term ? new[] { term } : [];
        var categoryIds = categoryChips.Where(chip => chip.IsSelected).Select(chip => chip.Category!.Value).ToArray();

        if (text.Length == 0 && termIds.Length == 0 && categoryIds.Length == 0)
        {
            Apply(FileSearchStateFactory.CreateIdle(), [], false);

            return;
        }

        if (!append)
            Apply(FileSearchStateFactory.CreateLoading(), [], false);

        var query = FileSearchQueryFactory.Create(text, termIds, categoryIds, append ? rows.Count : 0, PageSize);
        var result = await search.SearchAsync(query, CancellationToken.None).ConfigureAwait(true);

        // shortcut: an older search is not cancelled, its result is dropped when a newer one started, cancel it if the store ever gets slow.
        if (current == generation)
            _ = result.Match(page => Show(page, append), _ => Failed());
    }

    private Unit Show(FileSearchPage page, bool append)
    {
        var read = page.Rows.Select(row => new FileRowViewModel(row, RevealAsync, Localization));
        Apply(FileSearchStateFactory.CreateLoaded(), append ? [.. rows, .. read] : [.. read], page.HasMore);

        return Unit.Instance;
    }

    private Unit Failed()
    {
        Apply(FileSearchStateFactory.CreateFailed(), rows, false);

        return Unit.Instance;
    }

    private void Apply(FileSearchState next, IReadOnlyList<FileRowViewModel> nextRows, bool more)
    {
        state = next;
        rows = nextRows;
        hasMore = more;
        revealFailed = false;
        OnPropertyChanged(nameof(State));
        OnPropertyChanged(nameof(Rows));
        OnPropertyChanged(nameof(IsActive));
        OnPropertyChanged(nameof(HasRows));
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(IsFailed));
        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(HasMore));
        OnPropertyChanged(nameof(HasRevealError));
        OnPropertyChanged(nameof(RevealErrorText));
    }

    private async Task RevealAsync(FileSearchRow row)
    {
        revealFailed = !await revealer.RevealAsync(row.AccountId, row.Path, CancellationToken.None).ConfigureAwait(true);
        OnPropertyChanged(nameof(HasRevealError));
        OnPropertyChanged(nameof(RevealErrorText));
    }
}
