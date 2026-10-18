using System.Runtime.InteropServices;
using GWGUI.Emulation.Services;

namespace GWGUI.Tests.Emulation;

public sealed class CoreAudioCallbackHostTests
{
    [Fact]
    public void SampleCallbackKeepsItsOwnerAndPublishesTheOriginalValues()
    {
        using var host = new ExternalCoreAudioCallbackHost { SampleRate = 20 };
        Assert.Same(host, host.SampleCallback.Target);
        Assert.Same(host, host.BatchCallback.Target);
        host.SampleCallback(short.MinValue, short.MaxValue);
        Assert.True(host.TryDequeue(out var chunk));
        Assert.Same(host.LatestChunk, chunk);
        Assert.Equal(new[] { short.MinValue, short.MaxValue }, chunk!.InterleavedSamples.ToArray());
        Assert.Equal(1, chunk.FrameCount);
        Assert.Equal(20, chunk.SampleRate);
        Assert.Equal(1, chunk.Sequence);
        Assert.Equal(TimeSpan.Zero, chunk.Timestamp);
        Assert.Equal(0, host.BufferedFrames);
        Assert.Equal(0, host.OverrunCount);
        Assert.Null(host.Failure);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(65537UL)]
    [InlineData(ulong.MaxValue)]
    public void InvalidBatchCountsReturnBeforeReadingThePointer(ulong frames)
    {
        using var host = new ExternalCoreAudioCallbackHost { SampleRate = 20 };
        Assert.Equal((nuint)0, host.BatchCallback((nint)1, checked((nuint)frames)));
        Assert.Null(host.LatestChunk);
        Assert.Null(host.Failure);
        Assert.Equal(0, host.BufferedFrames);
    }

    [Fact]
    public void NullBatchPointerDoesNotPublishOrSetFailure()
    {
        using var host = new ExternalCoreAudioCallbackHost();
        Assert.Equal((nuint)0, host.BatchCallback(0, 2));
        Assert.False(host.TryDequeue(out var chunk));
        Assert.Null(chunk);
        Assert.Equal(0, host.UnderrunCount);
        Assert.Null(host.Failure);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(65536)]
    public void AcceptedBatchesPublishTheRetainedChunkWithoutCountingTrimmingAsOverrun(int frames)
    {
        using var host = new ExternalCoreAudioCallbackHost { SampleRate = 10 };
        var samples = Enumerable.Range(0, frames * 2).Select(index => unchecked((short)index)).ToArray();
        var pointer = Marshal.AllocHGlobal(samples.Length * sizeof(short));
        try
        {
            Marshal.Copy(samples, 0, pointer, samples.Length);
            Assert.Equal((nuint)frames, host.BatchCallback(pointer, (nuint)frames));
            var retainedFrames = Math.Min(frames, 2);
            Assert.Equal(retainedFrames, host.BufferedFrames);
            Assert.Equal(0, host.OverrunCount);
            Assert.True(host.TryDequeue(out var chunk));
            Assert.Same(host.LatestChunk, chunk);
            Assert.Equal(retainedFrames, chunk!.FrameCount);
            Assert.Equal(samples.AsSpan((frames - retainedFrames) * 2).ToArray(), chunk.InterleavedSamples.ToArray());
            Assert.Equal(1, chunk.Sequence);
            Assert.Null(host.Failure);
        }
        finally { Marshal.FreeHGlobal(pointer); }
    }

    [Fact]
    public void EvictionHappensBeforeEnqueueAndLeavesTheLatestRetainedChunkPublished()
    {
        using var host = new ExternalCoreAudioCallbackHost { SampleRate = 10 };
        host.SampleCallback(1, 2);
        host.SampleCallback(3, 4);
        host.SampleCallback(5, 6);
        Assert.Equal(2, host.BufferedFrames);
        Assert.Equal(1, host.OverrunCount);
        Assert.True(host.TryDequeue(out var second));
        Assert.Equal(2, second!.Sequence);
        Assert.Equal(new short[] { 3, 4 }, second.InterleavedSamples.ToArray());
        Assert.True(host.TryDequeue(out var third));
        Assert.Equal(3, third!.Sequence);
        Assert.Same(host.LatestChunk, third);
        Assert.False(host.TryDequeue(out _));
        Assert.Equal(0, host.UnderrunCount);
    }

    [Fact]
    public void RateChangesApplyToTheNextChunkAndKeepTheSequence()
    {
        using var host = new ExternalCoreAudioCallbackHost { SampleRate = 20 };
        host.SampleCallback(1, 2);
        host.SampleCallback(3, 4);
        host.SampleRate = 0;
        host.SampleCallback(5, 6);
        Assert.Equal(1, host.BufferedFrames);
        Assert.Equal(2, host.OverrunCount);
        Assert.True(host.TryDequeue(out var chunk));
        Assert.Equal(3, chunk!.Sequence);
        Assert.Equal(0, chunk.SampleRate);
    }

    [Fact]
    public void DisposeClearsStateAndCallbacksIgnoreFurtherInput()
    {
        using var host = new ExternalCoreAudioCallbackHost { SampleRate = 5 };
        host.SampleCallback(1, 2);
        host.SampleCallback(3, 4);
        Assert.Equal(1, host.OverrunCount);
        host.Dispose();
        Assert.Equal(0, host.BufferedFrames);
        Assert.Null(host.LatestChunk);
        Assert.Null(host.Failure);
        host.SampleCallback(5, 6);
        Assert.Equal((nuint)0, host.BatchCallback((nint)1, 1));
        Assert.False(host.TryDequeue(out _));
        Assert.Equal(1, host.OverrunCount);
        Assert.Equal(0, host.UnderrunCount);
        Assert.Null(host.LatestChunk);
    }
}
