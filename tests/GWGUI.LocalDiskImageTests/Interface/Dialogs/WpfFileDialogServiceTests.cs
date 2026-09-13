using GWGUI.App.Services.Dialogs;

namespace GWGUI.Tests.Interface.Dialogs;

public sealed class WpfFileDialogServiceTests
{
    [Theory]
    [InlineData(@"\\?\F:\Rétro\Images", @"F:\Rétro\Images")]
    [InlineData(@"\\?\UNC\server\images", @"\\server\images")]
    [InlineData(@"F:\Rétro\Images", @"F:\Rétro\Images")]
    public void NormalizesPathsForTheWindowsFileDialog(string path, string expected)
    {
        Assert.Equal(expected, WpfFileDialogService.NormalizeDialogPath(path));
    }
}
