using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Cpm;
using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie les définitions, dispositions et lecteurs CP/M communs.</summary>
public sealed class CpmDefinitionsTests
{
    /// <summary>Vérifie la résolution de tous les formats catalogués et l'absence d'un format inconnu.</summary>
    [Fact]
    public void CatalogResolvesEveryImmutableFormat()
    {
        var expected = new[] { DiskImageFormatIds.Commodore1541, DiskImageFormatIds.Commodore1571, DiskImageFormatIds.Commodore1581, DiskImageFormatIds.EpsonQx10_320, DiskImageFormatIds.EpsonQx10_396, DiskImageFormatIds.EpsonQx10_399, DiskImageFormatIds.EpsonQx10_400, DiskImageFormatIds.EpsonQx10Logo };
        Assert.Equal(expected.Order(), CpmLayoutCatalog.FormatIds.Order());
        Assert.All(expected, format => Assert.NotNull(CpmLayoutCatalog.Resolve(format)));
        Assert.Null(CpmLayoutCatalog.Resolve("unknown"));
        var mutableView = Assert.IsAssignableFrom<ISet<string>>(CpmLayoutCatalog.FormatIds);
        Assert.Throws<NotSupportedException>(() => mutableView.Add("other"));
    }

    /// <summary>Vérifie la validation des layouts et la copie des allocations d'un extent.</summary>
    [Fact]
    public void LayoutAndExtentProtectTheirState()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CpmLayout(-1, 0, 1, 1, 1, false));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CpmLayout(0, 0, 0, 1, 1, false));
        var allocations = new[] { 1, 2 };
        var extent = new CpmExtent(3, "FILE.TXT", 0, 1, allocations);
        allocations[0] = 9;
        Assert.Equal(1, extent.Allocations[0]);
    }

    /// <summary>Vérifie noms, attributs, minuscules, label, mot de passe, utilisateurs et extents successifs.</summary>
    [Fact]
    public void DirectoryReaderDecodesEntriesAndReservedUsers()
    {
        var bytes = Enumerable.Repeat(CpmFormat.UnusedEntryMarker, 8 * CpmFormat.DirectoryEntrySize).ToArray();
        WriteEntry(bytes, 0, CpmFormat.VolumeLabelUser, "VOLUME", "", 0, 0, []);
        WriteEntry(bytes, 1, CpmFormat.PasswordLabelUser, "SECRET", "", 0, 0, []);
        WriteEntry(bytes, 2, 3, "FILE", "TXT", 0, 1, [1]);
        WriteEntry(bytes, 3, 3, "FILE", "TXT", 1, 1, [2]);
        bytes[2 * CpmFormat.DirectoryEntrySize + CpmFormat.FileNameOffset] |= 0x80;
        var image = Logical(bytes);
        var layout = new CpmLayout(0, 0, 8, 128, 1, false);
        var directory = CpmDirectoryReader.ReadDirectory(image, layout, rejectLowercase: true);
        Assert.Equal("VOLUME", directory.VolumeName);
        Assert.Equal(2, directory.Extents.Count);
        Assert.All(directory.Extents, extent => Assert.Equal((byte)3, extent.User));
        Assert.False(CpmDirectoryReader.TryDecodeName("file    txt                     "u8, rejectLowercase: true, out _));
    }

    /// <summary>Vérifie que l'aplatissement conserve les positions des blocs absents et tronqués.</summary>
    [Fact]
    public void FlattenPreservesMissingAndTruncatedBlockPositions()
    {
        var image = new SectorImage("test", 4, 1, 1, 3, [Block(0, [1, 2, 3, 4]), Block(2, [9, 8])], allowVariableBlockSize: true);
        var logical = CpmDirectoryReader.Flatten(image);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 0, 0, 0, 0, 9, 8, 0, 0 }, logical.Bytes);
        Assert.Contains(1, logical.MissingBlocks);
        Assert.Contains(2, logical.TruncatedBlocks);
    }

    /// <summary>Vérifie les dispositions CPC système, CPC données et PCW par le lecteur public.</summary>
    [Theory]
    [InlineData("system")]
    [InlineData("data")]
    [InlineData("pcw")]
    public void PublicAmstradReaderReadsNamedLayouts(string kind)
    {
        var image = BuildAmstrad(kind, empty: false);
        var reader = new AmstradCpmFileSystemReader();
        Assert.True(reader.CanRead(image));
        var volume = reader.Read(image);
        Assert.Contains(volume.Entries, entry => entry.Name == "FILE.TXT" && entry.Content!.Take(4).SequenceEqual("DATA"u8.ToArray()));
    }

    /// <summary>Vérifie qu'un répertoire PCW vide est accepté et qu'une spécification invalide est rejetée.</summary>
    [Fact]
    public void PcwAllowsEmptyDirectoryAndRejectsInvalidSpecification()
    {
        var reader = new AmstradCpmFileSystemReader();
        Assert.True(reader.CanRead(BuildAmstrad("pcw", empty: true)));
        var invalid = Replace(BuildAmstrad("pcw", empty: true), 0, data => data[2] = 0);
        Assert.False(reader.CanRead(invalid));
    }

    /// <summary>Vérifie allocations hors limites, absentes et dupliquées sans décaler les données suivantes.</summary>
    [Fact]
    public void ReconstructionReportsInvalidAndDuplicateAllocations()
    {
        var bytes = new byte[8192];
        var layout = new CpmLayout(0, 0, 8, 512, 1, false);
        FillDirectory(bytes, layout);
        WriteEntry(bytes, 0, 0, "FILE", "BIN", 0, 12, [2, 3, 250, 2]);
        "FIRST"u8.CopyTo(bytes.AsSpan(2 * 512));
        "SECOND"u8.CopyTo(bytes.AsSpan(3 * 512));
        var logical = Logical(bytes);
        var directory = CpmDirectoryReader.ReadDirectory(logical, layout, true);
        var warnings = new List<string>();
        var result = CpmDirectoryReader.Reconstruct(logical, layout, CpmDirectoryReader.GroupExtents(directory.Extents).Single(), warnings);
        Assert.False(result.Rejected);
        Assert.False(result.Valid);
        Assert.Contains(warnings, warning => warning.Contains("outside", StringComparison.Ordinal));
        Assert.Contains(warnings, warning => warning.Contains("more than once", StringComparison.Ordinal));
        Assert.Equal((byte)'S', result.Content[512]);
    }

    private static SectorImage BuildAmstrad(string kind, bool empty)
    {
        const int blockSize = 512;
        var blockCount = kind == "pcw" ? 600 : 48;
        var bytes = new byte[blockCount * blockSize];
        var directoryOffset = kind switch { "data" => AmstradCpmLayout.CpcData.DirectoryOffset, "pcw" => 9 * blockSize, _ => 0 };
        var allocationOrigin = directoryOffset;
        if (kind == "pcw") { bytes[2] = 80; bytes[3] = 9; bytes[4] = 2; bytes[5] = 1; bytes[6] = 3; bytes[7] = 2; }
        bytes.AsSpan(directoryOffset, 64 * CpmFormat.DirectoryEntrySize).Fill(CpmFormat.UnusedEntryMarker);
        if (!empty)
        {
            var wide = kind == "pcw";
            WriteEntry(bytes, directoryOffset / CpmFormat.DirectoryEntrySize, 0, "FILE", "TXT", 0, 1, [2], wide);
            "DATA"u8.CopyTo(bytes.AsSpan(allocationOrigin + 2 * 1024));
        }
        var firstSector = kind == "data" ? AmstradCpmLayout.DataFirstSectorId : AmstradCpmLayout.SystemFirstSectorId;
        var format = kind == "pcw" ? DiskImageFormatIds.AmstradPcw : DiskImageFormatIds.AmstradCpc;
        return new(format, blockSize, blockCount, 1, 1, Enumerable.Range(0, blockCount).Select(block => new SectorBlock(block, new(block, 0, block == 0 ? firstSector : 1), bytes.AsSpan(block * blockSize, blockSize).ToArray())));
    }

    private static CpmDirectoryReader.LogicalImage Logical(byte[] bytes) => new(bytes, 1, new HashSet<int>(), new HashSet<int>());
    private static SectorBlock Block(int logical, byte[] data) => new(logical, new(0, 0, logical), data);
    private static void FillDirectory(byte[] bytes, CpmLayout layout) => bytes.AsSpan(layout.DirectoryOffset, layout.DirectoryEntries * CpmFormat.DirectoryEntrySize).Fill(CpmFormat.UnusedEntryMarker);

    private static void WriteEntry(byte[] bytes, int index, byte user, string stem, string extension, byte extent, byte records, int[] allocations, bool wide = false)
    {
        var offset = index * CpmFormat.DirectoryEntrySize;
        bytes.AsSpan(offset, CpmFormat.DirectoryEntrySize).Clear();
        bytes[offset] = user;
        System.Text.Encoding.ASCII.GetBytes(stem.PadRight(CpmFormat.FileNameLength)).CopyTo(bytes, offset + CpmFormat.FileNameOffset);
        System.Text.Encoding.ASCII.GetBytes(extension.PadRight(CpmFormat.FileExtensionLength)).CopyTo(bytes, offset + CpmFormat.FileExtensionOffset);
        bytes[offset + CpmFormat.ExtentLowOffset] = extent;
        bytes[offset + CpmFormat.RecordCountOffset] = records;
        for (var allocationIndex = 0; allocationIndex < allocations.Length; allocationIndex++)
        {
            if (wide) System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset + CpmFormat.AllocationOffset + allocationIndex * 2), (ushort)allocations[allocationIndex]);
            else bytes[offset + CpmFormat.AllocationOffset + allocationIndex] = (byte)allocations[allocationIndex];
        }
    }

    private static SectorImage Replace(SectorImage source, int blockNumber, Action<byte[]> update)
    {
        var blocks = source.AvailableBlocks.Select(block => { if (block.LogicalBlock != blockNumber) return block; var data = block.Data.ToArray(); update(data); return block with { Data = data }; });
        return new(source.FormatId, source.BlockSize, source.Cylinders, source.Heads, source.SectorsPerTrack, blocks);
    }
}
