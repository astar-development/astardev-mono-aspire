using AStarDev.CloudSync.Accounts;
using AStarDev.CloudSync.Classification;
using AStarDev.CloudSync.Sync;
using AStarDev.CloudSync.TestHelpers.Classification;
using AStarDev.CloudSync.TestHelpers.Ui;
using AStarDev.CloudSync.ViewModels.Search;
using AStarDev.FunctionalParadigm;

namespace AStarDev.CloudSync.ViewModels.TestsUnit.Search;

public class GivenFileSearch
{
    private static readonly AccountId Account = AccountIdFactory.Create(Guid.NewGuid());
    private static readonly ClassificationCategory Colours = ClassificationTestData.Category(ClassificationTestData.NewCategoryId(), "Colour");
    private static readonly ClassificationCategory Cars = ClassificationTestData.Category(ClassificationTestData.NewCategoryId(), "Car");

    private static FileSearchRow Row(string path, long? size, params string[] categories) =>
        FileSearchRowFactory.Create(Account, RelativeSyncPathFactory.Create(path).Match(value => value, _ => throw new InvalidOperationException()), size, categories);

    private static FileSearchPage Page(bool hasMore, params FileSearchRow[] rows) => FileSearchPageFactory.Create(rows, hasMore);

    [Fact]
    public async Task not_list_files_for_blank_search()
    {
        var search = new FakeClassifiedFileSearch();
        var files = TestData.CreateFileSearch(search, new FakeFileRevealer());

        await files.SetTextAsync("   ");

        (files.IsActive, files.HasRows, search.Queries.Count).ShouldBe((false, false, 0));
    }

    [Fact]
    public async Task list_files_for_text()
    {
        var search = new FakeClassifiedFileSearch().Returning(Page(false, Row("Pics/teal-car.jpg", 2048, "Car", "Colour")));
        var files = TestData.CreateFileSearch(search, new FakeFileRevealer());

        await files.SetTextAsync(" teal ");

        var row = files.Rows.Single();
        (search.Queries.Single().Text, row.Name, row.CategoriesText, row.SizeText, files.IsActive, files.HasMore).ShouldBe(("teal", "teal-car.jpg", "Car, Colour", "2.0 KB", true, false));
    }

    [Fact]
    public async Task list_files_for_term()
    {
        var term = TermIdFactory.Create(Guid.NewGuid());
        var search = new FakeClassifiedFileSearch().Returning(Page(false, Row("a.txt", null, "Colour")));
        var files = TestData.CreateFileSearch(search, new FakeFileRevealer());

        await files.SelectTermAsync(term, "teal");

        search.Queries.Single().TermIds.ShouldBe([term]);
        files.Chips.Select(chip => (chip.Label, chip.IsSelected)).ShouldBe([("teal", true)]);
        files.Rows.Single().SizeText.ShouldBeEmpty();
    }

    [Fact]
    public async Task toggle_chip_filter()
    {
        var search = new FakeClassifiedFileSearch().Returning(Page(false, Row("a.txt", 1, "Colour")));
        var files = TestData.CreateFileSearch(search, new FakeFileRevealer(), Colours, Cars);
        await files.LoadCommand.ExecuteAsync(null);
        var colour = files.Chips.Single(chip => chip.Label == "Colour");

        await colour.ToggleCommand.ExecuteAsync(null);
        var whileSelected = (colour.IsSelected, files.IsActive);
        var queried = search.Queries[^1].CategoryIds;
        await colour.ToggleCommand.ExecuteAsync(null);

        queried.ShouldBe([Colours.Id]);
        (whileSelected, colour.IsSelected, files.IsActive).ShouldBe(((true, true), false, false));
    }

    [Fact]
    public async Task clear_the_term_chip_when_it_is_chosen_again()
    {
        var files = TestData.CreateFileSearch();
        await files.SelectTermAsync(TermIdFactory.Create(Guid.NewGuid()), "teal");

        await files.Chips.Single().ToggleCommand.ExecuteAsync(null);

        (files.Chips.Count, files.IsActive).ShouldBe((0, false));
    }

    [Fact]
    public async Task show_the_categories_in_name_order_as_chips()
    {
        var files = TestData.CreateFileSearch(new FakeClassifiedFileSearch(), new FakeFileRevealer(), Colours, Cars);

        await files.LoadCommand.ExecuteAsync(null);

        files.Chips.Select(chip => chip.Label).ShouldBe(["Car", "Colour"]);
    }

    [Fact]
    public async Task show_the_empty_state_when_nothing_matches()
    {
        var files = TestData.CreateFileSearch();

        await files.SetTextAsync("zzz");

        (files.IsEmpty, files.HasRows, files.EmptyText).ShouldBe((true, false, "No files match"));
    }

    [Fact]
    public async Task show_the_failed_state_when_the_store_is_unavailable()
    {
        var files = TestData.CreateFileSearch(new FakeClassifiedFileSearch().Failing(ClassificationFailureFactory.CreateUnavailable()), new FakeFileRevealer());

        await files.SetTextAsync("zzz");

        (files.IsFailed, files.HasRows, files.FailedText).ShouldBe((true, false, "The files could not be searched"));
    }

    [Fact]
    public async Task load_next_page()
    {
        var search = new FakeClassifiedFileSearch().Returning(Page(true, Row("a.txt", 1), Row("b.txt", 1))).Returning(Page(false, Row("c.txt", 1)));
        var files = TestData.CreateFileSearch(search, new FakeFileRevealer());
        await files.SetTextAsync("txt");

        await files.ShowMoreCommand.ExecuteAsync(null);

        files.Rows.Select(row => row.Name).ShouldBe(["a.txt", "b.txt", "c.txt"]);
        search.Queries.Select(query => (query.Offset, query.Limit)).ShouldBe([(0, 200), (2, 200)]);
        files.HasMore.ShouldBeFalse();
    }

    [Fact]
    public async Task keep_the_listed_files_when_the_next_page_fails()
    {
        var search = new FakeClassifiedFileSearch().Returning(Page(true, Row("a.txt", 1))).Failing(ClassificationFailureFactory.CreateUnavailable());
        var files = TestData.CreateFileSearch(search, new FakeFileRevealer());
        await files.SetTextAsync("txt");

        await files.ShowMoreCommand.ExecuteAsync(null);

        (files.IsFailed, files.Rows.Count).ShouldBe((true, 1));
    }

    [Fact]
    public async Task reveal_the_file_when_a_row_is_chosen()
    {
        var revealer = new FakeFileRevealer();
        var files = TestData.CreateFileSearch(new FakeClassifiedFileSearch().Returning(Page(false, Row("Docs/a.txt", 1))), revealer);
        await files.SetTextAsync("a");

        await files.Rows.Single().RevealCommand.ExecuteAsync(null);

        (revealer.Revealed.Select(file => (file.AccountId, file.Path.Value)).Single(), files.HasRevealError).ShouldBe(((Account, "Docs/a.txt"), false));
    }

    [Fact]
    public async Task show_an_error_when_the_file_cannot_be_revealed()
    {
        var files = TestData.CreateFileSearch(new FakeClassifiedFileSearch().Returning(Page(false, Row("a.txt", 1))), new FakeFileRevealer(false));
        await files.SetTextAsync("a");

        await files.Rows.Single().RevealCommand.ExecuteAsync(null);

        (files.HasRevealError, files.RevealErrorText).ShouldBe((true, "The file could not be shown in its folder"));
    }

    [Fact]
    public async Task not_report_nothing_matches_while_files_match()
    {
        var overview = await TestData.CreateLoadedOverviewViewModelAsync();
        var files = TestData.CreateFileSearch(new FakeClassifiedFileSearch().Returning(Page(false, Row("zzz.txt", 1))), new FakeFileRevealer());
        var results = new SearchResultsViewModel(overview, _ => { }, _ => { }, files, TestData.CreateLocalization());

        results.SearchText = "zzz-no-such-account";

        (results.Accounts.Count, results.Files.Rows.Count, results.IsEmpty).ShouldBe((0, 1, false));
    }

    [Fact]
    public async Task keep_listing_accounts_when_the_files_fail()
    {
        var overview = await TestData.CreateLoadedOverviewViewModelAsync();
        var files = TestData.CreateFileSearch(new FakeClassifiedFileSearch().Failing(ClassificationFailureFactory.CreateUnavailable()), new FakeFileRevealer());
        var results = new SearchResultsViewModel(overview, _ => { }, _ => { }, files, TestData.CreateLocalization());

        results.SearchText = "archive@";

        results.Accounts.Select(account => account.Name).ShouldBe(["Archive"]);
        (results.Files.IsFailed, results.IsEmpty).ShouldBe((true, false));
    }
}
