using MediaVolumeOrigins = global::GWGUI.MediaFileSystems.Constants.MediaVolumeOrigins;
using MediaSourceDescriptor = global::GWGUI.MediaEngine.Contracts.MediaSourceDescriptor;
using MediaVolumeDescriptor = global::GWGUI.MediaFileSystems.Contracts.MediaVolumeDescriptor;
using GWGUI.MediaEngine.Enums;

using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Representations.Sequential;
using GWGUI.MediaFileSystems.Definitions;
using GWGUI.MediaFileSystems.Exploration.Sequential;

namespace GWGUI.Tests.Media;

public sealed class SequentialContentFileSystemReaderTests
{
    [Fact]
    public void KeepsStoredFileNameAndDoesNotPresentAnonymousBlocksAsFiles()
    {
        var volume = new MediaVolumeDescriptor(
            0, 4, MediaVolumeOrigins.SequentialContent, fileSystemId: FileSystemIds.SequentialContent);
        var document = new MediaImageDocument(
            new MediaSourceDescriptor("memory.uef", []),
            "tape.uef",
            MediaKind.Tape,
            new SequentialMediaImageRepresentation(4),
            [volume],
            [],
            new Dictionary<string, string>());
        var blocks = new[]
        {
            new MediaSequentialDecodedBlock(0, new byte[] { 1, 2 }, true,
                new Dictionary<string, string> { ["fileName"] = "HELLO", ["blockNumber"] = "0" }),
            new MediaSequentialDecodedBlock(1, new byte[] { 3 }, true,
                new Dictionary<string, string> { ["fileName"] = "HELLO", ["blockNumber"] = "1" }),
            new MediaSequentialDecodedBlock(2, new byte[] { 4 }, true,
                new Dictionary<string, string>())
        };

        var result = new SequentialContentFileSystemReader().Read(
            document, volume, new DecodedContent("acorn-tape", blocks, []), string.Empty);

        var file = Assert.Single(result.Entries);
        Assert.Equal("HELLO", file.Name);
        Assert.Equal(new byte[] { 1, 2, 3 }, file.Content);
        Assert.False(file.SyntheticName);
        Assert.Contains(result.Warnings, warning => warning.Contains("no stored file name", StringComparison.Ordinal));
    }

    private sealed record DecodedContent(
        string? DecoderId,
        IReadOnlyList<MediaSequentialDecodedBlock> Blocks,
        IReadOnlyList<string> Diagnostics) : IMediaSequentialContent;
}
