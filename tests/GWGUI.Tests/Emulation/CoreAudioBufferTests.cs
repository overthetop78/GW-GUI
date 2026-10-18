using System.Collections.Concurrent;
using GWGUI.Emulation.Contracts;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Services;

namespace GWGUI.Tests.Emulation;

public sealed class CoreAudioBufferTests
{
    [Theory]
    [InlineData(int.MinValue, 1)]
    [InlineData(-1, 1)]
    [InlineData(0, 1)]
    [InlineData(4, 1)]
    [InlineData(5, 1)]
    [InlineData(9, 1)]
    [InlineData(10, 2)]
    [InlineData(int.MaxValue, 429496729)]
    public void CapacityKeepsTheOriginalMinimumAndIntegerDivision(int rate, int expected)
        => Assert.Equal(expected, EmulationAudioFunctions.MaximumBufferedFrames(rate));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UntrimmedChunksKeepTheirIdentityAndMetadata(bool native)
    {
        var buffer = new EmulationAudioBuffer();
        var chunk = Chunk(2);
        if (native) buffer.EnqueueNative(chunk, 20, static rate => rate);
        else buffer.Enqueue(chunk);
        Assert.Equal(2, buffer.BufferedFrames);
        Assert.True(buffer.TryDequeue(out var actual));
        Assert.Same(chunk, actual);
        Assert.Equal(0, buffer.BufferedFrames);
        Assert.Equal(0, buffer.OverrunCount);
        Assert.Equal(0, buffer.UnderrunCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void StereoTrimmingKeepsTheNewestSamplesAndMetadata(bool native)
    {
        var buffer = new EmulationAudioBuffer();
        var chunk = Chunk(6);
        if (native) buffer.EnqueueNative(chunk, 20, static rate => rate);
        else buffer.Enqueue(chunk);
        Assert.Equal(4, buffer.BufferedFrames);
        Assert.Equal(1, buffer.OverrunCount);
        Assert.True(buffer.TryDequeueNative(out var actual));
        Assert.NotSame(chunk, actual);
        Assert.Equal(Enumerable.Range(4, 8).Select(value => (short)value), actual!.InterleavedSamples.ToArray());
        Assert.Equal(4, actual.FrameCount);
        Assert.Equal(chunk.Sequence, actual.Sequence);
        Assert.Equal(chunk.Timestamp, actual.Timestamp);
        Assert.Equal(chunk.SampleRate, actual.SampleRate);
        Assert.Equal(EmulationAudioChannelLayout.Stereo, actual.ChannelLayout);
    }

    [Theory]
    [InlineData(EmulationAudioChannelLayout.Surround51, 6)]
    [InlineData(EmulationAudioChannelLayout.Surround71, 8)]
    public void SonyNativeTrimmingUsesTheChannelCountAndPreservesTheLayout(EmulationAudioChannelLayout layout, int channels)
    {
        var buffer = new EmulationAudioBuffer();
        var chunk = Chunk(6, layout: layout);
        buffer.EnqueueNative(chunk, 20, static rate => rate, multichannel: true);
        Assert.True(buffer.TryDequeueNative(out var actual));
        Assert.Equal(layout, actual!.ChannelLayout);
        Assert.Equal(4, actual.FrameCount);
        Assert.Equal(Enumerable.Range(2 * channels, 4 * channels).Select(value => (short)value), actual.InterleavedSamples.ToArray());
        Assert.True(actual.HasValidLength);
    }

    [Fact]
    public void OrdinaryNativeTrimmingRetainsItsStereoConstructionPolicy()
    {
        var chunk = Chunk(6, layout: EmulationAudioChannelLayout.Surround51);
        var actual = EmulationAudioFunctions.RetainNativeNewestFrames(chunk, 4, multichannel: false);
        Assert.Equal(EmulationAudioChannelLayout.Stereo, actual.ChannelLayout);
        Assert.Equal(chunk.InterleavedSamples.Slice(4).ToArray(), actual.InterleavedSamples.ToArray());
    }

    [Fact]
    public void NativeCapacityComesFromTheCurrentHostRatherThanTheChunk()
    {
        using var host = new Host { Rate = 30 };
        var buffer = new EmulationAudioBuffer();
        var first = Chunk(5, rate: 10);
        buffer.EnqueueNative(first, host, static owner => owner.ReadRate());
        Assert.Equal(5, buffer.BufferedFrames);
        Assert.Equal(0, buffer.OverrunCount);
        host.Rate = 10;
        buffer.EnqueueNative(Chunk(3, rate: 100), host, static owner => owner.ReadRate());
        Assert.Equal(2, buffer.BufferedFrames);
        Assert.Equal(2, buffer.OverrunCount);
        Assert.Equal(2, host.Reads);
        Assert.False(host.Disposed);
    }

    [Fact]
    public void RichCapacityComesFromEachChunksSampleRate()
    {
        var buffer = new EmulationAudioBuffer();
        buffer.Enqueue(Chunk(5, rate: 30));
        Assert.Equal(5, buffer.BufferedFrames);
        buffer.Enqueue(Chunk(3, rate: 10));
        Assert.Equal(2, buffer.BufferedFrames);
        Assert.Equal(2, buffer.OverrunCount);
    }

    [Fact]
    public void TrimmingAndEvictionHaveSeparateOverrunCountsAndKeepQueueOrder()
    {
        var buffer = new EmulationAudioBuffer();
        buffer.Enqueue(Chunk(6, sequence: 1));
        buffer.Enqueue(Chunk(2, sequence: 2));
        buffer.Enqueue(Chunk(2, sequence: 3));
        buffer.Enqueue(Chunk(1, sequence: 4));
        Assert.Equal(3, buffer.OverrunCount);
        Assert.Equal(3, buffer.BufferedFrames);
        Assert.True(buffer.TryDequeue(out var third));
        Assert.Equal(3, third!.Sequence);
        Assert.Equal(1, buffer.BufferedFrames);
        Assert.True(buffer.TryDequeue(out var fourth));
        Assert.Equal(4, fourth!.Sequence);
        Assert.Equal(0, buffer.BufferedFrames);
    }

    [Fact]
    public void OnlyRichEmptyDequeuesIncrementTheUnderrunCounter()
    {
        var buffer = new EmulationAudioBuffer();
        Assert.False(buffer.TryDequeueNative(out var native));
        Assert.Null(native);
        Assert.Equal(0, buffer.UnderrunCount);
        Assert.False(buffer.TryDequeue(out var rich));
        Assert.Null(rich);
        Assert.Equal(1, buffer.UnderrunCount);
        Assert.False(buffer.TryDequeueNative(out _));
        Assert.Equal(1, buffer.UnderrunCount);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TrimmingFailureLeavesTheExistingQueueAndCountersUntouched(bool native)
    {
        var buffer = new EmulationAudioBuffer();
        var first = Chunk(2);
        buffer.Enqueue(first);
        var invalid = new AudioChunk(Array.Empty<short>(), 20, 6, 2, TimeSpan.Zero);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            if (native) buffer.EnqueueNative(invalid, 20, static rate => rate);
            else buffer.Enqueue(invalid);
        });
        Assert.Equal(2, buffer.BufferedFrames);
        Assert.Equal(0, buffer.OverrunCount);
        Assert.True(buffer.TryDequeueNative(out var actual));
        Assert.Same(first, actual);
    }

    [Fact]
    public void FailedHostReadPropagatesBeforeChangingTheQueue()
    {
        var buffer = new EmulationAudioBuffer();
        var first = Chunk(2);
        buffer.Enqueue(first);
        var error = new InvalidOperationException("host read");
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => buffer.EnqueueNative(Chunk(3), error, static failure => throw failure)));
        Assert.Equal(2, buffer.BufferedFrames);
        Assert.Equal(0, buffer.OverrunCount);
        Assert.True(buffer.TryDequeueNative(out var actual));
        Assert.Same(first, actual);
    }

    [Fact]
    public void NativeClearRunsAfterQueueResetAndKeepsTheCountersEvenIfItsCallbackFails()
    {
        var buffer = new EmulationAudioBuffer();
        Assert.False(buffer.TryDequeue(out _));
        buffer.Enqueue(Chunk(6));
        var error = new InvalidOperationException("clear latest");
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() => buffer.ClearNative(() =>
        {
            Assert.Equal(0, buffer.BufferedFrames);
            throw error;
        })));
        Assert.Equal(1, buffer.OverrunCount);
        Assert.Equal(1, buffer.NativeOverrunCount);
        Assert.Equal(1, buffer.UnderrunCount);
        Assert.False(buffer.TryDequeueNative(out _));
    }

    [Fact]
    public void HostReadHoldsTheQueueLockWhileTheNativeCounterGetterRemainsUnlocked()
    {
        var buffer = new EmulationAudioBuffer();
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        using var counterRead = new ManualResetEventSlim();
        using var framesRead = new ManualResetEventSlim();
        var failures = new ConcurrentQueue<Exception>();
        var writer = new Thread(() =>
        {
            try
            {
                buffer.EnqueueNative(Chunk(2), (entered, release), static signals =>
                {
                    signals.entered.Set();
                    signals.release.Wait();
                    return 20;
                });
            }
            catch (Exception error) { failures.Enqueue(error); }
        });
        var reader = new Thread(() =>
        {
            try
            {
                Assert.Equal(0, buffer.NativeOverrunCount);
                counterRead.Set();
                _ = buffer.BufferedFrames;
                framesRead.Set();
            }
            catch (Exception error) { failures.Enqueue(error); }
        });
        var writerStarted = false;
        var readerStarted = false;
        try
        {
            writer.Start(); writerStarted = true;
            Assert.True(entered.Wait(TimeSpan.FromSeconds(5)));
            reader.Start(); readerStarted = true;
            Assert.True(counterRead.Wait(TimeSpan.FromSeconds(5)));
            Assert.False(framesRead.Wait(TimeSpan.FromMilliseconds(100)));
        }
        finally
        {
            release.Set();
            if (writerStarted) writer.Join();
            if (readerStarted) reader.Join();
        }
        Assert.True(framesRead.IsSet);
        Assert.Empty(failures);
        Assert.Equal(2, buffer.BufferedFrames);
    }

    private static AudioChunk Chunk(int frames, int rate = 20, long sequence = 7, EmulationAudioChannelLayout layout = EmulationAudioChannelLayout.Stereo)
    {
        var channels = System.Numerics.BitOperations.PopCount((uint)layout);
        return new AudioChunk(Enumerable.Range(0, frames * channels).Select(value => (short)value).ToArray(), rate, frames, sequence, TimeSpan.FromSeconds(3), layout);
    }

    private sealed class Host : IDisposable
    {
        internal int Rate;
        internal int Reads;
        internal bool Disposed;
        internal int ReadRate() { Reads++; return Rate; }
        public void Dispose() => Disposed = true;
    }
}
