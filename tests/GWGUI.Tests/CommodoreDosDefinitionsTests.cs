using System.IO;
using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Containers.Commodore.D81;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Commodore;
using GWGUI.MediaEngine.FileSystems.Readers;
using GWGUI.MediaEngine.Geometries.Commodore;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

public sealed class CommodoreDosDefinitionsTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.Commodore1541)]
    [InlineData(DiskImageFormatIds.Commodore1571)]
    [InlineData(DiskImageFormatIds.Commodore1581)]
    public async Task PublicReadersExposeACommodoreDosVolume(string formatId)
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}{Extension(formatId)}");
        try
        {
            await File.WriteAllBytesAsync(path, CreateImage(formatId));
            var image = formatId switch
            {
                DiskImageFormatIds.Commodore1541 => await new D64Reader().ReadAsync(path),
                DiskImageFormatIds.Commodore1571 => await new D71Reader().ReadAsync(path),
                _ => await new D81Reader().ReadAsync(path)
            };
            var reader = new CommodoreDosFileSystemReader();
            Assert.True(reader.CanRead(image));
            Assert.Equal("TEST", reader.Read(image).Name);
        }
        finally { File.Delete(path); }
    }

    [Theory]
    [InlineData(CommodoreDosFileType.Del, "DEL")]
    [InlineData(CommodoreDosFileType.Seq, "SEQ")]
    [InlineData(CommodoreDosFileType.Prg, "PRG")]
    [InlineData(CommodoreDosFileType.Usr, "USR")]
    [InlineData(CommodoreDosFileType.Rel, "REL")]
    [InlineData(CommodoreDosFileType.Cbm, "CBM")]
    public void FileTypeNamesCoverEveryBaseType(CommodoreDosFileType fileType, string expected) => Assert.Equal(expected, CommodoreDosFileTypeNames.GetBaseTypeName(fileType));

    [Fact]
    public void FileTypeCommentPreservesClosedAndLockedFlags()
    {
        Assert.Equal("PRG", CommodoreDosFileTypeNames.GetComment(CommodoreDosFileType.Prg | CommodoreDosFileType.Closed));
        Assert.Equal("PRG, open, locked", CommodoreDosFileTypeNames.GetComment(CommodoreDosFileType.Prg | CommodoreDosFileType.Locked));
    }

    [Fact]
    public void PetsciiDecoderCoversLettersPaddingAndUnknownBytes()
    {
        Assert.Equal("AZ", PetsciiDecoder.Decode([0x41, 0x7a, 0xa0, 0x42]));
        Assert.Equal("�", PetsciiDecoder.Decode([0x1f]));
    }

    [Fact]
    public async Task CyclicAndMissingDataChainsProduceWarnings()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.d64");
        try
        {
            var bytes = CreateImage(DiskImageFormatIds.Commodore1541);
            var directory = Commodore1541Geometry.ToSideLogicalBlock(18, 1, 35) * CommodoreDosLayout.SectorSize;
            bytes[directory] = 0;
            bytes[directory + 1] = 0;
            bytes[directory + 2] = (byte)(CommodoreDosFileType.Prg | CommodoreDosFileType.Closed);
            bytes[directory + 3] = 1;
            bytes[directory + 4] = 0;
            bytes[directory + 5] = (byte)'F';
            bytes[directory + 6] = (byte)'I';
            bytes[directory + 7] = (byte)'L';
            bytes[directory + 8] = (byte)'E';
            var dataLogical = Commodore1541Geometry.ToSideLogicalBlock(1, 0, 35);
            var dataOffset = dataLogical * CommodoreDosLayout.SectorSize;
            bytes[dataOffset] = 1;
            bytes[dataOffset + 1] = 0;
            await File.WriteAllBytesAsync(path, bytes);
            var image = await new D64Reader().ReadAsync(path);
            var cyclic = new CommodoreDosFileSystemReader().Read(image);
            Assert.Contains(cyclic.Warnings, warning => warning.Contains("Cyclic data chain", StringComparison.Ordinal));
            var missing = new SectorImage(image.FormatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, image.AvailableBlocks.Where(block => block.LogicalBlock != dataLogical), logicalBlockCount: image.BlockCount);
            var absent = new CommodoreDosFileSystemReader().Read(missing);
            Assert.Contains(absent.Warnings, warning => warning.Contains("is missing", StringComparison.Ordinal));
        }
        finally { File.Delete(path); }
    }

    private static byte[] CreateImage(string formatId)
    {
        var length = formatId switch
        {
            DiskImageFormatIds.Commodore1541 => D64Layout.Tracks35.ImageLength,
            DiskImageFormatIds.Commodore1571 => D71Layout.Tracks35.ImageLength,
            _ => D81Layout.ImageLength
        };
        var bytes = new byte[length];
        var layout = CommodoreDosLayout.Resolve(formatId)!;
        var headerLogical = formatId == DiskImageFormatIds.Commodore1581 ? Commodore1581Geometry.ToLogicalBlock(layout.HeaderTrack, layout.HeaderSector) : Commodore1541Geometry.ToSideLogicalBlock(layout.HeaderTrack, layout.HeaderSector, 35);
        var header = headerLogical * CommodoreDosLayout.SectorSize;
        bytes[header + CommodoreDosLayout.DirectoryEntriesOffset] = 0x41;
        bytes[header + layout.VolumeNameOffset] = (byte)'T';
        bytes[header + layout.VolumeNameOffset + 1] = (byte)'E';
        bytes[header + layout.VolumeNameOffset + 2] = (byte)'S';
        bytes[header + layout.VolumeNameOffset + 3] = (byte)'T';
        bytes[header + layout.VolumeNameOffset + 4] = 0xa0;
        return bytes;
    }

    private static string Extension(string formatId) => formatId switch
    {
        DiskImageFormatIds.Commodore1541 => ".d64",
        DiskImageFormatIds.Commodore1571 => ".d71",
        _ => ".d81"
    };
}
