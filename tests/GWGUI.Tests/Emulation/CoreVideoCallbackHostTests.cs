using System.Runtime.InteropServices;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Services;

namespace GWGUI.Tests.Emulation;

public sealed class CoreVideoCallbackHostTests
{
    [Theory]
    [InlineData(EmulationPixelFormat.Xrgb8888, 12)]
    [InlineData(EmulationPixelFormat.Rgb565, 8)]
    [InlineData(EmulationPixelFormat.Rgb1555, 8)]
    public void CopiesPaddedRowsAndRepeatsTheLastFrame(EmulationPixelFormat format, int pitch)
    {
        using var host = new ExternalCoreVideoCallbackHost { PixelFormat = format, AspectRatio = 1.25f };
        var pixels = Enumerable.Range(0, pitch * 2).Select(index => (byte)index).ToArray();
        var pointer = Marshal.AllocHGlobal(pixels.Length);
        try
        {
            host.Callback(0, 0, 0, 0);
            Assert.Null(host.LatestFrame);
            Marshal.Copy(pixels, 0, pointer, pixels.Length);
            host.Callback(pointer, 2, 2, (nuint)pitch);
            var first = Assert.IsType<GWGUI.Emulation.Contracts.VideoFrame>(host.LatestFrame);
            Assert.Equal(pixels, first.Pixels.ToArray());
            Assert.Equal((2, 2, pitch, format, 1.25f, 1L),
                (first.Width, first.Height, first.Pitch, first.PixelFormat, first.AspectRatio, first.Sequence));
            Marshal.WriteByte(pointer, 255);
            host.Callback(0, 999, 999, 999);
            var repeated = host.LatestFrame!;
            Assert.Equal(first.Pixels, repeated.Pixels);
            Assert.Equal(pixels, repeated.Pixels.ToArray());
            Assert.Equal(2, repeated.Sequence);
            Assert.True(repeated.Timestamp >= first.Timestamp);
            Assert.Null(host.Failure);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Theory]
    [InlineData(0U, 1U, 4UL)]
    [InlineData(1U, 0U, 4UL)]
    [InlineData(1U, 1U, 0UL)]
    public void IgnoresEmptyImagesBeforeReadingThePointer(uint width, uint height, ulong pitch)
    {
        using var host = new ExternalCoreVideoCallbackHost();
        host.Callback(1, width, height, (nuint)pitch);
        Assert.Null(host.LatestFrame);
        Assert.Null(host.Failure);
    }

    [Fact]
    public void RejectsInvalidSizesBeforePointerReadAndRetainsTheFirstError()
    {
        using var host = new ExternalCoreVideoCallbackHost();
        host.Callback(1, 2, 1, 4);
        var error = Assert.IsType<InvalidOperationException>(host.Failure);
        host.Callback(1, 1, 1, int.MaxValue);
        Assert.Same(error, host.Failure);
        Assert.Null(host.LatestFrame);
        using var oversized = new ExternalCoreVideoCallbackHost();
        oversized.Callback(1, 1, 1, int.MaxValue);
        Assert.IsType<InvalidOperationException>(oversized.Failure);
        using var overflowing = new ExternalCoreVideoCallbackHost();
        overflowing.Callback(1, 1, 2, int.MaxValue);
        Assert.IsType<OverflowException>(overflowing.Failure);
    }

    [Fact]
    public void FailedFrameKeepsThePreviousImageAndDisposeReleasesItsState()
    {
        using var host = new ExternalCoreVideoCallbackHost();
        var pointer = Marshal.AllocHGlobal(4);
        try
        {
            Marshal.WriteInt32(pointer, 123);
            host.Callback(pointer, 1, 1, 4);
            var previous = host.LatestFrame;
            host.Callback(1, 2, 1, 4);
            Assert.Same(previous, host.LatestFrame);
            host.Callback(0, 0, 0, 0);
            Assert.Equal(2, host.LatestFrame!.Sequence);
            host.Dispose();
            Assert.Null(host.LatestFrame);
            Assert.Null(host.Failure);
            host.Callback(1, 1, 1, 4);
            Assert.Null(host.LatestFrame);
            Assert.Null(host.Failure);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }
}
