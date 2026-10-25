using GWGUI.Emulation;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class CoreNativeAudioCopyTests
{
    [Theory]
    [InlineData(0, 2)]
    [InlineData(1, 2)]
    [InlineData(7, 2)]
    [InlineData(3, 6)]
    [InlineData(2, 8)]
    public void CopiesExactlyTheRequestedInterleavedSamples(int frames, int channels)
    {
        var samples = Enumerable.Range(0, frames * channels + 2)
            .Select(index => unchecked((short)(index * 7919))).ToArray();
        var pointer = Marshal.AllocHGlobal(samples.Length * sizeof(short));
        try
        {
            Marshal.Copy(samples, 0, pointer, samples.Length);
            var result = EmulationSdk.AudioProcessing.CopyBatch(pointer, (nuint)frames, (uint)channels);
            Assert.Equal(samples[..(frames * channels)], result);
            if (channels == 2)
            {
                Assert.Equal(result, EmulationSdk.AudioProcessing.CopyBatch(pointer, (nuint)frames));
                Assert.Equal(result, EmulationSdk.AudioProcessing.CopyBatch(pointer, frames));
            }
            Marshal.WriteInt16(pointer, short.MaxValue);
            Assert.Equal(samples[..(frames * channels)], result);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Theory]
    [InlineData(2147483648UL, 1U)]
    [InlineData(1073741824UL, 2U)]
    [InlineData(1UL, 2147483648U)]
    [InlineData(536870912UL, 4U)]
    public void NativeCountsOverflowBeforeAllocationOrPointerRead(ulong frames, uint channels)
    {
        Assert.Throws<OverflowException>(() =>
            EmulationSdk.AudioProcessing.CopyBatch((nint)1, checked((nuint)frames), channels));
    }

    [Fact]
    public void BothStereoCountTypesCheckMultiplicationBeforeReading()
    {
        Assert.Throws<OverflowException>(() => EmulationSdk.AudioProcessing.CopyBatch((nint)1, 1073741824));
        Assert.Throws<OverflowException>(() => EmulationSdk.AudioProcessing.CopyBatch((nint)1, (nuint)1073741824));
    }
}
