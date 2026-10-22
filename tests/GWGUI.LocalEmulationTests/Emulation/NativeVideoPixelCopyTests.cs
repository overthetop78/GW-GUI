using GWGUI.Emulation;
using System.Runtime.InteropServices;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class NativeVideoPixelCopyTests
{
    [Theory]
    [InlineData(0u, 8, 0)]
    [InlineData(2u, 0, 0)]
    [InlineData(1u, 8, 8)]
    [InlineData(2u, 8, 16)]
    [InlineData(3u, 5, 15)]
    public void CopiesTheCompleteContiguousBufferIncludingRowPadding(uint height, int pitch, int expectedLength)
    {
        var source = Enumerable.Range(0, 32).Select(value => (byte)(value + 1)).ToArray();
        var pointer = Marshal.AllocHGlobal(source.Length);
        try
        {
            Marshal.Copy(source, 0, pointer, source.Length);
            var copy = EmulationSdk.VideoProcessing.CopyFramePixels(pointer, height, (nuint)pitch);
            Assert.Equal(source.Take(expectedLength), copy);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Fact]
    public void EachFrameOwnsItsArrayAndDoesNotModifyTheNativeSource()
    {
        byte[] source = [10, 20, 30, 40];
        var pointer = Marshal.AllocHGlobal(source.Length);
        try
        {
            Marshal.Copy(source, 0, pointer, source.Length);
            var first = EmulationSdk.VideoProcessing.CopyFramePixels(pointer, 1, 4);
            var second = EmulationSdk.VideoProcessing.CopyFramePixels(pointer, 1, 4);
            Assert.NotSame(first, second);
            first[0] = 99;
            Assert.Equal(source, second);
            Assert.Equal(source, EmulationSdk.VideoProcessing.CopyFramePixels(pointer, 1, 4));
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Fact]
    public void OversizedFramesFailBeforeAllocatingOrReadingTheSource()
    {
        Assert.Throws<OverflowException>(() => EmulationSdk.VideoProcessing.CopyFramePixels(nint.Zero, 1, (nuint)int.MaxValue + 1));
        Assert.Throws<OverflowException>(() => EmulationSdk.VideoProcessing.CopyFramePixels(nint.Zero, uint.MaxValue, 2));
        Assert.Throws<OverflowException>(() => EmulationSdk.VideoProcessing.CopyFramePixels(nint.Zero, 2, nuint.MaxValue));
    }
}
