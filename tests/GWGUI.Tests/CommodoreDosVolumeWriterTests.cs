using GWGUI.MediaEngine.Definitions;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Commodore.Dos;
using GWGUI.MediaEngine.FileSystems.Definitions;
using GWGUI.MediaEngine.Migration;

namespace GWGUI.Tests;

public sealed class CommodoreDosVolumeWriterTests
{
    [Theory]
    [InlineData(DiskImageFormatIds.Commodore1541, 683)]
    [InlineData(DiskImageFormatIds.Commodore1571, 1366)]
    [InlineData(DiskImageFormatIds.Commodore1581, 3200)]
    public void VolumesRoundTripBamDirectoryChainsAndContents(string formatId, int blockCount)
    {
        var entries = Enumerable.Range(0, 12).Select(index => CreateFile($"FILE {index:00}", 300 + index * 31, (uint)(byte)(CommodoreDosFileType.Closed | CommodoreDosFileType.Prg))).ToArray();
        var plan = new MigrationPlan(FileSystemIds.CommodoreDos, FileSystemIds.CommodoreDos, "GW TEST", entries);

        var image = new CommodoreDosVolumeWriter().Create(plan, formatId);
        var volume = new CommodoreDosFileSystemReader().Read(image);

        Assert.Equal(blockCount, image.BlockCount);
        Assert.Equal("GW TEST", volume.Name);
        Assert.Empty(volume.Warnings);
        Assert.Equal(entries.Length, volume.Entries.Count);
        foreach (var source in entries) Assert.Equal(source.Content, Assert.Single(volume.Entries, entry => entry.Name == source.TargetName).Content);
    }

    [Fact]
    public void CommodoreMetadataPreservesTypesLocksSplatsAndRelativeRecords()
    {
        var entries = new[]
        {
            CreateFile("PROGRAM", 700, (uint)(byte)(CommodoreDosFileType.Prg | CommodoreDosFileType.Closed)),
            CreateFile("SEQUENCE", 100, (uint)(byte)(CommodoreDosFileType.Seq | CommodoreDosFileType.Closed | CommodoreDosFileType.Locked)),
            CreateFile("USER SPLAT", 10, (uint)(byte)CommodoreDosFileType.Usr),
            CreateFile("RELATIVE", 4_000, (uint)(byte)(CommodoreDosFileType.Rel | CommodoreDosFileType.Closed) | 40u << CommodoreDosLayout.RelativeRecordLengthAttributeShift)
        };
        var plan = new MigrationPlan(FileSystemIds.CommodoreDos, FileSystemIds.CommodoreDos, "METADATA", entries);

        var volume = new CommodoreDosFileSystemReader().Read(new CommodoreDosVolumeWriter().Create(plan, DiskImageFormatIds.Commodore1541));

        Assert.Empty(volume.Warnings);
        Assert.Contains("PRG", Assert.Single(volume.Entries, entry => entry.Name == "PROGRAM").Comment);
        Assert.Contains("locked", Assert.Single(volume.Entries, entry => entry.Name == "SEQUENCE").Comment);
        Assert.Contains("open", Assert.Single(volume.Entries, entry => entry.Name == "USER SPLAT").Comment);
        var relative = Assert.Single(volume.Entries, entry => entry.Name == "RELATIVE");
        Assert.Equal(entries[3].RawAttributes, relative.RawAttributes);
        Assert.Equal(entries[3].Content, relative.Content);
    }

    [Fact]
    public void ForeignFilesUseTheExplicitWritePolicy()
    {
        var plan = new MigrationPlan("foreign.fs", FileSystemIds.CommodoreDos, "TARGET", [CreateFile("DATA", 100, 0)]);
        var policy = new CommodoreDosWritePolicy(CommodoreDosFileType.Seq | CommodoreDosFileType.Locked);

        var volume = new CommodoreDosFileSystemReader().Read(new CommodoreDosVolumeWriter().Create(plan, DiskImageFormatIds.Commodore1541, policy));

        var entry = Assert.Single(volume.Entries);
        Assert.Contains("SEQ", entry.Comment);
        Assert.Contains("locked", entry.Comment);
    }

    private static MigrationEntry CreateFile(string name, int length, uint rawAttributes)
    {
        var content = Enumerable.Range(0, length).Select(index => (byte)(index * 29)).ToArray();
        return new(name, name, FileSystemEntryKind.File, content, null, string.Empty, rawAttributes, true, []);
    }
}
