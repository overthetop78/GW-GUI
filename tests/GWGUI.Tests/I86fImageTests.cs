using System.IO;
using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Images;
using Xunit.Abstractions;

namespace GWGUI.Tests;

/// <summary>Vérifie le décodage et le routage public des conteneurs 86F locaux.</summary>
public sealed class I86fImageTests(ITestOutputHelper output)
{
    /// <summary>Vérifie que le Reader 86F décode des secteurs depuis l'image réelle disponible.</summary>
    [Fact]
    public async Task FrameworkPremierRealImageDecodesWhenPresent()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "IBM PC", "Framework Premier 1.1 Fr - Systeme 1 [5.25].86f");
        if (!File.Exists(path)) return;
        var image = await new I86fImageReader(new FluxDecoderRegistry()).ReadAsync(path);
        Assert.NotEmpty(image.AvailableBlocks);
    }

    /// <summary>Vérifie que le registre public présélectionne le Reader 86F.</summary>
    /// <summary>Compare l'image 86F de sauvegarde à sa référence sectorielle lorsqu'elles sont disponibles.</summary>
    [Fact]
    public async Task PublicRegistryRoutesFrameworkPremierToI86fReader()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "IBM PC", "Framework Premier 1.1 Fr - Systeme 1 [5.25].86f");
        var document = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.NotEmpty(document.Image.AvailableBlocks);
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

    /// <summary>Retourne la racine du dépôt contenant la solution.</summary>
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("Repository root not found.");
    }
}
