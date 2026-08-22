using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.FileSystems.Acorn.Adfs;
using GWGUI.MediaEngine.FileSystems.Definitions;
using System.Collections.Frozen;

namespace GWGUI.Tests;

public sealed class FileSystemModelsTests
{
    [Fact]
    public void EntryCopiesChildrenAndContentAndPreservesContentPresence()
    {
        var children = new List<FileSystemEntry>();
        var content = new List<byte> { 1 };
        var entry = new FileSystemEntry("FILE", FileSystemEntryKind.File, 1, null, string.Empty, 0, 0, true, children, content);
        children.Add(new FileSystemEntry("CHILD", FileSystemEntryKind.Unknown, 0, null, string.Empty, 0, 0, true, []));
        content.Add(2);
        Assert.Empty(entry.Children);
        Assert.Equal([1], entry.Content);
        Assert.Null(new FileSystemEntry("ABSENT", FileSystemEntryKind.File, 0, null, string.Empty, 0, 0, true, [], null).Content);
        Assert.Empty(new FileSystemEntry("EMPTY", FileSystemEntryKind.File, 0, null, string.Empty, 0, 0, true, [], []).Content!);
    }

    [Fact]
    public void VolumeCopiesEntriesAndWarnings()
    {
        var entries = new List<FileSystemEntry>();
        var warnings = new List<string>();
        var volume = new FileSystemVolume("VOLUME", FileSystemIds.AcornAdfs, 1, 0, null, null, entries, warnings);
        entries.Add(new FileSystemEntry("FILE", FileSystemEntryKind.File, 0, null, string.Empty, 0, 0, true, []));
        warnings.Add("warning");
        Assert.Empty(volume.Entries);
        Assert.Empty(volume.Warnings);
    }

    [Fact]
    public void EntryKindsRemainDistinct() => Assert.Equal(4, Enum.GetValues<FileSystemEntryKind>().Distinct().Count());

    [Fact]
    public void ReaderUsesCentralIdsAndAnImmutableCatalog()
    {
        var reader = new AcornAdfsFileSystemReader();
        Assert.Equal(FileSystemIds.AcornAdfs, reader.Id);
        Assert.IsAssignableFrom<FrozenSet<string>>(reader.CatalogFormatIds);
        Assert.All(reader.CatalogFormatIds, formatId => Assert.False(string.IsNullOrWhiteSpace(formatId)));
    }
}
