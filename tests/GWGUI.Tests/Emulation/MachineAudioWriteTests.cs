using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Functions;
using GWGUI.Emulation.Interfaces;

namespace GWGUI.Tests.Emulation;

public sealed class MachineAudioWriteTests
{
    [Fact]
    public void MissingOutputLeavesBorrowedValuesUntouched()
    {
        IAudioOutput? output = null;
        var volume = 0.5f;
        EmulationAudioFunctions.WriteVolume(ref output, ref volume, new short[] { 3 });
        Assert.Null(output);
        Assert.Equal(0.5f, volume);
    }

    [Theory]
    [InlineData(1f)]
    [InlineData(1.5f)]
    public void FullVolumeWritesTheOriginalSpan(float volume)
    {
        short[] source = [-3, -1, 1, 3];
        using var owned = new Output { ExpectedSource = source };
        IAudioOutput? output = owned;
        EmulationAudioFunctions.WriteVolume(ref output, ref volume, source);
        Assert.True(owned.UsedExpectedSource);
        Assert.Equal(source, Assert.Single(owned.Writes));
        Assert.Same(owned, output);
        Assert.False(owned.Disposed);
    }

    [Fact]
    public void FractionalSamplesAreTruncatedAndSourceRemainsUnchanged()
    {
        short[] source = [-3, -1, 1, 3, short.MaxValue, short.MinValue];
        using var owned = new Output { ExpectedSource = source };
        IAudioOutput? output = owned;
        var volume = 0.5f;
        EmulationAudioFunctions.WriteVolume(ref output, ref volume, source);
        Assert.Equal(new short[] { -1, 0, 0, 1, 16383, -16384 }, Assert.Single(owned.Writes));
        Assert.Equal(new short[] { -3, -1, 1, 3, short.MaxValue, short.MinValue }, source);
        Assert.False(owned.UsedExpectedSource);
        Assert.False(owned.Disposed);
        Assert.Same(owned, output);
        Assert.Equal(0.5f, volume);
    }

    [Fact]
    public void ZeroVolumeWritesSilenceOnce()
    {
        using var owned = new Output();
        IAudioOutput? output = owned;
        var volume = 0f;
        EmulationAudioFunctions.WriteVolume(ref output, ref volume, new short[] { short.MinValue, 3, short.MaxValue });
        Assert.Equal(new short[] { 0, 0, 0 }, Assert.Single(owned.Writes));
        Assert.False(owned.Disposed);
    }

    [Fact]
    public void EmptyScaledInputStillReachesTheBorrowedOutput()
    {
        using var owned = new Output();
        IAudioOutput? output = owned;
        var volume = 0.5f;
        EmulationAudioFunctions.WriteVolume(ref output, ref volume, ReadOnlySpan<short>.Empty);
        Assert.Empty(Assert.Single(owned.Writes));
        Assert.False(owned.Disposed);
    }

    [Fact]
    public void WriteFailurePreservesTheExceptionAndBorrowedOutput()
    {
        var error = new InvalidOperationException("output failure");
        using var owned = new Output { Error = error };
        IAudioOutput? output = owned;
        var volume = 0.5f;
        var actual = Assert.Throws<InvalidOperationException>(() =>
            EmulationAudioFunctions.WriteVolume(ref output, ref volume, new short[] { 3 }));
        Assert.Same(error, actual);
        Assert.Same(owned, output);
        Assert.False(owned.Disposed);
        Assert.Equal(1, owned.WriteCount);
        Assert.Equal(0.5f, volume);
    }

    private sealed class Output : IAudioOutput
    {
        internal short[]? ExpectedSource { get; init; }
        internal Exception? Error { get; init; }
        internal List<short[]> Writes { get; } = [];
        internal bool UsedExpectedSource { get; private set; }
        internal bool Disposed { get; private set; }
        internal int WriteCount { get; private set; }
        public void Start(int sampleRate) { }
        public void Write(ReadOnlySpan<short> samples)
        {
            WriteCount++;
            if (Error is not null) throw Error;
            if (ExpectedSource is { Length: > 0 } source)
                UsedExpectedSource = Unsafe.AreSame(ref MemoryMarshal.GetReference(source.AsSpan()),
                    ref MemoryMarshal.GetReference(samples));
            Writes.Add(samples.ToArray());
        }
        public void Flush() { }
        public void Stop() { }
        public void Dispose() => Disposed = true;
    }
}
