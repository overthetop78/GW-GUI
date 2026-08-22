using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Containers.TeleDisk;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using GWGUI.MediaEngine.FileSystems.Fat12;
using GWGUI.MediaEngine.Recognition.TeleDisk;
using GWGUI.MediaEngine.SectorImages;
using System.IO;
using System.Buffers.Binary;

namespace GWGUI.Tests;

/// <summary>Vérifie la lecture déterministe des conteneurs TeleDisk non compressés.</summary>
public sealed class Td0ReaderTests
{
    private const int CommentHeaderOffset = 12;
    private const int TrackHeaderOffset = 36;
    private const int FirstSectorHeaderOffset = 40;
    private const int FirstSectorEncodingOffset = 48;
    private const int FirstSectorPayloadOffset = 49;

    [Theory]
    [InlineData(40, 1, 8, DiskImageFormatIds.Ibm160)]
    [InlineData(40, 1, 9, DiskImageFormatIds.Ibm180)]
    [InlineData(40, 2, 8, DiskImageFormatIds.Ibm320)]
    [InlineData(40, 2, 9, DiskImageFormatIds.Ibm360)]
    [InlineData(80, 2, 9, DiskImageFormatIds.Ibm720)]
    [InlineData(80, 2, 15, DiskImageFormatIds.Ibm1200)]
    [InlineData(80, 2, 18, DiskImageFormatIds.Ibm1440)]
    public void ClassifierUsesTheIbmGeometryCatalog(int cylinders, int heads, int sectorsPerTrack, string formatId)
    {
        var boot = new byte[FatBootSectorLayout.SectorSize];
        boot[0] = FatBootSectorLayout.ShortJumpOpcode;
        var blocks = new[] { new SectorBlock(0, new SectorAddress(0, 0, FatBootSectorLayout.BootSectorNumber), boot) };

        Assert.Equal(formatId, Td0SectorImageClassifier.Detect(blocks, FatBootSectorLayout.SectorSize, cylinders, heads, sectorsPerTrack));
    }

    [Fact]
    public void ClassifierUsesFatBpbNearJumpAndUcsdFallback()
    {
        var boot = new byte[FatBootSectorLayout.SectorSize];
        boot[0] = FatBootSectorLayout.NearJumpOpcode;
        var blocks = new[] { new SectorBlock(0, new SectorAddress(0, 0, FatBootSectorLayout.BootSectorNumber), boot) };

        Assert.Equal(DiskImageFormatIds.Ibm720, Td0SectorImageClassifier.Detect(blocks, 512, 80, 2, 9));
        boot[0] = 0;
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.BytesPerSectorOffset), FatBootSectorLayout.SectorSize);
        boot[FatBootSectorLayout.SectorsPerClusterOffset] = 1;
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.TotalSectors16Offset), 80 * 2 * 9);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.SectorsPerTrackOffset), 9);
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.HeadCountOffset), 2);
        Assert.Equal(DiskImageFormatIds.Ibm720, Td0SectorImageClassifier.Detect(blocks, 512, 80, 2, 9));
        BinaryPrimitives.WriteUInt16LittleEndian(boot.AsSpan(FatBootSectorLayout.BytesPerSectorOffset), 0);
        Assert.Equal(DiskImageFormatIds.UcsdIbmMfm, Td0SectorImageClassifier.Detect(blocks, 512, 80, 2, 9));
    }

    /// <summary>Vérifie le commentaire, les trois encodages, l'ordre, l'intégrité et les CRC de l'image connue.</summary>
    [Fact]
    public async Task ReadsKnownRawRepeatedAndRleSectors()
    {
        var image = await new Td0Reader().ReadAsync(KnownImagePath());

        Assert.Equal(DiskImageFormatIds.UcsdIbmMfm, image.FormatId);
        Assert.Equal(1, image.Cylinders);
        Assert.Equal(1, image.Heads);
        Assert.Equal(3, image.SectorsPerTrack);
        Assert.Equal(Enumerable.Range(0, 128).Select(index => (byte)index), image.GetBlock(0).ToArray());
        Assert.Equal(Enumerable.Range(0, 64).SelectMany(_ => new byte[] { 0xAA, 0x55 }), image.GetBlock(1).ToArray());
        Assert.Equal(Enumerable.Range(0, 64).SelectMany(_ => new byte[] { 0x12, 0x34 }), image.GetBlock(2).ToArray());
        Assert.True(image.AvailableBlocks.Single(block => block.LogicalBlock == 0).IntegrityValid);
        Assert.True(image.AvailableBlocks.Single(block => block.LogicalBlock == 1).IntegrityValid);
        Assert.False(image.AvailableBlocks.Single(block => block.LogicalBlock == 2).IntegrityValid);
    }

    [Fact]
    public async Task WriterPreservesCommentMapsSizesAndSectorStates()
    {
        var source = await new Td0Reader().ReadDetailedAsync(KnownImagePath());
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.td0");
        try
        {
            await new Td0Writer().WriteAsync(source, path);
            var bytes = await File.ReadAllBytesAsync(path);
            var actual = await new Td0Reader().ReadDetailedAsync(path);
            Assert.Equal("TD"u8.ToArray(), bytes[..2]);
            Assert.Equal(source.Header, actual.Header);
            Assert.Equal(source.Comment?.Data, actual.Comment?.Data);
            Assert.Equal(source.Tracks.Select(track => (track.Cylinder, track.Head, track.Sectors.Count)), actual.Tracks.Select(track => (track.Cylinder, track.Head, track.Sectors.Count)));
            Assert.Equal(source.Tracks.SelectMany(track => track.Sectors).Select(sector => (sector.Cylinder, sector.Head, sector.Number, sector.SizeCode, sector.Flags)), actual.Tracks.SelectMany(track => track.Sectors).Select(sector => (sector.Cylinder, sector.Head, sector.Number, sector.SizeCode, sector.Flags)));
            Assert.Equal(source.Tracks.SelectMany(track => track.Sectors).Select(sector => sector.Data), actual.Tracks.SelectMany(track => track.Sectors).Select(sector => sector.Data));
            Assert.Equal(source.SectorImage.AvailableBlocks.Select(block => block.Data), actual.SectorImage.AvailableBlocks.Select(block => block.Data));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Vérifie le diagnostic propre à la signature de compression avancée.</summary>
    [Fact]
    public async Task RejectsAdvancedCompressionSignatureExplicitly()
    {
        var exception = await AssertRejectedAsync(bytes => { bytes[0] = (byte)'t'; bytes[1] = (byte)'d'; });
        Assert.Contains("Advanced TeleDisk compression", exception.Message, StringComparison.Ordinal);
    }

    /// <summary>Vérifie le rejet des sections tronquées, encodages invalides et CRC altérés.</summary>
    [Fact]
    public async Task RejectsTruncatedInvalidAndCorruptedStructures()
    {
        await AssertRejectedAsync(bytes => bytes[..(CommentHeaderOffset + 5)]);
        await AssertRejectedAsync(bytes => bytes[..(TrackHeaderOffset + 2)]);
        await AssertRejectedAsync(bytes => bytes[..(FirstSectorHeaderOffset + 3)]);
        await AssertRejectedAsync(bytes => bytes[..(FirstSectorPayloadOffset + 127)]);
        await AssertRejectedAsync(bytes => { bytes[FirstSectorEncodingOffset] = 9; return bytes; });
        await AssertRejectedAsync(bytes => { bytes[10] ^= 1; return bytes; });
        await AssertRejectedAsync(bytes => { bytes[TrackHeaderOffset + 3] ^= 1; return bytes; });
        await AssertRejectedAsync(bytes => { bytes[FirstSectorPayloadOffset] ^= 1; return bytes; });
    }

    /// <summary>Crée une variante temporaire et retourne l'erreur produite par le lecteur public.</summary>
    private static Task<InvalidDataException> AssertRejectedAsync(Action<byte[]> mutate) => AssertRejectedAsync(bytes => { mutate(bytes); return bytes; });

    /// <summary>Crée une variante temporaire et retourne l'erreur produite par le lecteur public.</summary>
    private static async Task<InvalidDataException> AssertRejectedAsync(Func<byte[], byte[]> mutate)
    {
        var bytes = mutate(await File.ReadAllBytesAsync(KnownImagePath()));
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.td0");
        try
        {
            await File.WriteAllBytesAsync(path, bytes);
            return await Assert.ThrowsAsync<InvalidDataException>(() => new Td0Reader().ReadAsync(path));
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>Retourne le chemin de l'image TeleDisk locale obligatoire.</summary>
    private static string KnownImagePath()
    {
        var path = Path.Combine(RepositoryRoot(), "image_test", "_generated", "TeleDisk", "raw-repeated-rle.td0");
        return File.Exists(path) ? path : throw new FileNotFoundException("L'image TeleDisk de test est introuvable.", path);
    }

    /// <summary>Localise la racine du dépôt courant.</summary>
    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "GWGUI.sln"))) directory = directory.Parent;
        return directory?.FullName ?? throw new DirectoryNotFoundException("La racine du dépôt est introuvable.");
    }
}
