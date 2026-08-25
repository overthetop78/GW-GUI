using GWGUI.Emulation;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

public sealed class AtariAudioCallbackTests
{
    [Fact]
    public void BatchAndSingleCallbacks_PreserveStereoChannelsAndOrder()
    {
        using var fixture = CreateCallbacks();
        SetSampleRate(fixture.Callbacks, AtariAudioTestConstants.StandardSampleRate);
        var samples = new short[]
        {
            AtariAudioTestConstants.FirstLeft, AtariAudioTestConstants.FirstRight,
            AtariAudioTestConstants.SecondLeft, AtariAudioTestConstants.SecondRight
        };
        var pointer = Marshal.AllocHGlobal(samples.Length * sizeof(short));
        try
        {
            Marshal.Copy(samples, AtariAudioTestConstants.FirstIndex, pointer, samples.Length);
            Assert.Equal((nuint)AtariAudioTestConstants.BatchFrameCount,
                fixture.Callbacks.AudioBatch(pointer, AtariAudioTestConstants.BatchFrameCount));
            fixture.Callbacks.AudioSample(AtariAudioTestConstants.SingleLeft, AtariAudioTestConstants.SingleRight);

            Assert.True(fixture.Callbacks.TryDequeueAudio(out var batch));
            Assert.Equal(samples, batch!.InterleavedStereo.ToArray());
            Assert.Equal(AtariAudioTestConstants.BatchFrameCount, batch.FrameCount);
            Assert.True(fixture.Callbacks.TryDequeueAudio(out var single));
            Assert.Equal(new short[] { AtariAudioTestConstants.SingleLeft, AtariAudioTestConstants.SingleRight },
                single!.InterleavedStereo.ToArray());
            Assert.True(single.Sequence > batch.Sequence);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Fact]
    public void Queue_DropsOldestChunksAtRateBoundAndCountsOverruns()
    {
        using var fixture = CreateCallbacks();
        SetSampleRate(fixture.Callbacks, AtariAudioTestConstants.BoundedSampleRate);
        for (var index = AtariAudioTestConstants.FirstIndex;
             index < AtariAudioTestConstants.BatchCount; index++)
            fixture.Callbacks.AudioSample((short)index, (short)-index);

        Assert.Equal(AtariAudioTestConstants.MaximumBufferedFrames, fixture.Callbacks.BufferedAudioFrames);
        Assert.Equal(AtariAudioTestConstants.ExpectedOverruns, fixture.Callbacks.AudioOverrunCount);
        Assert.True(fixture.Callbacks.TryDequeueAudio(out var oldestRetained));
        Assert.Equal(AtariAudioTestConstants.FirstRetainedSample,
            oldestRetained!.InterleavedStereo.Span[AtariAudioTestConstants.LeftChannelIndex]);
    }

    [Fact]
    public void OversizedBatch_RetainsNewestFramesAndCountsOverrun()
    {
        var buffer = new AtariAudioBuffer();
        var samples = Enumerable.Range(AtariAudioTestConstants.FirstIndex,
                AtariAudioTestConstants.OversizedSampleCount)
            .Select(value => (short)value).ToArray();
        buffer.Enqueue(new AudioChunk(samples, AtariAudioTestConstants.BoundedSampleRate,
            AtariAudioTestConstants.OversizedFrameCount, AtariAudioTestConstants.FirstSequence, TimeSpan.Zero));

        Assert.Equal(AtariAudioTestConstants.MaximumBufferedFrames, buffer.BufferedFrames);
        Assert.Equal(AtariAudioTestConstants.SingleOverrun, buffer.OverrunCount);
        Assert.True(buffer.TryDequeue(out var retained));
        Assert.Equal(AtariAudioTestConstants.MaximumBufferedFrames, retained!.FrameCount);
        Assert.Equal(samples[^AtariAudioTestConstants.MaximumBufferedSampleCount..],
            retained.InterleavedStereo.ToArray());
    }

    [Fact]
    public void EmptyRead_CountsUnderrunWithoutCreatingChunk()
    {
        var buffer = new AtariAudioBuffer();

        Assert.False(buffer.TryDequeue(out var chunk));
        Assert.Null(chunk);
        Assert.Equal(AtariAudioTestConstants.SingleUnderrun, buffer.UnderrunCount);
    }

    [Fact]
    public void Machines_HaveIsolatedBuffersAndRateChangesUseNewBound()
    {
        using var first = CreateCallbacks();
        using var second = CreateCallbacks();
        SetSampleRate(first.Callbacks, AtariAudioTestConstants.StandardSampleRate);
        SetSampleRate(second.Callbacks, AtariAudioTestConstants.BoundedSampleRate);
        first.Callbacks.AudioSample(AtariAudioTestConstants.FirstLeft, AtariAudioTestConstants.FirstRight);

        Assert.False(second.Callbacks.TryDequeueAudio(out var secondChunk));
        Assert.Null(secondChunk);
        Assert.Equal(AtariAudioTestConstants.SingleUnderrun, second.Callbacks.AudioUnderrunCount);
        Assert.Equal(AtariAudioTestConstants.NoUnderrun, first.Callbacks.AudioUnderrunCount);

        SetSampleRate(first.Callbacks, AtariAudioTestConstants.BoundedSampleRate);
        for (var index = AtariAudioTestConstants.FirstIndex;
             index < AtariAudioTestConstants.BatchCount; index++)
            first.Callbacks.AudioSample((short)index, (short)-index);
        Assert.InRange(first.Callbacks.BufferedAudioFrames, AtariAudioTestConstants.MinimumBufferedFrames,
            AtariAudioTestConstants.MaximumBufferedFrames);
    }

    private static void SetSampleRate(AtariExternalHostCallbacks callbacks, int sampleRate) =>
        callbacks.ApplySystemAvInfo(new ExternalCoreApi.SystemAvInfo
        {
            Timing = new ExternalCoreApi.Timing { SampleRate = sampleRate }
        });

    private static CallbackFixture CreateCallbacks()
    {
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-atari-audio-{Guid.NewGuid():N}");
        return new CallbackFixture(root, new AtariExternalHostCallbacks(Path.Combine(root, "system"),
            Path.Combine(root, "content"), Path.Combine(root, "saves"), Path.Combine(root, "assets"),
            new Dictionary<string, string>()));
    }

    private sealed record CallbackFixture(string Root, AtariExternalHostCallbacks Callbacks) : IDisposable
    {
        public void Dispose()
        {
            Callbacks.Dispose();
            if (Directory.Exists(Root)) Directory.Delete(Root, recursive: true);
        }
    }
}

internal static class AtariAudioTestConstants
{
    internal const int FirstIndex = 0;
    internal const int LeftChannelIndex = 0;
    internal const int StandardSampleRate = 48000;
    internal const int BoundedSampleRate = 10;
    internal const int MaximumBufferedFrames = 2;
    internal const int MinimumBufferedFrames = 1;
    internal const int BatchFrameCount = 2;
    internal const int BatchCount = 5;
    internal const int ExpectedOverruns = BatchCount - MaximumBufferedFrames;
    internal const short FirstRetainedSample = 3;
    internal const short FirstLeft = 101;
    internal const short FirstRight = -102;
    internal const short SecondLeft = 201;
    internal const short SecondRight = -202;
    internal const short SingleLeft = 301;
    internal const short SingleRight = -302;
    internal const int OversizedFrameCount = 4;
    internal const int StereoChannelCount = 2;
    internal const int OversizedSampleCount = OversizedFrameCount * StereoChannelCount;
    internal const int MaximumBufferedSampleCount = MaximumBufferedFrames * StereoChannelCount;
    internal const long FirstSequence = 1;
    internal const long SingleOverrun = 1;
    internal const long SingleUnderrun = 1;
    internal const long NoUnderrun = 0;
}
