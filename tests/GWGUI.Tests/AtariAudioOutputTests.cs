using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using GWGUI.Emulation.Atari.Cores;

namespace GWGUI.Tests;

public sealed class AtariAudioOutputTests
{
    public static TheoryData<string, double> FamilyCadences => new()
    {
        { AtariAudioOutputTestConstants.PalFamily, AtariAudioOutputTestConstants.PalFramesPerSecond },
        { AtariAudioOutputTestConstants.NtscFamily, AtariAudioOutputTestConstants.NtscFramesPerSecond },
        { AtariAudioOutputTestConstants.LynxFamily, AtariAudioOutputTestConstants.LynxFramesPerSecond },
        { AtariAudioOutputTestConstants.JaguarFamily, AtariAudioOutputTestConstants.JaguarFramesPerSecond }
    };

    public static TheoryData<string, int> CoreAudioRates => new()
    {
        { AtariAudioOutputTestConstants.HatariCore, AtariAudioOutputTestConstants.StandardSampleRate },
        { AtariAudioOutputTestConstants.Atari800Core, AtariAudioOutputTestConstants.StandardSampleRate },
        { AtariAudioOutputTestConstants.StellaCore, AtariAudioOutputTestConstants.Atari2600SampleRate },
        { AtariAudioOutputTestConstants.ProSystemCore, AtariAudioOutputTestConstants.HighSampleRate },
        { AtariAudioOutputTestConstants.HandyCore, AtariAudioOutputTestConstants.StandardSampleRate },
        { AtariAudioOutputTestConstants.VirtualJaguarCore, AtariAudioOutputTestConstants.HighSampleRate }
    };

    [Fact]
    public void Controller_StartsWithRealRateAndRestartsWhenRateChanges()
    {
        var output = new RecordingOutput();
        using var controller = new AtariAudioOutputController(output);
        controller.Start(AtariAudioOutputTestConstants.FirstSampleRate);

        controller.Write(Chunk(AtariAudioOutputTestConstants.SecondSampleRate));

        Assert.Equal(new[]
        {
            AtariAudioOutputTestConstants.FirstSampleRate,
            AtariAudioOutputTestConstants.SecondSampleRate
        }, output.StartRates);
        Assert.Equal(AtariAudioOutputTestConstants.SingleStop, output.StopCount);
    }

    [Fact]
    public void Controller_AppliesVolumeAndMuteWithoutPublishingSamplesWhileMuted()
    {
        var output = new RecordingOutput();
        using var controller = new AtariAudioOutputController(output);
        controller.Start(AtariAudioOutputTestConstants.FirstSampleRate);
        controller.SetVolume(AtariAudioOutputTestConstants.HalfVolume);
        controller.Write(Chunk(AtariAudioOutputTestConstants.FirstSampleRate));
        controller.SetMuted(true);
        controller.Write(Chunk(AtariAudioOutputTestConstants.FirstSampleRate));
        controller.SetMuted(false);
        controller.Write(Chunk(AtariAudioOutputTestConstants.FirstSampleRate));

        Assert.Equal(AtariAudioOutputTestConstants.ExpectedAudibleWriteCount, output.Writes.Count);
        Assert.All(output.Writes, samples => Assert.Equal(
            new short[] { AtariAudioOutputTestConstants.HalfLeft, AtariAudioOutputTestConstants.HalfRight }, samples));
        Assert.Equal(AtariAudioOutputTestConstants.SingleFlush, output.FlushCount);
    }

    [Fact]
    public void Controller_PauseResumeResetAndStopControlOutput()
    {
        var output = new RecordingOutput();
        using var controller = new AtariAudioOutputController(output);
        controller.Start(AtariAudioOutputTestConstants.FirstSampleRate);
        controller.Pause();
        controller.Write(Chunk(AtariAudioOutputTestConstants.FirstSampleRate));
        controller.Resume();
        controller.Reset();
        controller.Write(Chunk(AtariAudioOutputTestConstants.FirstSampleRate));
        controller.Stop();

        Assert.Equal(AtariAudioOutputTestConstants.ExpectedPauseResetFlushCount, output.FlushCount);
        Assert.Equal(AtariAudioOutputTestConstants.ExpectedStartCountAfterResume, output.StartRates.Count);
        Assert.Single(output.Writes);
        Assert.Equal(AtariAudioOutputTestConstants.ExpectedPauseAndFinalStopCount, output.StopCount);
    }

    [Fact]
    public void Controller_RecreatesOutputAfterWriteFailureAndRetriesChunk()
    {
        var failing = new RecordingOutput { RemainingWriteFailures = AtariAudioOutputTestConstants.SingleFailure };
        var replacement = new RecordingOutput();
        var outputs = new Queue<RecordingOutput>([failing, replacement]);
        using var controller = new AtariAudioOutputController(factory: () => outputs.Dequeue());
        controller.Start(AtariAudioOutputTestConstants.FirstSampleRate);

        controller.Write(Chunk(AtariAudioOutputTestConstants.FirstSampleRate));

        Assert.True(failing.Disposed);
        Assert.Single(replacement.Writes);
        Assert.Equal(AtariAudioOutputTestConstants.FirstSampleRate, Assert.Single(replacement.StartRates));
    }

    [Fact]
    public void Controller_ReplacesOutputWhenDeviceFactoryChanges()
    {
        var first = new RecordingOutput();
        var second = new RecordingOutput();
        using var controller = new AtariAudioOutputController(factory: () => first);
        controller.Start(AtariAudioOutputTestConstants.FirstSampleRate);

        controller.ReplaceFactory(() => second);
        controller.Write(Chunk(AtariAudioOutputTestConstants.FirstSampleRate));

        Assert.True(first.Disposed);
        Assert.Single(second.Writes);
    }

    [Theory]
    [MemberData(nameof(FamilyCadences))]
    public void FrameCadence_UsesExactCoreRateWithinNamedBounds(string _, double framesPerSecond)
    {
        var next = AtariMachineFunctions.NextFrameTimestamp(AtariAudioOutputTestConstants.InitialTimestamp,
            framesPerSecond);
        var expected = (long)(System.Diagnostics.Stopwatch.Frequency / framesPerSecond);

        Assert.Equal(expected, next - AtariAudioOutputTestConstants.InitialTimestamp);
    }

    [Fact]
    public void FrameWait_IsCancellationAware()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var target = System.Diagnostics.Stopwatch.GetTimestamp() + System.Diagnostics.Stopwatch.Frequency;
        var started = System.Diagnostics.Stopwatch.GetTimestamp();

        AtariMachineFunctions.WaitForFrame(target, cancellation.Token);

        var elapsed = System.Diagnostics.Stopwatch.GetElapsedTime(started);
        Assert.True(elapsed < TimeSpan.FromMilliseconds(AtariAudioOutputTestConstants.CancellationLimitMilliseconds));
    }

    [Theory]
    [MemberData(nameof(CoreAudioRates))]
    public void LongRunningCoreAudio_RemainsBoundedAndCountsPressure(string coreName, int sampleRate)
    {
        var buffer = new AtariAudioBuffer();
        var samples = new short[AtariAudioOutputTestConstants.BlockFrameCount *
                                AtariAudioOutputTestConstants.StereoChannelCount];
        var blockCount = sampleRate * AtariAudioOutputTestConstants.TestDurationSeconds /
                         AtariAudioOutputTestConstants.BlockFrameCount;
        for (var index = AtariAudioOutputTestConstants.FirstBlockIndex; index < blockCount; index++)
            buffer.Enqueue(new AudioChunk(samples, sampleRate, AtariAudioOutputTestConstants.BlockFrameCount,
                index, TimeSpan.Zero));

        Assert.InRange(buffer.BufferedFrames, AtariAudioOutputTestConstants.MinimumBufferedFrames,
            AtariAudioFunctions.MaximumBufferedFrames(sampleRate));
        Assert.False(string.IsNullOrWhiteSpace(coreName));
        Assert.True(buffer.OverrunCount > AtariAudioOutputTestConstants.NoOverrun);
        while (buffer.TryDequeue(out _)) { }
        Assert.True(buffer.UnderrunCount > AtariAudioOutputTestConstants.NoUnderrun);
    }

    private static AudioChunk Chunk(int sampleRate) => new(
        new short[] { AtariAudioOutputTestConstants.FullLeft, AtariAudioOutputTestConstants.FullRight }, sampleRate,
        AtariAudioOutputTestConstants.SingleFrameCount, AtariAudioOutputTestConstants.FirstSequence, TimeSpan.Zero);

    private sealed class RecordingOutput : IAudioOutput
    {
        internal List<int> StartRates { get; } = [];
        internal List<short[]> Writes { get; } = [];
        internal int StopCount { get; private set; }
        internal int FlushCount { get; private set; }
        internal int RemainingWriteFailures { get; set; }
        internal bool Disposed { get; private set; }

        public void Start(int sampleRate) => StartRates.Add(sampleRate);
        public void Write(ReadOnlySpan<short> interleavedStereo)
        {
            if (RemainingWriteFailures > AtariAudioOutputTestConstants.NoFailure)
            {
                RemainingWriteFailures--;
                throw new InvalidOperationException(nameof(RemainingWriteFailures));
            }
            Writes.Add(interleavedStereo.ToArray());
        }
        public void Flush() => FlushCount++;
        public void Stop() => StopCount++;
        public void Dispose() => Disposed = true;
    }
}

internal static class AtariAudioOutputTestConstants
{
    internal const int FirstSampleRate = 44100;
    internal const int SecondSampleRate = 48000;
    internal const short FullLeft = 1000;
    internal const short FullRight = -1000;
    internal const short HalfLeft = 500;
    internal const short HalfRight = -500;
    internal const float HalfVolume = 0.5f;
    internal const int SingleFrameCount = 1;
    internal const long FirstSequence = 1;
    internal const int NoFailure = 0;
    internal const long NoOverrun = 0;
    internal const long NoUnderrun = 0;
    internal const int SingleFailure = 1;
    internal const int SingleStop = 1;
    internal const int SingleFlush = 1;
    internal const int ExpectedAudibleWriteCount = 2;
    internal const int ExpectedPauseResetFlushCount = 2;
    internal const int ExpectedStartCountAfterResume = 2;
    internal const int ExpectedPauseAndFinalStopCount = 2;
    internal const long InitialTimestamp = 0;
    internal const int CancellationLimitMilliseconds = 100;
    internal const double PalFramesPerSecond = 50.0;
    internal const double NtscFramesPerSecond = 59.94;
    internal const double LynxFramesPerSecond = 60.0;
    internal const double JaguarFramesPerSecond = 60.0;
    internal const int StandardSampleRate = 44100;
    internal const int Atari2600SampleRate = 31440;
    internal const int HighSampleRate = 48000;
    internal const int TestDurationSeconds = 5;
    internal const int BlockFrameCount = 512;
    internal const int StereoChannelCount = 2;
    internal const int FirstBlockIndex = 0;
    internal const int MinimumBufferedFrames = 1;
    internal const string PalFamily = "PAL";
    internal const string NtscFamily = "NTSC";
    internal const string LynxFamily = "Lynx";
    internal const string JaguarFamily = "Jaguar";
    internal const string HatariCore = "Hatari";
    internal const string Atari800Core = "Atari800";
    internal const string StellaCore = "Stella";
    internal const string ProSystemCore = "ProSystem";
    internal const string HandyCore = "Handy";
    internal const string VirtualJaguarCore = "Virtual Jaguar";
}
