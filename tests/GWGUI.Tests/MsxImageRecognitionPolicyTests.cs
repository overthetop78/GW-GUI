using GWGUI.MediaEngine.Exploration;
using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Containers.Msx.Raw;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Images;
using GWGUI.MediaEngine.Recognition;
using GWGUI.MediaEngine.Recognition.Policies;

namespace GWGUI.Tests;

/// <summary>Vérifie la présélection MSX par extension, demande explicite et BPB.</summary>
public sealed class MsxImageRecognitionPolicyTests
{
    /// <summary>Vérifie les quatre géométries MSX-DOS prises en charge par le Reader public.</summary>
    [Theory]
    [InlineData(40, 1, 9, 0xf9, DiskImageFormatIds.Msx1D)]
    [InlineData(80, 1, 9, 0xf8, DiskImageFormatIds.Msx1Dd)]
    [InlineData(40, 2, 9, 0xf9, DiskImageFormatIds.Msx2D)]
    [InlineData(80, 2, 9, 0xf9, DiskImageFormatIds.Msx2Dd)]
    public async Task PublicReaderReadsEverySupportedGeometry(int cylinders, int heads, int sectorsPerTrack, byte mediaDescriptor, string formatId)
    {
        var path = await CreateMsxImageAsync(cylinders, heads, sectorsPerTrack, mediaDescriptor);
        try
        {
            var image = await new MsxRawImageReader().ReadAsync(path);
            Assert.Equal(formatId, image.FormatId);
            Assert.Equal(cylinders, image.Cylinders);
            Assert.Equal(heads, image.Heads);
            Assert.Equal(sectorsPerTrack, image.SectorsPerTrack);
            Assert.Equal(new(cylinders - 1, heads - 1, sectorsPerTrack), image.AvailableBlocks.Last().Address);
        }
        finally { File.Delete(path); }
    }

    /// <summary>Vérifie le rejet d'une capacité absente du catalogue malgré un BPB MSX-DOS cohérent.</summary>
    [Fact]
    public async Task PublicReaderRejectsUnsupportedCapacity()
    {
        var path = await CreateMsxImageAsync(40, 1, 8, 0xf9);
        try { await Assert.ThrowsAsync<InvalidDataException>(() => new MsxRawImageReader().ReadAsync(path)); }
        finally { File.Delete(path); }
    }

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

    /// <summary>Crée une image MSX-DOS temporaire dont le BPB décrit exactement la géométrie demandée.</summary>
    private static async Task<string> CreateMsxImageAsync(int cylinders, int heads, int sectorsPerTrack, byte mediaDescriptor)
    {
        var totalSectors = cylinders * heads * sectorsPerTrack;
        var data = new byte[totalSectors * FatBootSectorLayout.SectorSize];
        "MSX     "u8.CopyTo(data.AsSpan(FatBootSectorLayout.OemOffset, FatBootSectorLayout.OemLength));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(FatBootSectorLayout.BytesPerSectorOffset), FatBootSectorLayout.SectorSize);
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(FatBootSectorLayout.TotalSectors16Offset), checked((ushort)totalSectors));
        data[FatBootSectorLayout.MediaDescriptorOffset] = mediaDescriptor;
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(FatBootSectorLayout.SectorsPerTrackOffset), checked((ushort)sectorsPerTrack));
        BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(FatBootSectorLayout.HeadCountOffset), checked((ushort)heads));
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-msx-{Guid.NewGuid():N}.dsk");
        await File.WriteAllBytesAsync(path, data);
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
