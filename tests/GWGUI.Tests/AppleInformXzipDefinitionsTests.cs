using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems.Apple.InformXzip;

using GWGUI.MediaEngine.SectorImages;

namespace GWGUI.Tests;

/// <summary>Vérifie les définitions et validations Inform/XZIP.</summary>
public sealed class AppleInformXzipDefinitionsTests
{
    /// <summary>Vérifie l'ordre exact des seize secteurs entrelacés.</summary>
    [Fact]
    public void PreservesExactInterleaveOrder() => Assert.Equal(new[] { 0, 7, 14, 6, 13, 5, 12, 4, 11, 3, 10, 2, 9, 1, 8, 15 }, AppleInformXzipLayout.Interleave);

    /// <summary>Vérifie une histoire version 5 valide et son checksum.</summary>
    [Fact]
    public void ParsesValidVersionFiveHeaderAndChecksum()
    {
        var story = Story();
        Assert.True(ZMachineV5Header.TryParse(story, out var header));
        Assert.Equal(AppleInformXzipLayout.ZMachineVersion, header.Version);
        Assert.True(header.ChecksumMatches(story));
    }

    /// <summary>Vérifie le rejet d'une version, d'une adresse, d'une longueur et d'un checksum invalides.</summary>
    [Fact]
    public void RejectsInvalidHeaderFieldsAndChecksum()
    {
        var story = Story();
        story[AppleInformXzipLayout.VersionOffset] = 4;
        Assert.False(ZMachineV5Header.TryParse(story, out _));
        story = Story();
        BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(AppleInformXzipLayout.InitialProgramCounterOffset), 0);
        Assert.False(ZMachineV5Header.TryParse(story, out _));
        story = Story();
        BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(AppleInformXzipLayout.LengthOffset), ushort.MaxValue);
        Assert.False(ZMachineV5Header.TryParse(story, out _));
        story = Story(); story[^1] ^= 1;
        Assert.True(ZMachineV5Header.TryParse(story, out var header));
        Assert.False(header.ChecksumMatches(story));
    }

    /// <summary>Vérifie qu'un secteur logique absent est signalé par le lecteur public.</summary>
    [Fact]
    public void PublicReaderReportsMissingLogicalSector()
    {
        var image = Image(Story());
        var missing = new SectorImage(image.FormatId, image.BlockSize, image.Cylinders, image.Heads, image.SectorsPerTrack, image.AvailableBlocks.Where(block => block.LogicalBlock != AppleInformXzipLayout.InterpreterSectorCount));
        Assert.Contains(AppleInformXzipLayout.InterpreterSectorCount.ToString(), Assert.Throws<InvalidDataException>(() => new AppleInformXzipFileSystemReader().Read(missing)).Message, StringComparison.Ordinal);
    }

    private static byte[] Story()
    {
        const int length = 1024;
        var story = new byte[length];
        story[AppleInformXzipLayout.VersionOffset] = AppleInformXzipLayout.ZMachineVersion;
        BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(AppleInformXzipLayout.HighMemoryOffset), 0x100);
        BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(AppleInformXzipLayout.InitialProgramCounterOffset), 0x120);
        BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(AppleInformXzipLayout.DictionaryOffset), 0x200);
        BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(AppleInformXzipLayout.ObjectsOffset), 0x240);
        BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(AppleInformXzipLayout.GlobalsOffset), 0x280);
        BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(AppleInformXzipLayout.StaticMemoryOffset), 0x300);
        BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(AppleInformXzipLayout.LengthOffset), length / AppleInformXzipLayout.LengthUnit);
        for (var index = AppleInformXzipLayout.ChecksumDataOffset; index < length; index++) story[index] = (byte)(index * 17 + 3);
        var checksum = story.Skip(AppleInformXzipLayout.ChecksumDataOffset).Aggregate(0, (sum, value) => (sum + value) & ushort.MaxValue);
        BinaryPrimitives.WriteUInt16BigEndian(story.AsSpan(AppleInformXzipLayout.ChecksumOffset), (ushort)checksum);
        return story;
    }

    private static SectorImage Image(byte[] story)
    {
        var data = new byte[AppleInformXzipLayout.TrackCount * AppleInformXzipLayout.SectorsPerTrack * AppleInformXzipLayout.SectorSize];
        for (var sector = 0; sector < AppleInformXzipLayout.MaximumStorySectorCount; sector++)
        {
            var stored = AppleInformXzipLayout.InterpreterSectorCount + (sector & AppleInformXzipLayout.StoryTrackMask) + AppleInformXzipLayout.StoredSectorIndex(sector & AppleInformXzipLayout.SectorInTrackMask);
            if (sector * AppleInformXzipLayout.SectorSize < story.Length) story.AsSpan(sector * AppleInformXzipLayout.SectorSize, Math.Min(AppleInformXzipLayout.SectorSize, story.Length - sector * AppleInformXzipLayout.SectorSize)).CopyTo(data.AsSpan(stored * AppleInformXzipLayout.SectorSize));
        }
        var blocks = Enumerable.Range(0, data.Length / AppleInformXzipLayout.SectorSize).Select(logical => new SectorBlock(logical, new(logical / AppleInformXzipLayout.SectorsPerTrack, 0, logical % AppleInformXzipLayout.SectorsPerTrack), data.AsSpan(logical * AppleInformXzipLayout.SectorSize, AppleInformXzipLayout.SectorSize).ToArray()));
        return new(DiskImageFormatIds.AppleIIDos33, AppleInformXzipLayout.SectorSize, AppleInformXzipLayout.TrackCount, 1, AppleInformXzipLayout.SectorsPerTrack, blocks);
    }
}
