using GWGUI.Emulation;
using System.IO;
using System.Text;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class FrameTransmissionTests
{
    [Fact]
    public void RepeatedAndMissingVideoFramesKeepTheirExistingSuppression()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var frame = Frame(7);
        var observed = new List<VideoFrame?>();
        long sequence = 0;
        void Write(BinaryWriter output, VideoFrame? value)
        {
            Assert.Same(writer, output);
            observed.Add(value);
            output.Write(value is not null);
        }
        EmulationHostProtocolFunctions.WriteFrameIfChanged(writer, frame, ref sequence, Write);
        EmulationHostProtocolFunctions.WriteFrameIfChanged(writer, frame, ref sequence, Write);
        EmulationHostProtocolFunctions.WriteFrameIfChanged(writer, null, ref sequence, Write);
        Assert.Equal(7, sequence);
        Assert.Collection(observed,
            value => Assert.Same(frame, value),
            Assert.Null,
            Assert.Null);
        Assert.Equal(new byte[] { 1, 0, 0 }, stream.ToArray());
    }

    [Fact]
    public void FailedVideoWriteKeepsTheSequenceAndBorrowedWriterAlive()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        long sequence = 4;
        var expected = new InvalidOperationException("video writer failed");
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() =>
            EmulationHostProtocolFunctions.WriteFrameIfChanged(writer, Frame(7), ref sequence,
                (_, _) => throw expected)));
        Assert.Equal(4, sequence);
        Assert.True(stream.CanWrite);
        writer.Write(true);
    }

    [Fact]
    public void AudioQueueKeepsPacketOrderAndTheExistingWireCodec()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        var first = new AudioChunk(new short[] { 17 }, 44100, 1, 5, TimeSpan.FromTicks(11), EmulationAudioChannelLayout.Mono);
        var second = new AudioChunk(new short[] { -23, 42 }, 48000, 1, 6, TimeSpan.FromTicks(12));
        var queue = new Queue<AudioChunk?>(new[] { first, second, null });
        var calls = 0;
        EmulationHostProtocolFunctions.WriteQueuedAudio(writer, () => { calls++; return queue.Dequeue(); });
        writer.Flush();
        Assert.Equal(3, calls);
        Assert.Empty(queue);
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        var received = EmulationSdk.HostProtocol.ReadAudio(reader, "test");
        Assert.Equal(2, received.Count);
        foreach (var pair in new[] { (first, received[0]), (second, received[1]) })
        {
            Assert.Equal(pair.Item1.InterleavedSamples.ToArray(), pair.Item2.InterleavedSamples.ToArray());
            Assert.Equal(pair.Item1.SampleRate, pair.Item2.SampleRate);
            Assert.Equal(pair.Item1.FrameCount, pair.Item2.FrameCount);
            Assert.Equal(pair.Item1.Sequence, pair.Item2.Sequence);
            Assert.Equal(pair.Item1.Timestamp, pair.Item2.Timestamp);
            Assert.Equal(pair.Item1.ChannelLayout, pair.Item2.ChannelLayout);
        }
        Assert.Equal(stream.Length, stream.Position);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EmptyOrNullTerminatedAudioWritesOnlyAnEmptyPacket(bool packetAfterNull)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var queue = new Queue<AudioChunk?>();
        if (packetAfterNull)
        {
            queue.Enqueue(null);
            queue.Enqueue(new AudioChunk(new short[] { 1, 2 }, 48000, 1, 1, TimeSpan.Zero));
        }
        var calls = 0;
        EmulationHostProtocolFunctions.WriteQueuedAudio(writer, () =>
        {
            calls++;
            return queue.Count == 0 ? null : queue.Dequeue();
        });
        Assert.Equal(1, calls);
        Assert.Equal(packetAfterNull ? 1 : 0, queue.Count);
        Assert.Equal(new byte[] { 0, 0, 0, 0 }, stream.ToArray());
    }

    [Fact]
    public void AudioReadFailureHappensBeforeWritingAnyPartialPacket()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var expected = new InvalidOperationException("audio source failed");
        var calls = 0;
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() =>
            EmulationHostProtocolFunctions.WriteQueuedAudio(writer, () =>
                ++calls == 1
                    ? new AudioChunk(new short[] { 1, 2 }, 48000, 1, 1, TimeSpan.Zero)
                    : throw expected)));
        Assert.Equal(0, stream.Length);
    }

    [Fact]
    public void UnchangedDiagnosticsSkipSerialization()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var previousCount = 3;
        EmulationHostProtocolFunctions.WriteChangedDiagnostics(writer, () => 3,
            _ => throw new InvalidOperationException("must not serialize"), ref previousCount);
        Assert.Equal(3, previousCount);
        Assert.Equal(new byte[] { 0 }, stream.ToArray());
    }

    [Fact]
    public void ChangedDiagnosticsCommitTheCountAfterSuccessfulSerialization()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        var previousCount = 1;
        var count = 2;
        var reads = 0;
        EmulationHostProtocolFunctions.WriteChangedDiagnostics(writer, () => { reads++; return count; },
            output => { output.Write("changed"); count = 3; }, ref previousCount);
        writer.Flush();
        Assert.Equal(3, previousCount);
        Assert.Equal(2, reads);
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        Assert.True(reader.ReadBoolean());
        Assert.Equal("changed", reader.ReadString());
        Assert.Equal(stream.Length, stream.Position);
    }

    [Fact]
    public void FailedDiagnosticSerializationDoesNotAdvanceTheCounter()
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        var previousCount = 1;
        var expected = new InvalidOperationException("diagnostics failed");
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() =>
            EmulationHostProtocolFunctions.WriteChangedDiagnostics(writer, () => 2,
                _ => throw expected, ref previousCount)));
        Assert.Equal(1, previousCount);
        Assert.Equal(new byte[] { 1 }, stream.ToArray());
    }

    private static VideoFrame Frame(long sequence) =>
        new(new byte[] { 0, 0 }, 1, 1, 2, EmulationPixelFormat.Rgb565, 1f, sequence, TimeSpan.Zero);
}

