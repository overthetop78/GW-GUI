using System.IO;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Containers.Commodore;
using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Conversion.Commodore;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Commodore.Dos;
using GWGUI.MediaEngine.Geometries.Commodore;

namespace GWGUI.Tests;

/// <summary>Vérifie le Writer commun des conteneurs Commodore D64 et D71.</summary>
public sealed class CommodoreDosWriterTests
{
    /// <summary>Vérifie les pistes zonées et la carte facultative des huit dispositions reconnues.</summary>
    [Theory]
    [InlineData(DiskImageFormatIds.Commodore1541, ".d64", 174848)]
    [InlineData(DiskImageFormatIds.Commodore1541, ".d64", 175531)]
    [InlineData(DiskImageFormatIds.Commodore1541, ".d64", 196608)]
    [InlineData(DiskImageFormatIds.Commodore1541, ".d64", 197376)]
    [InlineData(DiskImageFormatIds.Commodore1571, ".d71", 349696)]
    [InlineData(DiskImageFormatIds.Commodore1571, ".d71", 351062)]
    [InlineData(DiskImageFormatIds.Commodore1571, ".d71", 393216)]
    [InlineData(DiskImageFormatIds.Commodore1571, ".d71", 394752)]
    public async Task PreservesEverySupportedLayout(string formatId, string extension, int length)
    {
        var source = TemporaryPath(extension);
        var output = TemporaryPath(extension);
        var expected = CreateDeterministicContainer(formatId, length);
        try
        {
            await File.WriteAllBytesAsync(source, expected);
            await MediaEngineFactory.CreateCommodoreDosConversionService().ConvertAsync(source, output, formatId);
            Assert.Equal(expected, await File.ReadAllBytesAsync(output));
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }

    /// <summary>Vérifie BAM, chaîne de répertoire et fichier après réouverture D64 et D71.</summary>
    [Theory]
    [InlineData(DiskImageFormatIds.Commodore1541, ".d64", 1, 10)]
    [InlineData(DiskImageFormatIds.Commodore1571, ".d71", 2, 30)]
    public async Task PreservesFileSystemStructures(string formatId, string extension, int sides, int expectedFreeBlocks)
    {
        var source = TemporaryPath(extension);
        var output = TemporaryPath(extension);
        var bytes = CreateFileSystemContainer(sides);
        try
        {
            await File.WriteAllBytesAsync(source, bytes);
            await MediaEngineFactory.CreateCommodoreDosConversionService().ConvertAsync(source, output, formatId);
            var image = extension == DiskImageFileExtensions.D64 ? await new D64Reader().ReadAsync(output) : await new D71Reader().ReadAsync(output);
            var volume = new CommodoreDosFileSystemReader().Read(image);
            Assert.Equal("TEST DISK", volume.Name);
            Assert.Equal(expectedFreeBlocks * Commodore1541Geometry.SectorSize, volume.FreeBytes);
            var entry = Assert.Single(volume.Entries);
            Assert.Equal("HELLO", entry.Name);
            Assert.Equal(new byte[] { 4, 3, 2, 1 }, entry.Content);
            Assert.Empty(volume.Warnings);
        }
        finally
        {
            File.Delete(source);
            File.Delete(output);
        }
    }

    /// <summary>Vérifie le routage cohérent des extensions D64 et D71.</summary>
    [Fact]
    public void RoutesOnlyMatchingContainers()
    {
        Assert.True(CommodoreDosConversionService.CanCreate(DiskImageFormatIds.Commodore1541, DiskImageFileExtensions.D64));
        Assert.True(CommodoreDosConversionService.CanCreate(DiskImageFormatIds.Commodore1571, DiskImageFileExtensions.D71));
        Assert.False(CommodoreDosConversionService.CanCreate(DiskImageFormatIds.Commodore1541, DiskImageFileExtensions.D71));
        Assert.False(CommodoreDosConversionService.CanCreate(DiskImageFormatIds.Commodore1571, DiskImageFileExtensions.D64));
    }

    /// <summary>Construit une disposition déterministe avec une carte d'erreurs valide lorsqu'elle est annoncée par la longueur.</summary>
    private static byte[] CreateDeterministicContainer(string formatId, int length)
    {
        var data = new byte[length];
        var dataLength = formatId == DiskImageFormatIds.Commodore1541 ? D64Layout.Find(length)!.ErrorMapOffset ?? length : D71Layout.Find(length)!.ErrorMapOffset ?? length;
        for (var index = 0; index < dataLength; index++) data[index] = (byte)((index * 19 + index / Commodore1541Geometry.SectorSize * 7) & 0xFF);
        for (var index = dataLength; index < data.Length; index++) data[index] = (byte)CommodoreDiskErrorCode.None;
        return data;
    }

    /// <summary>Construit un volume Commodore DOS cohérent sur une ou deux faces.</summary>
    private static byte[] CreateFileSystemContainer(int sides)
    {
        const int tracks = Commodore1541Geometry.StandardTrackCount;
        var blockCount = Commodore1541Geometry.BlocksPerSide(tracks) * sides;
        var data = new byte[blockCount * Commodore1541Geometry.SectorSize];
        var header = Sector(data, tracks, sides, Commodore1541DosLayout.HeaderTrack, 0, 0);
        header[0] = Commodore1541DosLayout.HeaderTrack;
        header[1] = Commodore1541DosLayout.DirectorySector;
        header[2] = Commodore1541DosLayout.HeaderSignature;
        WritePetscii(header, Commodore1541DosLayout.VolumeNameOffset, "TEST DISK");
        header[Commodore1541DosLayout.BamEntriesOffset] = 10;
        if (sides == Commodore1571Geometry.SideCount) Sector(data, tracks, sides, Commodore1541DosLayout.HeaderTrack, 0, 1)[Commodore1541DosLayout.BamEntriesOffset] = 20;
        var directory = Sector(data, tracks, sides, Commodore1541DosLayout.HeaderTrack, Commodore1541DosLayout.DirectorySector, 0);
        directory[2] = (byte)(CommodoreDosFileType.Closed | CommodoreDosFileType.Prg);
        directory[3] = 1;
        directory[4] = 0;
        WritePetscii(directory, 5, "HELLO");
        directory[30] = 1;
        var file = Sector(data, tracks, sides, 1, 0, 0);
        file[1] = 5;
        new byte[] { 4, 3, 2, 1 }.AsSpan().CopyTo(file[2..]);
        return data;
    }

    /// <summary>Retourne un secteur mutable selon l'ordre logique zoné du conteneur.</summary>
    private static Span<byte> Sector(byte[] data, int tracks, int sides, int track, int sector, int side)
    {
        var logical = sides == Commodore1571Geometry.SideCount ? Commodore1571Geometry.ToLogicalBlock(track, sector, tracks, side) : Commodore1541Geometry.ToSideLogicalBlock(track, sector, tracks);
        return data.AsSpan(logical * Commodore1541Geometry.SectorSize, Commodore1541Geometry.SectorSize);
    }

    /// <summary>Écrit un champ PETSCII simple complété par des espaces Commodore.</summary>
    private static void WritePetscii(Span<byte> destination, int offset, string value)
    {
        destination.Slice(offset, 16).Fill(0xA0);
        System.Text.Encoding.ASCII.GetBytes(value).CopyTo(destination[offset..]);
    }

    /// <summary>Crée un chemin temporaire portant l'extension indiquée.</summary>
    private static string TemporaryPath(string extension) => Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}{extension}");
}
