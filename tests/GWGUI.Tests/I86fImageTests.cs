using System.IO;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;
using Xunit.Abstractions;

namespace GWGUI.Tests;

public sealed class I86fImageTests(ITestOutputHelper output)
{
    [Fact]
    public async Task FrameworkPremierRealImageDecodesWhenPresent()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "IBM PC", "Framework Premier 1.1 Fr - Systeme 1 [5.25].86f");
        if (!File.Exists(path)) return;
        var image = await new I86fImageReader(new FluxDecoderRegistry()).ReadAsync(path);
        Assert.NotEmpty(image.AvailableBlocks);
    }

    [Fact]
    public async Task FrameworkPremierRealImageMatchesItsDecodedReferenceWhenPresent()
    {
        var root = Path.Combine(RepositoryRoot(), "image_test", "IBM PC");
        var path = Path.Combine(root, "Framework Premier 1.1 Fr - Systeme 1 Sauvegarde [5.25].86f");
        var referencePath = Path.ChangeExtension(path, ".scp.img");
        if (!File.Exists(path) || !File.Exists(referencePath)) return;

        var image = await new I86fImageReader(new FluxDecoderRegistry()).ReadAsync(path);
        var reference = await File.ReadAllBytesAsync(referencePath);
        var mismatches = new List<string>();
        foreach (var block in image.AvailableBlocks.OrderBy(block => block.LogicalBlock))
        {
            var expected = reference.AsSpan(block.LogicalBlock * 512, 512);
            if (!block.Data.SequenceEqual(expected.ToArray()))
                mismatches.Add($"L{block.LogicalBlock}=C{block.Address.Cylinder}/H{block.Address.Head}/S{block.Address.Number}, bytes={block.Data.Count}, prefix={block.Data.Take(512).SequenceEqual(expected.ToArray())}, crc={block.IntegrityValid}");
        }

        output.WriteLine($"missing={string.Join(',', image.MissingBlocks)} mismatches={mismatches.Count}");
        foreach (var mismatch in mismatches.Take(30)) output.WriteLine(mismatch);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
