using AStarDev.CloudSync.TestHelpers.Classification;
using AStarDev.CloudSync.TestHelpers.Ui;
using AStarDev.CloudSync.ViewModels.Classification;

namespace AStarDev.CloudSync.ViewModels.TestsUnit.Classification;

public class GivenTheTermListOpenFiles
{
    [Fact]
    public async Task when_open_files_is_chosen_then_the_term_is_requested()
    {
        var colours = ClassificationTestData.NewCategoryId();
        var list = new ClassificationTermListViewModel(new FakeTermStore(ClassificationTestData.Term("teal", colours)), new FakeCategoryStore(), new FakeFileClassificationStore(), TestData.CreateLocalization());
        await list.LoadCommand.ExecuteAsync(null);
        var requested = new List<string>();
        list.FilesRequested += (_, row) => requested.Add(row.Text);

        list.OpenFilesCommand.Execute(list.Rows.Single());

        requested.ShouldBe(["teal"]);
        list.OpenFilesText.ShouldBe("Open files");
    }
}
