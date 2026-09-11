using System.Buffers.Binary;
using System.Text;
using GWGUI.Domain.Contracts;
using GWGUI.MediaEngine.Composition;
using GWGUI.MediaEngine.Contracts;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Representations.Sequential;

namespace GWGUI.Tests.MediaEngine.Tape;

public sealed class SequentialMediaTests
{
    [Fact]
    public async Task WavKeepsTimelineChannelsAndUnknownChunks()
    {
        var path = Path.Combine(Path.GetTempPath(), $"gwgui-{Guid.NewGuid():N}.wav");
        try
        {
            var bytes = CreateStereoWav();
            await File.WriteAllBytesAsync(path, bytes);
            var document = await MediaEngineComposition.CreateDefault().ReadingService.ReadAsync(
                new MediaSourceDescriptor(path, [], bytes.Length));
            var representation = Assert.IsType<SequentialMediaImageRepresentation>(document.Representation);

            Assert.Equal("2", document.Metadata["channels"]);
            Assert.Equal(TimeSpan.FromSeconds(4d / 8_000), representation.Duration);
            Assert.Equal(new[] { 0, 1 }, representation.Channels);
            var renderedSegments = Assert.IsAssignableFrom<IReadOnlyList<SequentialMediaSegment>>(representation.Segments);
            Assert.Equal(2, renderedSegments.Count(segment => segment.Kind == SequentialSegmentKind.Samples));
            var unknown = Assert.Single(renderedSegments, segment => segment.Kind == SequentialSegmentKind.Unknown);
            Assert.Equal("JUNK", unknown.Metadata["chunkId"]);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void StructuredSegmentsExposeOnlyDeclaredTracksAndFaces()
    {
        var segments = new[]
        {
            new SequentialMediaSegment(0, SequentialSegmentKind.DataBlock, 10, TimeSpan.Zero, TimeSpan.FromSeconds(1), faceNumber: 0, trackNumber: 0),
            new SequentialMediaSegment(10, SequentialSegmentKind.Silence, 5, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), faceNumber: 1, trackNumber: 3)
        };
        var representation = new SequentialMediaImageRepresentation(15, TimeSpan.FromSeconds(2), segments);

        Assert.Equal(new[] { 0, 1 }, representation.Faces);
        Assert.Equal(new[] { 0, 3 }, representation.Tracks);
        Assert.Null(representation.Channels);
    }

    [Fact]
    public void DecodeResultPreservesUndecodedSourceSegments()
    {
        var decoded = new SequentialDecodedBlock(0, new byte[] { 1, 2, 3 }, [0], true);
        var unknown = new SequentialMediaSegment(12, SequentialSegmentKind.Unknown, 7);
        var result = new SequentialDecodeResult("test.decoder", 0.75, [decoded], [unknown], ["partial"]);

        Assert.Same(unknown, Assert.Single(result.UndecodedSegments));
        Assert.Same(decoded, Assert.Single(result.Blocks));
        Assert.Equal("partial", Assert.Single(result.Diagnostics));
    }

    private static byte[] CreateStereoWav()
    {
        const ushort channels = 2;
        const uint sampleRate = 8_000;
        const ushort bitsPerSample = 16;
        const ushort blockAlign = channels * (bitsPerSample / 8);
        var pcm = new byte[4 * blockAlign];
        var chunksLength = (8 + 16) + (8 + 4) + (8 + pcm.Length);
        var result = new byte[12 + chunksLength];
        Encoding.ASCII.GetBytes("RIFF").CopyTo(result, 0);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(4), checked((uint)(result.Length - 8)));
        Encoding.ASCII.GetBytes("WAVE").CopyTo(result, 8);
        var offset = 12;
        WriteChunkHeader(result, ref offset, "fmt ", 16);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(offset), 1);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(offset + 2), channels);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(offset + 4), sampleRate);
        BinaryPrimitives.WriteUInt32LittleEndian(result.AsSpan(offset + 8), sampleRate * blockAlign);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(offset + 12), blockAlign);
        BinaryPrimitives.WriteUInt16LittleEndian(result.AsSpan(offset + 14), bitsPerSample);
        offset += 16;
        WriteChunkHeader(result, ref offset, "JUNK", 4);
        result.AsSpan(offset, 4).Fill(0x7f);
        offset += 4;
        WriteChunkHeader(result, ref offset, "data", pcm.Length);
        pcm.CopyTo(result, offset);
        return result;
    }

    private static void WriteChunkHeader(byte[] destination, ref int offset, string id, int length)
    {
        Encoding.ASCII.GetBytes(id).CopyTo(destination, offset);
        BinaryPrimitives.WriteUInt32LittleEndian(destination.AsSpan(offset + 4), checked((uint)length));
        offset += 8;
    }
}
