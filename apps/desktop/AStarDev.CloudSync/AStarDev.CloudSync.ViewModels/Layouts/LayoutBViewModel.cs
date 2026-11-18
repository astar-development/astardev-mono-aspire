using System.ComponentModel;
using System.Windows.Input;
using AStarDev.CloudSync.Accounts;
using AStarDev.CloudSync.Sync;
using AStarDev.CloudSync.ViewModels.Accounts;
using AStarDev.CloudSync.ViewModels.Activity;
using AStarDev.CloudSync.ViewModels.Classification;
using AStarDev.CloudSync.ViewModels.Common;
using AStarDev.CloudSync.ViewModels.Conflicts;
using AStarDev.CloudSync.ViewModels.FolderSelection;
using AStarDev.CloudSync.ViewModels.Overview;
using AStarDev.CloudSync.ViewModels.Search;
using AStarDev.CloudSync.ViewModels.SyncPairs;
using AStarDev.FunctionalParadigm;
using AStarDev.Localization;
using CommunityToolkit.Mvvm.Input;

namespace AStarDev.CloudSync.ViewModels.Layouts;

/// <summary>Layout B, grouped by provider: a sidebar tree of providers and their accounts, and the selected account's detail.</summary>
public sealed class LayoutBViewModel : LocalizedObservableObject
{
    private readonly LocalPathFormatter localPathFormatter;
    private readonly ISyncActions syncActions;
    private readonly IConflictListFactory conflictLists;
    private readonly IFolderSelectionFactory folderSelections;
    private readonly IActivityListFactory activityLists;
    private LayoutBDetailViewModel detail;
    private LayoutBPage page = LayoutBPageFactory.CreateAccount();
    private string searchText = string.Empty;
    private AccountId shownAccountId;

    /// <summary>Creates the layout over the shared overview and rebuilds it whenever the overview's accounts or sync pairs change.</summary>
    /// <param name="overview">The shared overview every layout binds to.</param>
    /// <param name="localPathFormatter">Shortens the synced folders' local paths.</param>
    /// <param name="syncActions">Carries out the account actions the user picks.</param>
    /// <param name="conflictLists">Creates the conflict list of each account's Conflicts tab.</param>
    /// <param name="folderSelections">Creates the folder selection of each account's Folders tab.</param>
    /// <param name="activityLists">Creates the activity list of each account's Activity tab.</param>
    /// <param name="classification">The Classification page, which also hosts the file search the results panel shows.</param>
    /// <param name="localization">Supplies the text and culture.</param>
    public LayoutBViewModel(SyncOverviewViewModel overview, LocalPathFormatter localPathFormatter, ISyncActions syncActions, IConflictListFactory conflictLists, IFolderSelectionFactory folderSelections, IActivityListFactory activityLists, ClassificationPageViewModel classification, ILocalizationService localization) : base(localization)
    {
        Overview = overview;
        Classification = classification;
        Search = new SearchResultsViewModel(overview, OpenAccount, OpenFolder, classification.Files, localization);
        this.localPathFormatter = localPathFormatter;
        this.syncActions = syncActions;
        this.conflictLists = conflictLists;
        this.folderSelections = folderSelections;
        this.activityLists = activityLists;
        ConnectProviderCommand = new AsyncRelayCommand(AddAccountAndReloadAsync);
        detail = new NoAccountDetailViewModel(localization);
        overview.PropertyChanged += OnOverviewChanged;
        Search.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(SearchResultsViewModel.SearchText))
                SearchText = Search.SearchText;
        };
        classification.Tabs.Terms.FilesRequested += (_, term) => OpenTermFiles(term);
        Rebuild();
    }

    /// <summary>The shared overview.</summary>
    public SyncOverviewViewModel Overview { get; }

    /// <summary>The accounts grouped by provider, in overview order.</summary>
    public IReadOnlyList<ProviderGroupViewModel> ProviderGroups { get; private set; } = [];

    /// <summary>The search results shown while the sidebar search box has text: accounts, folders and files, with category chips. Its text follows the sidebar box and the other way round.</summary>
    public SearchResultsViewModel Search { get; }

    /// <summary>The Classification page: terms, suggestions and the reclassify button.</summary>
    public ClassificationPageViewModel Classification { get; }

    /// <summary>Whether the selected account's detail is shown.</summary>
    public bool IsAccountPageShown => page is LayoutBPage.Account;

    /// <summary>Whether the search results are shown.</summary>
    public bool IsSearchPageShown => page is LayoutBPage.Search;

    /// <summary>Whether the Classification page is shown.</summary>
    public bool IsClassificationPageShown => page is LayoutBPage.Classification;

    /// <summary>The Classification button text.</summary>
    public string ClassificationText => Localization.GetLocal("LayoutB.Classification");

    /// <summary>Shows the Classification page and reads the term list again.</summary>
    public ICommand ShowClassificationCommand => field ??= new RelayCommand(ShowClassification);

    /// <summary>The detail pane content: the selected account, or a no-accounts message.</summary>
    public LayoutBDetailViewModel Detail
    {
        get => detail;
        private set
        {
            if (SetProperty(ref detail, value))
                OnPropertyChanged(nameof(SelectedItem));
        }
    }

    /// <summary>The tree's selected node. Setting an account selects it; any other node keeps the current selection.</summary>
    public object SelectedItem
    {
        get => detail is AccountDetailViewModel accountDetail ? accountDetail.Account : detail;
        set
        {
            if (value is AccountViewModel account && !ReferenceEquals(SelectedItem, account))
                Choose(account);
            else
                OnPropertyChanged();
        }
    }

    /// <summary>How many accounts are syncing, e.g. "1 account syncing".</summary>
    public string SyncingSummaryText
    {
        get
        {
            int syncing = Overview.Accounts.Count(account => account.Status.IsSyncing);

            return Localization.GetLocal(syncing == 1 ? "LayoutB.AccountsSyncing.One" : "LayoutB.AccountsSyncing.Many", syncing.ToString("N0", Localization.CurrentCulture));
        }
    }

    /// <summary>The sidebar search text. Accounts and folders whose name, email, provider or path contain it, ignoring case and surrounding spaces, are shown; blank shows everything.</summary>
    public string SearchText
    {
        get => searchText;
        set
        {
            if (!SetProperty(ref searchText, value ?? string.Empty))
                return;

            Rebuild();
            Search.SearchText = searchText;
            ShowPageForSearchText();
        }
    }

    /// <summary>The search box placeholder.</summary>
    public string SearchWatermark => Localization.GetLocal("LayoutB.Search");

    /// <summary>The connect-a-provider action text.</summary>
    public string ConnectProviderText => Localization.GetLocal("LayoutB.ConnectProvider");

    /// <summary>Starts connecting a new account, from any provider.</summary>
    public IAsyncRelayCommand ConnectProviderCommand { get; }

    private void ShowClassification()
    {
        Show(LayoutBPageFactory.CreateClassification());
        _ = Classification.LoadCommand.ExecuteAsync(null);
    }

    private void OpenTermFiles(TermRowViewModel term)
    {
        Show(LayoutBPageFactory.CreateSearch());
        _ = Search.Files.LoadCommand.ExecuteAsync(null);
        _ = Search.Files.SelectTermAsync(term.Id, term.Text);
    }

    private void OpenAccount(AccountViewModel account) => Choose(account);

    private void OpenFolder(SyncPairViewModel pair)
    {
        var account = Overview.Accounts.FirstOrDefault(candidate => candidate.Id == pair.AccountId);

        if (account is null)
            return;

        Choose(account);

        if (detail is AccountDetailViewModel accountDetail)
            accountDetail.SelectedTabIndex = AccountDetailViewModel.FoldersTabIndex;
    }

    private void Choose(AccountViewModel account)
    {
        Select(account);
        Show(LayoutBPageFactory.CreateAccount());
    }

    private void ShowPageForSearchText()
    {
        if (searchText.Trim().Length > 0)
            Show(LayoutBPageFactory.CreateSearch());
        else if (page is LayoutBPage.Search && !Search.Files.IsActive)
            Show(LayoutBPageFactory.CreateAccount());
    }

    private void Show(LayoutBPage shown)
    {
        page = shown;
        OnPropertyChanged(nameof(IsAccountPageShown));
        OnPropertyChanged(nameof(IsSearchPageShown));
        OnPropertyChanged(nameof(IsClassificationPageShown));
    }

    private void OnOverviewChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName is nameof(SyncOverviewViewModel.Accounts) or nameof(SyncOverviewViewModel.SyncPairs))
            Rebuild();
    }

    private void Rebuild()
    {
        List<AccountViewModel> visibleAccounts = [.. Overview.Accounts.Where(IsVisible)];
        ProviderGroups = [.. visibleAccounts.GroupBy(account => account.Provider.Name, StringComparer.Ordinal).Select(group => new ProviderGroupViewModel(group.First().Provider, [.. group], Localization))];
        OnPropertyChanged(nameof(ProviderGroups));
        OnPropertyChanged(nameof(SyncingSummaryText));

        if (Overview.Accounts.Count == 0)
        {
            Detail = new NoAccountDetailViewModel(Localization);

            return;
        }

        if (visibleAccounts.Count == 0)
        {
            Detail = new NoMatchingAccountsDetailViewModel(Localization);

            return;
        }

        Select(visibleAccounts.FirstOrDefault(account => account.Id == shownAccountId) ?? visibleAccounts[0]);
    }

    private void Select(AccountViewModel account)
    {
        shownAccountId = account.Id;
        Detail = new AccountDetailViewModel(account, [.. Overview.SyncPairs.Where(pair => pair.AccountId == account.Id && (AccountMatches(account) || PairMatches(pair)))], Overview.Activity, localPathFormatter, syncActions, AccountDetailTabsFactory.Create(conflictLists.Create(Option.Some(account.Id)), folderSelections.CreatePage(), activityLists.Create(Overview, Option.Some(account.Id))), Localization);
    }

    private bool IsVisible(AccountViewModel account) => AccountMatches(account) || Overview.SyncPairs.Any(pair => pair.AccountId == account.Id && PairMatches(pair));

    private bool AccountMatches(AccountViewModel account) => account.MatchesSearch(searchText);

    private bool PairMatches(SyncPairViewModel pair) => pair.MatchesSearch(searchText);

    private async Task AddAccountAndReloadAsync(CancellationToken cancellationToken)
    {
        await syncActions.AddAccountAsync(cancellationToken);
        await Overview.LoadAsync(cancellationToken);
    }
}
