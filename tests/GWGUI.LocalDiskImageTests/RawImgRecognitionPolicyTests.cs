using GWGUI.MediaEngine.Exploration;
using System.IO;
using GWGUI.MediaEngine.Containers.Amstrad.CpcDsk;
using GWGUI.MediaEngine.Containers.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Policies;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>VÃ©rifie le routage des images IMG brutes vers leurs interprÃ©tations IBM et Amstrad.</summary>
public sealed class RawImgRecognitionPolicyTests
{
    /// <summary>VÃ©rifie qu'une image IBM avec BPB valide conserve son identifiant IBM.</summary>
    [Fact]
    public async Task IbmImageWithValidBpbRemainsIbm()
    {
        var path = Path.Combine(FindImageTestRoot(), "IBM PC", "Bank Street Writer for IBM PC (1984) (5.25-160k) DISK01S1.IMG");
        var image = (await DiskImageExplorer.CreateDefault().ExploreAsync(path)).Image;
        Assert.StartsWith(DiskImageFormatIds.IbmPrefix, image.FormatId, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>VÃ©rifie qu'une gÃ©omÃ©trie IBM connue reste constructible sans BPB.</summary>
    [Fact]
    public async Task KnownIbmSizeWithoutBpbUsesGeometryCatalog()
    {
        var path = await CreateTemporaryImageAsync(new byte[360 * 1024], ".img");
        try { Assert.Equal(DiskImageFormatIds.Ibm360, (await DiskImageExplorer.CreateDefault().ExploreAsync(path)).Image.FormatId); }
        finally { File.Delete(path); }
    }

    /// <summary>VÃ©rifie qu'une charge utile CPC issue d'une image locale est rÃ©identifiÃ©e en Amstrad CPC.</summary>
    [Fact]
    public async Task CpcRawPayloadIsRetagged()
    {
        var source = Path.Combine(FindImageTestRoot(), "validated_images", "Amstrad", "CPC", "3 pouces simple face - 180 Kio", "007 - A View to a Kill (1985)(Domark).dsk");
        await AssertRetaggedPayloadAsync(source, DiskImageFormatIds.AmstradCpc);
    }

    /// <summary>VÃ©rifie qu'une charge utile PCW issue d'une image locale est rÃ©identifiÃ©e en Amstrad PCW.</summary>
    [Fact]
    public async Task PcwRawPayloadIsRetagged()
    {
        var source = Path.Combine(FindImageTestRoot(), "validated_images", "Amstrad", "PCW", "3 pouces double face - 720 Kio", "CF2DD.DSK");
        await AssertRetaggedPayloadAsync(source, DiskImageFormatIds.AmstradPcw);
    }

    /// <summary>VÃ©rifie que l'erreur du Reader IBM est conservÃ©e pour une taille IMG impossible.</summary>
    [Fact]
    public async Task UnsupportedImgGeometryPreservesReaderError()
    {
        var path = await CreateTemporaryImageAsync(new byte[513], ".img");
        try
        {
            var registry = new DiskImageRecognitionRegistry([new RawImgRecognitionPolicy(new RawImgReader())]);
            var exception = await Assert.ThrowsAsync<DiskImageCandidatesRejectedException>(() => registry.ReadAsync(path, null, CancellationToken.None));
            Assert.Contains("513", exception.Failures[0].Exception.Message, StringComparison.Ordinal);
            Assert.Contains("512", exception.Failures[0].Exception.Message, StringComparison.Ordinal);
        }
        finally { File.Delete(path); }
    }

    /// <summary>VÃ©rifie qu'une extension diffÃ©rente de IMG ne prÃ©sÃ©lectionne pas la politique.</summary>
    [Fact]
    public async Task NonImgExtensionIsNotSelected()
    {
        var path = await CreateTemporaryImageAsync(new byte[512], ".bin");
        try { Assert.False(await new RawImgRecognitionPolicy(new RawImgReader()).CanReadAsync(new(path, null), CancellationToken.None)); }
        finally { File.Delete(path); }
    }

    /// <summary>Extrait temporairement la charge utile d'un conteneur CPCEMU local puis vÃ©rifie son interprÃ©tation IMG.</summary>
    private static async Task AssertRetaggedPayloadAsync(string sourcePath, string expectedFormatId)
    {
        var container = await new CpcDskReader().ReadAsync(sourcePath);
        var bytes = container.AvailableBlocks.OrderBy(block => block.LogicalBlock).SelectMany(block => block.Data).ToArray();
        var path = await CreateTemporaryImageAsync(bytes, ".img");
        try { Assert.Equal(expectedFormatId, (await DiskImageExplorer.CreateDefault().ExploreAsync(path)).Image.FormatId); }
        finally { File.Delete(path); }
    }

    /// <summary>CrÃ©e un fichier IMG temporaire avec le contenu fourni.</summary>
    private static async Task<string> CreateTemporaryImageAsync(byte[] bytes, string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-raw-img-{Guid.NewGuid():N}{extension}");
        await File.WriteAllBytesAsync(path, bytes);
        return path;
    }

    /// <summary>Retourne la racine locale des images de test.</summary>
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
