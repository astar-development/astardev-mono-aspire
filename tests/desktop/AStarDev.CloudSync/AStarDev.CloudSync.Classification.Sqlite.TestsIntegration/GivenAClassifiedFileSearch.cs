using AStarDev.CloudSync.Accounts;
using Microsoft.Data.Sqlite;
using static AStarDev.CloudSync.Classification.Sqlite.TestsIntegration.ClassificationData;

namespace AStarDev.CloudSync.Classification.Sqlite.TestsIntegration;

public sealed class GivenAClassifiedFileSearch : MigratedDatabaseTests
{
    private readonly AccountId account = NewAccount();

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await InBaselineAsync(account, "Photos/cat.jpg", "Photos/dog.jpg", "Notes/plan.txt", "c.jpg", "d.jpg", "e.jpg");
    }

    private async Task<ClassificationTerm> AddTermAsync(string text, string category)
    {
        var stored = Ok(await Database.Categories.UpsertAsync(Category(category), CancellationToken));

        return Ok(await Database.Terms.UpsertAsync(Term(text, stored.Id), CancellationToken));
    }

    private async Task LinkAsync(string path, params ClassificationTerm[] terms) =>
        _ = Ok(await Database.Links.ReplaceAutoLinksAsync(account, Path(path), [.. terms.Select(term => term.Id)], CancellationToken));

    private async Task<FileSearchPage> SearchAsync(string? text = null, TermId[]? terms = null, CategoryId[]? categories = null, int offset = 0, int limit = 50) =>
        Ok(await Database.Search.SearchAsync(FileSearchQueryFactory.Create(text, terms ?? [], categories ?? [], offset, limit), CancellationToken));

    private static List<string> Names(FileSearchPage page) => [.. page.Rows.Select(row => row.Name)];

    [Fact]
    public async Task when_a_classified_file_is_searched_then_its_name_size_and_categories_are_returned()
    {
        var cat = await AddTermAsync("cat", "Animals");
        var pet = await AddTermAsync("pet", "Home");
        await LinkAsync("Photos/cat.jpg", cat, pet);

        var row = (await SearchAsync()).Rows.Single();

        (row.Name, row.Size).ShouldBe(("cat.jpg", 10L));
        row.CategoryNames.ShouldBe(["Animals", "Home"]);
    }

    [Fact]
    public async Task when_a_file_has_two_terms_in_one_category_then_the_category_is_listed_once()
    {
        var cat = await AddTermAsync("cat", "Animals");
        var dog = await AddTermAsync("dog", "Animals");
        await LinkAsync("Photos/cat.jpg", cat, dog);

        (await SearchAsync()).Rows.Single().CategoryNames.ShouldBe(["Animals"]);
    }

    [Fact]
    public async Task when_files_have_no_links_then_nothing_is_returned()
    {
        (await SearchAsync()).Rows.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_filtered_by_term_then_only_files_with_that_term_are_returned()
    {
        var cat = await AddTermAsync("cat", "Animals");
        var plan = await AddTermAsync("plan", "Documents");
        await LinkAsync("Photos/cat.jpg", cat);
        await LinkAsync("Notes/plan.txt", plan);

        Names(await SearchAsync(terms: [plan.Id])).ShouldBe(["plan.txt"]);
    }

    [Fact]
    public async Task when_filtered_by_category_then_only_files_in_that_category_are_returned()
    {
        var cat = await AddTermAsync("cat", "Animals");
        var plan = await AddTermAsync("plan", "Documents");
        await LinkAsync("Photos/cat.jpg", cat);
        await LinkAsync("Notes/plan.txt", plan);

        Names(await SearchAsync(categories: [cat.CategoryId])).ShouldBe(["cat.jpg"]);
    }

    [Fact]
    public async Task when_filtered_by_a_term_and_a_category_then_a_file_must_match_both()
    {
        var cat = await AddTermAsync("cat", "Animals");
        var plan = await AddTermAsync("plan", "Documents");
        await LinkAsync("Photos/cat.jpg", cat);
        await LinkAsync("Notes/plan.txt", plan);

        (await SearchAsync(terms: [cat.Id], categories: [plan.CategoryId])).Rows.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_filtered_by_a_term_then_all_categories_of_the_file_are_still_listed()
    {
        var cat = await AddTermAsync("cat", "Animals");
        var pet = await AddTermAsync("pet", "Home");
        await LinkAsync("Photos/cat.jpg", cat, pet);

        (await SearchAsync(terms: [cat.Id])).Rows.Single().CategoryNames.ShouldBe(["Animals", "Home"]);
    }

    [Theory]
    [InlineData("PHOTOS")]
    [InlineData("tab")]
    [InlineData("animal")]
    public async Task when_free_text_matches_the_path_a_term_or_a_category_then_the_file_is_returned(string text)
    {
        var tabby = await AddTermAsync("tabby", "Animals");
        await LinkAsync("Photos/cat.jpg", tabby);
        var plan = await AddTermAsync("plan", "Documents");
        await LinkAsync("Notes/plan.txt", plan);

        Names(await SearchAsync(text)).ShouldBe(["cat.jpg"]);
    }

    [Fact]
    public async Task when_free_text_matches_nothing_then_nothing_is_returned()
    {
        await LinkAsync("Photos/cat.jpg", await AddTermAsync("cat", "Animals"));

        (await SearchAsync("zzz")).Rows.ShouldBeEmpty();
    }

    [Fact]
    public async Task when_there_are_more_files_than_the_limit_then_pages_follow_and_the_last_has_no_more()
    {
        var thing = await AddTermAsync("thing", "Things");
        foreach (var path in new[] { "c.jpg", "d.jpg", "e.jpg" })
            await LinkAsync(path, thing);

        var first = await SearchAsync(limit: 2);
        var second = await SearchAsync(offset: 2, limit: 2);

        Names(first).ShouldBe(["c.jpg", "d.jpg"]);
        Names(second).ShouldBe(["e.jpg"]);
        (first.HasMore, second.HasMore).ShouldBe((true, false));
    }

    [Fact]
    public async Task when_the_matches_fill_the_limit_exactly_then_there_is_no_more()
    {
        var thing = await AddTermAsync("thing", "Things");
        await LinkAsync("c.jpg", thing);
        await LinkAsync("d.jpg", thing);

        (await SearchAsync(limit: 2)).HasMore.ShouldBeFalse();
    }

    [Fact]
    public async Task when_the_database_fails_then_unavailable_is_returned()
    {
        await using (var connection = new SqliteConnection(new SqliteConnectionStringBuilder { DataSource = Database.DatabasePath, Pooling = false }.ToString()))
        {
            await connection.OpenAsync(CancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "DROP TABLE \"FileLinks\"";
            _ = await command.ExecuteNonQueryAsync(CancellationToken);
        }

        var failure = Failed(await Database.Search.SearchAsync(FileSearchQueryFactory.Create(null, [], [], 0, 10), CancellationToken));

        failure.ShouldBe(ClassificationFailureFactory.CreateUnavailable());
    }
}
