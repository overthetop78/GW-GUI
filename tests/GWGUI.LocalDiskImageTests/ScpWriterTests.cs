using System.Buffers.Binary;
using System.IO;
using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.Tests;

public sealed class ScpWriterTests
{
    [Fact]
    public async Task WritesEveryScpSectionAndRoundTripsThroughReader()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-scp-writer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "round-trip.scp");
        await File.WriteAllBytesAsync(path, [0xff]);
        try
        {
            var header = new ScpHeader(0x19, 0x80, 2, 0, 3, ScpFlags.IndexAligned | ScpFlags.Normalized | ScpFlags.Writable | ScpFlags.ThirdPartyCreator, ScpBitCellEncoding.Explicit16Bit, ScpHeadSelection.Both, 1, uint.MaxValue);
            var tracks = new[]
            {
                Track(0, [Revolution(4_000_000, 10, 70_000, 65_536, 20), Revolution(4_000_001, 131_073, 42)]),
                Track(3, [Revolution(4_000_002, 100, 200), Revolution(4_000_003, 300, 400)])
            };
            var source = new ScpImage(header, tracks, false, 0);

            await new ScpWriter().WriteAsync(path, source);
            var bytes = await File.ReadAllBytesAsync(path);
            var loaded = await new ScpReader().ReadAsync(path);

            Assert.True(loaded.ChecksumValid);
            Assert.Equal(header with { Checksum = loaded.Header.Checksum }, loaded.Header);
            Assert.NotEqual(uint.MaxValue, loaded.Header.Checksum);
            Assert.Equal([0, 3], loaded.Tracks.Select(track => (int)track.TrackNumber));
            Assert.Equal([10u, 70_000u, 65_535u, 21u], loaded.Tracks[0].Revolutions[0].FluxIntervals);
            Assert.Equal(135_566u, loaded.Tracks[0].Revolutions[0].FluxIntervals.Aggregate(0u, (sum, value) => sum + value));
            Assert.Equal([131_073u, 42u], loaded.Tracks[0].Revolutions[1].FluxIntervals);
            Assert.Equal(5u, loaded.Tracks[0].Revolutions[0].DeclaredFluxCount);
            Assert.Equal(4u, loaded.Tracks[0].Revolutions[1].DeclaredFluxCount);
            Assert.All(loaded.Tracks.SelectMany(track => track.Revolutions), revolution => Assert.Equal(ScpRevolutionOrigin.Captured, revolution.Origin));
            Assert.NotEqual(ScpFormatConstants.MissingTrackOffset, TrackOffset(bytes, 0));
            Assert.NotEqual(ScpFormatConstants.MissingTrackOffset, TrackOffset(bytes, 3));
            Assert.All(Enumerable.Range(0, ScpFormatConstants.FloppyTrackSlots).Except([0, 3]), slot => Assert.Equal(ScpFormatConstants.MissingTrackOffset, TrackOffset(bytes, slot)));
            Assert.Empty(Directory.EnumerateFiles(directory, ".*.tmp"));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public void ConvertsEncodedTrackAtAbsoluteTimesWithoutCumulativeDrift()
    {
        var sourceIntervals = Enumerable.Repeat(41u, 1_001).ToArray();
        var encoded = new EncodedTrack("test", [true], new FluxRevolution(8_000_000, sourceIntervals));

        var native = new ScpEncodedTrackFluxService().Create(encoded, 0);
        var revolution = new ScpEncodedTrackFluxService().Create(encoded, 1);

        Assert.Equal(sourceIntervals, native.FluxIntervals);
        Assert.Equal(8_000_000u, native.IndexTimeTicks);
        Assert.Equal(ScpRevolutionOrigin.Synthetic, revolution.Origin);
        Assert.Equal(4_000_000u, revolution.IndexTimeTicks);
        Assert.Equal(200d, revolution.DurationMilliseconds(50));
        Assert.Equal(300d, revolution.Rpm(50));
        Assert.Equal(20_521u, revolution.FluxIntervals.Aggregate(0u, (sum, value) => sum + value));
        Assert.NotEqual(21_021u, revolution.FluxIntervals.Aggregate(0u, (sum, value) => sum + value));
        Assert.All(revolution.FluxIntervals, interval => Assert.True(interval > 0));
    }

    [Fact]
    public async Task RejectsInconsistentTrackBeforeReplacingDestination()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"gwgui-scp-writer-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "preserved.scp");
        byte[] original = [1, 2, 3];
        await File.WriteAllBytesAsync(path, original);
        try
        {
            var header = new ScpHeader(0x19, 0x80, 1, 0, 0, ScpFlags.None, ScpBitCellEncoding.Default16Bit, ScpHeadSelection.Both, 0, 0);
            var invalid = new ScpImage(header, [new ScpTrack(0, 1, 0, [Revolution(8_000_000, 100)])], false, 0);

            await Assert.ThrowsAsync<InvalidDataException>(() => new ScpWriter().WriteAsync(path, invalid));

            Assert.Equal(original, await File.ReadAllBytesAsync(path));
        }
        finally
        {
            Directory.Delete(directory, true);
        }
    }

    private static ScpTrack Track(byte number, IReadOnlyList<ScpRevolution> revolutions)
    {
        var address = ScpFormatAlgorithms.ToTrackAddress(number);
        return new ScpTrack(number, address.Cylinder, address.Head, revolutions);
    }

    private static ScpRevolution Revolution(uint indexTime, params uint[] intervals) => new(indexTime, 0, intervals);

    private static uint TrackOffset(byte[] bytes, int slot) => BinaryPrimitives.ReadUInt32LittleEndian(bytes.AsSpan(ScpFormatConstants.TrackTableOffset + slot * ScpFormatConstants.TrackTableEntrySize, ScpFormatConstants.TrackTableEntrySize));
}
