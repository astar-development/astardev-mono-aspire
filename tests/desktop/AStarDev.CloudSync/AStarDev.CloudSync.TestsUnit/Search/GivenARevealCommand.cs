using AStarDev.CloudSync.Search;

namespace AStarDev.CloudSync.TestsUnit.Search;

public class GivenARevealCommand
{
    [Fact]
    public void when_created_for_linux_then_the_folder_of_the_file_is_opened()
    {
        var command = RevealCommandFactory.CreateForLinux(Path.Combine(Path.GetTempPath(), "Docs", "a.txt"));

        command.Program.ShouldBe("xdg-open");
        command.Arguments.ShouldBe([Path.Combine(Path.GetTempPath(), "Docs")]);
    }

    [Fact]
    public void when_created_for_windows_then_explorer_selects_the_file()
    {
        var command = RevealCommandFactory.CreateForWindows(@"C:\Users\me\OneDrive\a.txt");

        command.Program.ShouldBe("explorer.exe");
        command.Arguments.ShouldBe([@"/select,C:\Users\me\OneDrive\a.txt"]);
    }

    [Fact]
    public void when_created_for_mac_then_finder_reveals_the_file()
    {
        var command = RevealCommandFactory.CreateForMac("/Users/me/OneDrive/a.txt");

        command.Program.ShouldBe("open");
        command.Arguments.ShouldBe(["-R", "/Users/me/OneDrive/a.txt"]);
    }

    [Fact]
    public void when_created_for_the_running_system_then_a_command_with_the_path_is_returned()
    {
        var command = RevealCommandFactory.Create(Path.Combine(Path.GetTempPath(), "a.txt"));

        command.Arguments.ShouldNotBeEmpty();
    }
}
