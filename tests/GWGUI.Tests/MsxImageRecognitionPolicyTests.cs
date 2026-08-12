using System.IO;
using GWGUI.MediaEngine.Containers.Msx.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Policies;

namespace GWGUI.Tests;

/// <summary>Vérifie la présélection MSX par extension, demande explicite et BPB.</summary>
public sealed class MsxImageRecognitionPolicyTests
{
    /// <summary>Vérifie qu'une image MSX-DOS locale est présélectionnée puis lue par l'API publique.</summary>
    [Fact]
    public async Task ValidMsxDskIsSelectedAndRead()
    {
        var path = Path.Combine(FindImageTestRoot(), "validated_images", "MSX", "MSX", "3.5 pouces - MSX-DOS FAT12 - 720 Kio", "seeds-of-evil-msx.dsk");
        var result = await DiskImageExplorer.CreateDefault().ExploreAsync(path);
        Assert.Equal(DiskImageFormatIds.Msx2Dd, result.Image.FormatId);
        Assert.NotEmpty(result.Image.AvailableBlocks);
    }

    /// <summary>Vérifie qu'une extension différente de DSK ne présélectionne pas la politique MSX.</summary>
    [Fact]
    public async Task NonDskExtensionIsNotSelected()
    {
        var path = await CreateImageAsync(".img");
        try
        {
            var context = new DiskImageRecognitionContext(path, DiskImageFormatIds.Msx2Dd);
            Assert.False(await new MsxImageRecognitionPolicy(new MsxRawImageReader()).CanReadAsync(context, CancellationToken.None));
        }
        finally { File.Delete(path); }
    }

    /// <summary>Vérifie qu'un faux DSK sans demande explicite ni BPB MSX est refusé avant lecture.</summary>
    [Fact]
    public async Task InvalidDskWithoutRequestedFormatIsNotSelected()
    {
        var path = await CreateImageAsync(".dsk");
        try
        {
            var context = new DiskImageRecognitionContext(path, null);
            Assert.False(await new MsxImageRecognitionPolicy(new MsxRawImageReader()).CanReadAsync(context, CancellationToken.None));
        }
        finally { File.Delete(path); }
    }

    /// <summary>Vérifie qu'une demande MSX explicite présélectionne le faux DSK mais que le Reader rejette ensuite son BPB.</summary>
    [Fact]
    public async Task ExplicitMsxRequestDoesNotBypassReaderValidation()
    {
        var path = await CreateImageAsync(".dsk");
        try
        {
            var registry = new DiskImageRecognitionRegistry([new MsxImageRecognitionPolicy(new MsxRawImageReader())]);
            var exception = await Assert.ThrowsAsync<DiskImageCandidatesRejectedException>(() => registry.ReadAsync(path, DiskImageFormatIds.Msx2Dd, CancellationToken.None));
            Assert.IsType<InvalidDataException>(Assert.Single(exception.Failures).Exception);
        }
        finally { File.Delete(path); }
    }

    /// <summary>Vérifie que l'annulation de la lecture du contexte est propagée par la présélection.</summary>
    [Fact]
    public async Task ContextReadCancellationIsPropagated()
    {
        var path = await CreateImageAsync(".dsk");
        try
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();
            var context = new DiskImageRecognitionContext(path, null);
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await new MsxImageRecognitionPolicy(new MsxRawImageReader()).CanReadAsync(context, cancellation.Token));
        }
        finally { File.Delete(path); }
    }

    /// <summary>Crée un fichier temporaire ne contenant aucun BPB MSX valide.</summary>
    private static async Task<string> CreateImageAsync(string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-msx-policy-{Guid.NewGuid():N}{extension}");
        await File.WriteAllBytesAsync(path, new byte[512]);
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
