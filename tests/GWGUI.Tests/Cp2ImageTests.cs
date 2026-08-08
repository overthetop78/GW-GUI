using System.IO;
using GWGUI.Scp.Images;

namespace GWGUI.Tests;

public sealed class Cp2ImageTests
{
    [Fact]
    public async Task PfsWriteRealImageExposesItsDosFileSystemWhenPresent()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "IBM PC", "PFS Write C00 (1985) (5.25-360k) disk01.cp2");
        if (!File.Exists(path)) return;
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.Equal("ibm.360", document.Image.FormatId);
        Assert.True(document.FileSystemRecognized);
        Assert.NotEmpty(document.Volume.Entries);
        Assert.All(document.Volume.Entries, entry => Assert.False(string.IsNullOrWhiteSpace(entry.Name)));
        Console.WriteLine($"{document.Volume.Name} | {document.Volume.FileSystem} | {document.Volume.Entries.Count} root entries | {document.Volume.Warnings.Count} warning(s)");
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
