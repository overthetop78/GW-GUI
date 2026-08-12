using System.IO;
using GWGUI.MediaEngine.Containers.Commodore.D64;
using GWGUI.MediaEngine.Containers.Commodore.D71;
using GWGUI.MediaEngine.Containers.Commodore.D81;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Commodore.Dos;
using GWGUI.MediaEngine.Primitives;
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
            var layout = CommodoreDosLayout.Resolve(formatId)!;
            Assert.True(CommodoreDosGeometry.TryToLogicalBlock(image, layout.HeaderTrack, layout.HeaderSector, out var headerBlock));
            Assert.True(image.TryGetBlock(headerBlock, out var header));
            Assert.Equal(layout.HeaderSignature, header.Data[CommodoreDosLayout.DirectoryEntriesOffset]);
            Assert.True(CommodoreDosGeometry.TryToLogicalBlock(image, layout.DirectoryTrack, layout.DirectorySector, out var directoryBlock));
            Assert.True(image.TryGetBlock(directoryBlock, out var directory));
            Assert.Equal(CommodoreDosLayout.SectorSize, directory.Data.Count);
            Assert.All(directory.Data, value => Assert.Equal(0, value));
            Assert.True(CommodoreDosDirectoryReader.IsPlausible(image, layout.DirectoryTrack, layout.DirectorySector));
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
        Assert.Equal("AZ", PetsciiCodec.Decode([0x41, 0x7a, 0xa0, 0x42]));
        Assert.Equal("�", PetsciiCodec.Decode([0x1f]));
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
            Assert.Contains(cyclic.Warnings, warning => warning.Contains("cyclique", StringComparison.Ordinal));
            var missing = new SectorImage(image.FormatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, image.AvailableBlocks.Where(block => block.LogicalBlock != dataLogical), logicalBlockCount: image.BlockCount);
            var absent = new CommodoreDosFileSystemReader().Read(missing);
            Assert.Contains(absent.Warnings, warning => warning.Contains("absent", StringComparison.Ordinal));
        }
        finally { File.Delete(path); }
    }

    [Fact]
    public void FileChainsDistinguishValidEmptyMissingTruncatedAndInvalidFinalSectors()
    {
        var validSector = new byte[CommodoreDosLayout.SectorSize];
        validSector[CommodoreDosLayout.NextSectorOffset] = 3;
        validSector[CommodoreDosLayout.LinkLength] = 0x11;
        validSector[CommodoreDosLayout.LinkLength + 1] = 0x22;
        var validWarnings = new List<string>();
        var valid = CommodoreDosFileReader.Read(CreateSparse1541Image(new Dictionary<int, byte[]> { [0] = validSector }), 1, 0, validWarnings, "valid");
        Assert.True(valid.IsValid);
        Assert.Equal(new byte[] { 0x11, 0x22 }, valid.Content);
        Assert.Empty(validWarnings);
        Assert.True(CommodoreDosFileReader.Read(CreateSparse1541Image(new Dictionary<int, byte[]>()), 0, 0, [], "empty").IsValid);
        var missing = CommodoreDosFileReader.Read(CreateSparse1541Image(new Dictionary<int, byte[]>()), 1, 0, [], "missing");
        Assert.False(missing.IsValid);
        var truncated = CommodoreDosFileReader.Read(CreateSparse1541Image(new Dictionary<int, byte[]> { [0] = new byte[10] }, true), 1, 0, [], "truncated");
        Assert.False(truncated.IsValid);
        var invalidFinalSector = new byte[CommodoreDosLayout.SectorSize];
        invalidFinalSector[CommodoreDosLayout.NextSectorOffset] = 0;
        var invalidFinal = CommodoreDosFileReader.Read(CreateSparse1541Image(new Dictionary<int, byte[]> { [0] = invalidFinalSector }), 1, 0, [], "invalid-final");
        Assert.False(invalidFinal.IsValid);
    }

    [Fact]
    public void CommodoreDosGeometrySupportsBoth1571SidesAndRejectsInvalidCoordinates()
    {
        var image = new SectorImage(DiskImageFormatIds.Commodore1571, CommodoreDosLayout.SectorSize, 35, 2, 1, [], logicalBlockCount: D71Layout.Tracks35.DataBlockCount);
        Assert.True(CommodoreDosGeometry.TryToLogicalBlock(image, 18, 0, out var firstSide));
        Assert.True(CommodoreDosGeometry.TryToLogicalBlock(image, 53, 0, out var secondSide));
        Assert.Equal(Commodore1541Geometry.BlocksPerSide(35), secondSide - firstSide);
        Assert.False(CommodoreDosGeometry.TryToLogicalBlock(image, 0, 0, out _));
    }

    [Fact]
    public void DirectoryReaderDistinguishesCycleMissingAndTruncatedSectors()
    {
        var directoryBlock = Commodore1541Geometry.ToSideLogicalBlock(18, 1, 35);
        var cyclicSector = new byte[CommodoreDosLayout.SectorSize];
        cyclicSector[CommodoreDosLayout.NextTrackOffset] = 18;
        cyclicSector[CommodoreDosLayout.NextSectorOffset] = 1;
        var cycleWarnings = new List<string>();
        CommodoreDosDirectoryReader.Read(CreateSparse1541Image(new Dictionary<int, byte[]> { [directoryBlock] = cyclicSector }), 18, 1, cycleWarnings);
        Assert.Contains(cycleWarnings, warning => warning.Contains("cyclique", StringComparison.Ordinal));
        var missingWarnings = new List<string>();
        CommodoreDosDirectoryReader.Read(CreateSparse1541Image(new Dictionary<int, byte[]>()), 18, 1, missingWarnings);
        Assert.Contains(missingWarnings, warning => warning.Contains("absent", StringComparison.Ordinal));
        var truncatedWarnings = new List<string>();
        CommodoreDosDirectoryReader.Read(CreateSparse1541Image(new Dictionary<int, byte[]> { [directoryBlock] = new byte[10] }, true), 18, 1, truncatedWarnings);
        Assert.Contains(truncatedWarnings, warning => warning.Contains("ne contient que", StringComparison.Ordinal));
    }

    [Fact]
    public void BamReadersDistinguishZeroFreeBlocksFromMissingSectors()
    {
        var d64HeaderBlock = Commodore1541Geometry.ToSideLogicalBlock(Commodore1541DosLayout.HeaderTrack, Commodore1541DosLayout.HeaderSector, 35);
        var presentD64 = CreateSparse1541Image(new Dictionary<int, byte[]> { [d64HeaderBlock] = new byte[CommodoreDosLayout.SectorSize] });
        var d64Warnings = new List<string>();
        Assert.Equal(0, Commodore1541BamReader.Read(presentD64, d64Warnings).FreeBlocks);
        Assert.Empty(d64Warnings);
        var missingWarnings = new List<string>();
        Assert.Null(Commodore1541BamReader.Read(CreateSparse1541Image(new Dictionary<int, byte[]>()), missingWarnings).FreeBlocks);
        Assert.NotEmpty(missingWarnings);
        var truncatedWarnings = new List<string>();
        Assert.Null(Commodore1541BamReader.Read(CreateSparse1541Image(new Dictionary<int, byte[]> { [d64HeaderBlock] = new byte[10] }, true), truncatedWarnings).FreeBlocks);
        Assert.Contains(truncatedWarnings, warning => warning.Contains("tronqué", StringComparison.Ordinal));

        var blocksPerSide = Commodore1541Geometry.BlocksPerSide(35);
        var d71 = new SectorImage(DiskImageFormatIds.Commodore1571, CommodoreDosLayout.SectorSize, 35, 2, 1, [new SectorBlock(d64HeaderBlock, new SectorAddress(17, 0, 0), new byte[CommodoreDosLayout.SectorSize]), new SectorBlock(blocksPerSide + d64HeaderBlock, new SectorAddress(17, 1, 0), new byte[CommodoreDosLayout.SectorSize])], logicalBlockCount: D71Layout.Tracks35.DataBlockCount);
        Assert.Equal(0, Commodore1541BamReader.Read(d71, []).FreeBlocks);
        var d71MissingWarnings = new List<string>();
        var d71MissingSecond = new SectorImage(DiskImageFormatIds.Commodore1571, CommodoreDosLayout.SectorSize, 35, 2, 1, [new SectorBlock(d64HeaderBlock, new SectorAddress(17, 0, 0), new byte[CommodoreDosLayout.SectorSize])], logicalBlockCount: D71Layout.Tracks35.DataBlockCount);
        Assert.Null(Commodore1541BamReader.Read(d71MissingSecond, d71MissingWarnings).FreeBlocks);
        Assert.NotEmpty(d71MissingWarnings);
        var d71TruncatedWarnings = new List<string>();
        var d71TruncatedSecond = new SectorImage(DiskImageFormatIds.Commodore1571, CommodoreDosLayout.SectorSize, 35, 2, 1, [new SectorBlock(d64HeaderBlock, new SectorAddress(17, 0, 0), new byte[CommodoreDosLayout.SectorSize]), new SectorBlock(blocksPerSide + d64HeaderBlock, new SectorAddress(17, 1, 0), new byte[10])], true, logicalBlockCount: D71Layout.Tracks35.DataBlockCount);
        Assert.Null(Commodore1541BamReader.Read(d71TruncatedSecond, d71TruncatedWarnings).FreeBlocks);
        Assert.Contains(d71TruncatedWarnings, warning => warning.Contains("tronqué", StringComparison.Ordinal));

        var first = Commodore1581Geometry.ToLogicalBlock(Commodore1581DosLayout.HeaderTrack, Commodore1581DosLayout.FirstBamSector);
        var second = Commodore1581Geometry.ToLogicalBlock(Commodore1581DosLayout.HeaderTrack, Commodore1581DosLayout.SecondBamSector);
        var d81 = new SectorImage(DiskImageFormatIds.Commodore1581, CommodoreDosLayout.SectorSize, 80, 1, 40, [new SectorBlock(first, new SectorAddress(39, 0, 1), new byte[CommodoreDosLayout.SectorSize]), new SectorBlock(second, new SectorAddress(39, 0, 2), new byte[CommodoreDosLayout.SectorSize])]);
        Assert.Equal(0, Commodore1581BamReader.Read(d81, []).FreeBlocks);
        var d81Warnings = new List<string>();
        var d81MissingSecond = new SectorImage(DiskImageFormatIds.Commodore1581, CommodoreDosLayout.SectorSize, 80, 1, 40, [new SectorBlock(first, new SectorAddress(39, 0, 1), new byte[CommodoreDosLayout.SectorSize])]);
        Assert.Null(Commodore1581BamReader.Read(d81MissingSecond, d81Warnings).FreeBlocks);
        Assert.NotEmpty(d81Warnings);
        var d81TruncatedWarnings = new List<string>();
        var d81TruncatedSecond = new SectorImage(DiskImageFormatIds.Commodore1581, CommodoreDosLayout.SectorSize, 80, 1, 40, [new SectorBlock(first, new SectorAddress(39, 0, 1), new byte[CommodoreDosLayout.SectorSize]), new SectorBlock(second, new SectorAddress(39, 0, 2), new byte[10])], true);
        Assert.Null(Commodore1581BamReader.Read(d81TruncatedSecond, d81TruncatedWarnings).FreeBlocks);
        Assert.Contains(d81TruncatedWarnings, warning => warning.Contains("tronqué", StringComparison.Ordinal));
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
        bytes[header + CommodoreDosLayout.DirectoryEntriesOffset] = layout.HeaderSignature;
        bytes[header + layout.VolumeNameOffset] = (byte)'T';
        bytes[header + layout.VolumeNameOffset + 1] = (byte)'E';
        bytes[header + layout.VolumeNameOffset + 2] = (byte)'S';
        bytes[header + layout.VolumeNameOffset + 3] = (byte)'T';
        bytes[header + layout.VolumeNameOffset + 4] = 0xa0;
        return bytes;
    }

    /// <summary>Crée une image 1541 contenant seulement les blocs logiques indiqués.</summary>
    private static SectorImage CreateSparse1541Image(IReadOnlyDictionary<int, byte[]> blocks, bool allowVariableBlockSize = false) => new(DiskImageFormatIds.Commodore1541, CommodoreDosLayout.SectorSize, 35, 1, 1, blocks.Select(item => new SectorBlock(item.Key, new SectorAddress(0, 0, item.Key), item.Value)), allowVariableBlockSize, logicalBlockCount: D64Layout.Tracks35.DataBlockCount);

    private static string Extension(string formatId) => formatId switch
    {
        DiskImageFormatIds.Commodore1541 => ".d64",
        DiskImageFormatIds.Commodore1571 => ".d71",
        _ => ".d81"
    };
}
