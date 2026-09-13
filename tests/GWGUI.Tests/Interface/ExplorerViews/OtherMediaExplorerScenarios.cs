using System.Windows;
using System.Windows.Controls;
using GWGUI.App.ViewModels.Explorer;
using GWGUI.App.Views.Controls.Explorer;
using GWGUI.Domain.Contracts;
using GWGUI.Domain.Enums;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Exploration.Results;
using GWGUI.MediaEngine.FileSystems;
using GWGUI.MediaEngine.Reading.Sources;
using GWGUI.MediaEngine.Representations.Blocks;
using GWGUI.MediaEngine.Representations.Optical;
using GWGUI.MediaEngine.Representations.Sequential;
using GWGUI.Tests.Application.TestInfrastructure;

namespace GWGUI.Tests.Interface.ExplorerViews;

[Collection("WPF")]
public sealed class OtherMediaExplorerScenarios(StaExecutionScenarios sta)
{
    [Fact]
    public Task HardDiskPartitionsCanBeSelectedIndependently() => sta.Run(() =>
    {
        var representation = new BlockMediaImageRepresentation(8_192, [(0, 8_192)]);
        var first = Volume(0, 4_096, "SYSTEM", 1, "SYSTEM.BIN");
        var second = Volume(4_096, 4_096, "DATA", 2, "DATA.BIN");
        var section = Display(Document(MediaKind.HardDisk, representation, [first, second]));
        var selector = Find<ComboBox>(section, "VolumeSelector");

        Assert.Equal(Visibility.Visible, selector.Visibility);
        Assert.Equal(2, selector.Items.Count);
        selector.SelectedIndex = 1;
        Assert.Equal("DATA", Find<TextBlock>(section, "VolumeNameText").Text);
        Assert.Equal("DATA.BIN", Assert.IsType<ExplorerContentItem>(
            Assert.Single(Find<ListView>(section, "ContentsList").Items.Cast<object>())).Entry.Name);
    });

    [Fact]
    public Task OpticalSessionsFilterTheirTracksAndVolumes() => sta.Run(() =>
    {
        var source = new MemoryRandomAccessData(new byte[6 * 2_048]);
        var tracks = new[]
        {
            new OpticalTrackDescriptor(1, 1, OpticalTrackMode.Mode1Data2048, 0, 2, 2_048, 0, 2_048, source, 0),
            new OpticalTrackDescriptor(1, 2, OpticalTrackMode.Audio, 2, 2, 2_352, 0, 2_352,
                new MemoryRandomAccessData(new byte[2 * 2_352]), 0),
            new OpticalTrackDescriptor(2, 3, OpticalTrackMode.Mode1Data2048, 4, 2, 2_048, 0, 2_048, source, 4 * 2_048)
        };
        var representation = new OpticalMediaImageRepresentation(6 * 2_048, tracks);
        var first = Volume(0, 4_096, "SESSION 1", session: 1, track: 1);
        var second = Volume(8_192, 4_096, "SESSION 2", session: 2, track: 3);
        var section = Display(Document(MediaKind.Optical, representation, [first, second]));
        var sessions = Find<ComboBox>(section, "SessionSelector");
        var audio = Find<ListBox>(section, "AudioTrackList");

        Assert.Equal(2, sessions.Items.Count);
        Assert.Single(audio.Items);
        sessions.SelectedIndex = 1;
        Assert.Equal("SESSION 2", Find<TextBlock>(section, "VolumeNameText").Text);
        Assert.Empty(audio.Items);
    });

    [Fact]
    public Task SequentialContentAppearsAsFiles() => sta.Run(() =>
    {
        var segment = new SequentialMediaSegment(0, SequentialSegmentKind.DataBlock, 3);
        var representation = new SequentialMediaImageRepresentation(3, segments: [segment]);
        var volume = Volume(0, 3, "TAPE", fileName: "PROGRAM.BIN");
        var section = Display(Document(MediaKind.Tape, representation, [volume]));

        Assert.Equal(Visibility.Collapsed, Find<ComboBox>(section, "SessionSelector").Visibility);
        Assert.Equal("PROGRAM.BIN", Assert.IsType<ExplorerContentItem>(
            Assert.Single(Find<ListView>(section, "ContentsList").Items.Cast<object>())).Entry.Name);
    });

    [Fact]
    public Task AtariCasUsesCassetteSummaryAndLogicalFiles() => sta.Run(() =>
    {
        var segment = new SequentialMediaSegment(0, SequentialSegmentKind.DataBlock, 132, duration: TimeSpan.FromSeconds(2));
        var representation = new SequentialMediaImageRepresentation(132, TimeSpan.FromSeconds(2), [segment]);
        var volume = Volume(0, 132, "TEST TAPE", fileName: "TEST TAPE.bin");
        var section = Display(Document(
            MediaKind.Tape,
            representation,
            [volume],
            "tape.atari-cas",
            new Dictionary<string, string> { ["systemId"] = "atari-8bit" }));

        Assert.Equal("Atari CAS", Find<TextBlock>(section, "FileSystemText").Text);
        Assert.Equal("Atari 8-bit", Find<TextBlock>(section, "SystemText").Text);
        Assert.Equal("1", Find<TextBlock>(section, "EntryCountText").Text);
    });

    private static ExplorerSection Display(ExploredMediaImage document)
    {
        var section = new ExplorerSection();
        section.Clear();
        section.Display(document);
        return section;
    }

    private static ExploredMediaImage Document(
        MediaKind kind,
        GWGUI.MediaEngine.Interfaces.IMediaImageRepresentation representation,
        IReadOnlyList<ExploredMediaVolume> volumes,
        string formatId = "test.media",
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        var document = new MediaImageDocument(
            new MediaSourceDescriptor("memory.media", []),
            formatId,
            kind,
            representation,
            volumes.Select(volume => volume.Descriptor).ToArray(),
            [],
            metadata ?? new Dictionary<string, string>());
        return new ExploredMediaImage(document, volumes, []);
    }

    private static ExploredMediaVolume Volume(
        long start,
        long length,
        string name,
        int? partition = null,
        string fileName = "FILE.BIN",
        int? session = null,
        int? track = null)
    {
        var descriptor = new MediaVolumeDescriptor(
            start,
            length,
            "test",
            partitionNumber: partition,
            sessionNumber: session,
            trackNumber: track,
            name: name);
        var entry = new FileSystemEntry(fileName, FileSystemEntryKind.File, 1, null, string.Empty, 0, 0, true, [], [1]);
        var fileSystem = new FileSystemVolume(name, "test.fs", length, 0, null, null, [entry], [], freeSpaceKnown: false);
        return new ExploredMediaVolume(descriptor, "test.reader", fileSystem, []);
    }

    private static T Find<T>(ExplorerSection section, string name) where T : class =>
        Assert.IsType<T>(section.FindName(name));
}
