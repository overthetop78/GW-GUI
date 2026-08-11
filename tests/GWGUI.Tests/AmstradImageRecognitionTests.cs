using GWGUI.MediaEngine.Definitions;
using System.IO;
using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Images;

namespace GWGUI.Tests;

/// <summary>Vérifie la reconnaissance publique des conteneurs CPCEMU et leur interprétation Amstrad.</summary>
public sealed class AmstradImageRecognitionTests
{
    [Theory]
    [InlineData("validated_images/Amstrad/CPC/3 pouces simple face - 180 Kio/007 - A View to a Kill (1985)(Domark).dsk")]
    [InlineData("validated_images/Amstrad/CPC/3 pouces simple face - 180 Kio/sean_2024.dsk")]
    public async Task RecognizesStandardAndExtendedSignaturesWithAnUnusualExtension(string relativePath)
    {
        var sourcePath = ImagePath(relativePath);
        var temporaryPath = Path.Combine(Path.GetTempPath(), $"gwgui-cpcemu-{Guid.NewGuid():N}.unusual");
        try
        {
            File.Copy(sourcePath, temporaryPath);

            var parsed = await new CpcDskReader().ReadAsync(sourcePath);
            var explored = await DiskImageExplorer.CreateDefault().ExploreAsync(temporaryPath);

            Assert.Equal(DiskImageFormatIds.AmstradCpc, explored.Image.FormatId);
            AssertSameSectorImage(parsed, explored.Image);
        }
        finally
        {
            if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
        }
    }

    [Fact]
    public async Task RejectsDskExtensionWithoutCpcemuSignature()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-not-cpcemu-{Guid.NewGuid():N}.dsk");
        try
        {
            await File.WriteAllBytesAsync(path, new byte[184_320]);

            var explored = await DiskImageExplorer.CreateDefault().ExploreAsync(path);

            Assert.NotEqual(DiskImageFormatIds.AmstradCpc, explored.Image.FormatId);
            Assert.NotEqual(DiskImageFormatIds.AmstradPcw, explored.Image.FormatId);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public async Task AppliesPcwInterpretationWithoutChangingTheNeutralContainerImage()
    {
        var sourcePath = ImagePath("validated_images/Amstrad/PCW/3 pouces double face - 720 Kio/CF2DD.DSK");
        var parsed = await new CpcDskReader().ReadAsync(sourcePath);

        var explored = await DiskImageExplorer.CreateDefault().ExploreAsync(sourcePath);

        Assert.Equal(CpcDskFormat.FormatId, parsed.FormatId);
        Assert.Equal(DiskImageFormatIds.AmstradPcw, explored.Image.FormatId);
        AssertSameSectorImage(parsed, explored.Image);
    }

    private static void AssertSameSectorImage(
        GWGUI.MediaEngine.SectorImages.SectorImage expected,
        GWGUI.MediaEngine.SectorImages.SectorImage actual)
    {
        Assert.Equal(expected.BlockSize, actual.BlockSize);
        Assert.Equal(expected.Cylinders, actual.Cylinders);
        Assert.Equal(expected.Heads, actual.Heads);
        Assert.Equal(expected.SectorsPerTrack, actual.SectorsPerTrack);
        Assert.Equal(expected.Capacity, actual.Capacity);
        Assert.Equal(expected.BlockCount, actual.BlockCount);
        foreach (var expectedBlock in expected.AvailableBlocks)
        {
            Assert.True(actual.TryGetBlock(expectedBlock.LogicalBlock, out var actualBlock));
            Assert.Equal(expectedBlock.Address, actualBlock.Address);
            Assert.Equal(expectedBlock.IntegrityValid, actualBlock.IntegrityValid);
            Assert.Equal(expectedBlock.Data, actualBlock.Data);
        }
    }

    private static string ImagePath(string relativePath) =>
        Path.Combine(FindImageTestRoot(), relativePath.Replace('/', Path.DirectorySeparatorChar));

    private static string FindImageTestRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory is not null; directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, "image_test");
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Le dossier local image_test est introuvable.");
    }
}
