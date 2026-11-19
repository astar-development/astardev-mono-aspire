namespace AStarDev.CloudSync.Classification.TestsUnit;

public sealed class GivenAFileSearchQueryFactory
{
    [Fact]
    public void when_the_source_lists_change_later_then_the_query_is_unchanged()
    {
        var termIds = new List<TermId> { TermIdFactory.Create(Guid.NewGuid()) };

        var query = FileSearchQueryFactory.Create("cat", termIds, [], 0, 10);
        termIds.Clear();

        query.TermIds.Count.ShouldBe(1);
    }

    [Fact]
    public void when_the_text_is_null_or_padded_then_it_is_trimmed_to_empty_or_plain_text()
    {
        (FileSearchQueryFactory.Create(null, [], [], 0, 10).Text, FileSearchQueryFactory.Create(" cat ", [], [], 0, 10).Text).ShouldBe((string.Empty, "cat"));
    }

    [Fact]
    public void when_the_offset_is_negative_and_the_limit_is_zero_then_they_are_clamped()
    {
        var query = FileSearchQueryFactory.Create(null, [], [], -5, 0);

        (query.Offset, query.Limit).ShouldBe((0, 1));
    }
}
