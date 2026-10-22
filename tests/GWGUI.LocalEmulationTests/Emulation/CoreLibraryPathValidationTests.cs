using System.IO;
using GWGUI.Emulation;

namespace GWGUI.Tests.Emulation;

public sealed class CoreLibraryPathValidationTests
{
    [Theory]
    [InlineData("core.dll")]
    [InlineData("")]
    [InlineData("C:core.dll")]
    [InlineData("\\core.dll")]
    public void RejectsRelativePathsBeforeConsultingTheMissingFileFactory(string path)
    {
        var expected = new ArgumentException("absolute path required", "absolutePath");
        var reads = 0;
        var missingRead = false;
        var observed = Assert.Throws<ArgumentException>(() => EmulationSdk.NativeResources.ValidateLibraryPath(path,
            () => { reads++; return expected; },
            _ => { missingRead = true; throw new InvalidOperationException("wrong factory"); }));
        Assert.Same(expected, observed);
        Assert.Equal(1, reads);
        Assert.False(missingRead);
    }

    [Fact]
    public void RejectsADirectoryAsAFileAndPreservesTheRequestedPath()
    {
        var root = Root();
        var expected = new FileNotFoundException("missing core", root);
        var reads = 0;
        var observed = Assert.Throws<FileNotFoundException>(() => EmulationSdk.NativeResources.ValidateLibraryPath(root,
            () => throw new InvalidOperationException("wrong factory"),
            path =>
            {
                Assert.Equal(root, path);
                reads++;
                return expected;
            }));
        Assert.Same(expected, observed);
        Assert.Equal(root, observed.FileName);
        Assert.Equal(1, reads);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void PropagatesOnlyTheSelectedFactoryFailure(bool relative)
    {
        var expected = new InvalidOperationException("factory failed");
        var visited = new List<string>();
        var observed = Assert.Throws<InvalidOperationException>(() => EmulationSdk.NativeResources.ValidateLibraryPath(
            relative ? "relative.dll" : Root(),
            () => { visited.Add("absolute"); throw expected; },
            _ => { visited.Add("missing"); throw expected; }));
        Assert.Same(expected, observed);
        Assert.Equal(relative ? "absolute" : "missing", Assert.Single(visited));
    }

    private static string Root() => Path.GetPathRoot(Environment.CurrentDirectory)!;
}
