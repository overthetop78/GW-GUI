using GWGUI.Emulation;
using GWGUI.Emulation.Atari;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

public sealed class AtariVideoCallbackTests
{
    public static TheoryData<int, EmulationPixelFormat> PixelFormats => new()
    {
        { AtariConstants.PixelFormat0Rgb1555, EmulationPixelFormat.Rgb1555 },
        { AtariConstants.PixelFormatRgb565, EmulationPixelFormat.Rgb565 },
        { AtariConstants.PixelFormatXrgb8888, EmulationPixelFormat.Xrgb8888 }
    };

    public static TheoryData<AtariMachineFamily, uint, uint, float> FamilyRatios => new()
    {
        { AtariMachineFamily.St, AtariVideoTestConstants.StWidth, AtariVideoTestConstants.StHeight,
            AtariVideoTestConstants.StRatio },
        { AtariMachineFamily.EightBit, AtariVideoTestConstants.EightBitWidth,
            AtariVideoTestConstants.StandardHeight, AtariVideoTestConstants.EightBitRatio },
        { AtariMachineFamily.Atari5200, AtariVideoTestConstants.EightBitWidth,
            AtariVideoTestConstants.StandardHeight, AtariVideoTestConstants.EightBitRatio },
        { AtariMachineFamily.Atari2600, AtariVideoTestConstants.StandardWidth,
            AtariVideoTestConstants.StandardHeight, AtariVideoTestConstants.FourThreeRatio },
        { AtariMachineFamily.Atari7800, AtariVideoTestConstants.StandardWidth,
            AtariVideoTestConstants.StandardHeight, AtariVideoTestConstants.FourThreeRatio },
        { AtariMachineFamily.Lynx, AtariVideoTestConstants.LynxWidth, AtariVideoTestConstants.LynxHeight,
            AtariVideoTestConstants.LynxRatio },
        { AtariMachineFamily.Jaguar, AtariVideoTestConstants.StandardWidth,
            AtariVideoTestConstants.StandardHeight, AtariVideoTestConstants.FourThreeRatio }
    };

    [Theory]
    [MemberData(nameof(PixelFormats))]
    public void NegotiatedPixelFormat_IsPublished(int nativeFormat, EmulationPixelFormat expected)
    {
        using var fixture = CreateCallbacks();
        var callbacks = fixture.Callbacks;
        var formatPointer = Marshal.AllocHGlobal(sizeof(int));
        var pixels = Marshal.AllocHGlobal(AtariVideoTestConstants.SinglePixelPitch);
        try
        {
            Marshal.WriteInt32(formatPointer, nativeFormat);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetPixelFormat, formatPointer));
            callbacks.Video(pixels, AtariVideoTestConstants.SinglePixelWidth,
                AtariVideoTestConstants.SinglePixelHeight, AtariVideoTestConstants.SinglePixelPitch);

            Assert.Equal(expected, Assert.IsType<VideoFrame>(callbacks.LatestVideoFrame).PixelFormat);
        }
        finally
        {
            Marshal.FreeHGlobal(pixels);
            Marshal.FreeHGlobal(formatPointer);
        }
    }

    [Fact]
    public void VideoCallback_CopiesPitchTimesHeightRowByRowIncludingPadding()
    {
        using var fixture = CreateCallbacks();
        var callbacks = fixture.Callbacks;
        var source = Enumerable.Range(AtariVideoTestConstants.FirstByte,
            AtariVideoTestConstants.PaddedFrameLength).Select(value => (byte)value).ToArray();
        var pointer = Marshal.AllocHGlobal(source.Length);
        try
        {
            Marshal.Copy(source, AtariVideoTestConstants.FirstByte, pointer, source.Length);
            callbacks.Video(pointer, AtariVideoTestConstants.PaddedWidth, AtariVideoTestConstants.PaddedHeight,
                AtariVideoTestConstants.PaddedPitch);

            var frame = Assert.IsType<VideoFrame>(callbacks.LatestVideoFrame);
            Assert.Equal(AtariVideoTestConstants.PaddedPitch, frame.Pitch);
            Assert.Equal(source, frame.Pixels.ToArray());
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Fact]
    public void NullPointer_DuplicatesPreviousFrameWithSequenceAndTimestamp()
    {
        using var fixture = CreateCallbacks();
        var callbacks = fixture.Callbacks;
        var pointer = Marshal.AllocHGlobal(AtariVideoTestConstants.SinglePixelPitch);
        try
        {
            callbacks.Video(pointer, AtariVideoTestConstants.SinglePixelWidth,
                AtariVideoTestConstants.SinglePixelHeight, AtariVideoTestConstants.SinglePixelPitch);
            var first = Assert.IsType<VideoFrame>(callbacks.LatestVideoFrame);
            callbacks.Video(nint.Zero, default, default, default);
            var duplicate = Assert.IsType<VideoFrame>(callbacks.LatestVideoFrame);

            Assert.Equal(first.Pixels, duplicate.Pixels);
            Assert.Equal(first.Sequence + AtariVideoTestConstants.SequenceStep, duplicate.Sequence);
            Assert.True(duplicate.Timestamp >= first.Timestamp);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Fact]
    public void Callback_AlternatesBuffersAndHandlesDynamicResolution()
    {
        using var fixture = CreateCallbacks();
        var callbacks = fixture.Callbacks;
        var pointer = Marshal.AllocHGlobal(AtariVideoTestConstants.DynamicFrameLength);
        var geometryPointer = Marshal.AllocHGlobal(Marshal.SizeOf<ExternalCoreApi.Geometry>());
        try
        {
            callbacks.Video(pointer, AtariVideoTestConstants.FirstDynamicWidth,
                AtariVideoTestConstants.DynamicHeight, AtariVideoTestConstants.FirstDynamicPitch);
            var first = Assert.IsType<VideoFrame>(callbacks.LatestVideoFrame);
            callbacks.Video(pointer, AtariVideoTestConstants.FirstDynamicWidth,
                AtariVideoTestConstants.DynamicHeight, AtariVideoTestConstants.FirstDynamicPitch);
            var second = Assert.IsType<VideoFrame>(callbacks.LatestVideoFrame);
            Marshal.StructureToPtr(new ExternalCoreApi.Geometry
            {
                BaseWidth = AtariVideoTestConstants.SecondDynamicWidth,
                BaseHeight = AtariVideoTestConstants.DynamicHeight,
                MaximumWidth = AtariVideoTestConstants.SecondDynamicWidth,
                MaximumHeight = AtariVideoTestConstants.DynamicHeight,
                AspectRatio = AtariVideoTestConstants.DynamicRatio
            }, geometryPointer, false);
            Assert.True(callbacks.Environment(ExternalCoreApiConstants.SetGeometry, geometryPointer));
            callbacks.Video(pointer, AtariVideoTestConstants.SecondDynamicWidth,
                AtariVideoTestConstants.DynamicHeight, AtariVideoTestConstants.SecondDynamicPitch);
            var third = Assert.IsType<VideoFrame>(callbacks.LatestVideoFrame);

            Assert.True(MemoryMarshal.TryGetArray(first.Pixels, out var firstSegment));
            Assert.True(MemoryMarshal.TryGetArray(second.Pixels, out var secondSegment));
            Assert.True(MemoryMarshal.TryGetArray(third.Pixels, out var thirdSegment));
            Assert.NotSame(firstSegment.Array, secondSegment.Array);
            Assert.Equal(AtariVideoTestConstants.DynamicFrameLength, third.Pixels.Length);
            Assert.Equal(checked((int)AtariVideoTestConstants.SecondDynamicWidth), third.Width);
            Assert.Equal(AtariVideoTestConstants.DynamicRatio, third.AspectRatio);
        }
        finally
        {
            Marshal.FreeHGlobal(geometryPointer);
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Theory]
    [MemberData(nameof(FamilyRatios))]
    public void FamilyGeometry_PublishesExactDimensionsAndRatio(AtariMachineFamily _, uint width,
        uint height, float ratio)
    {
        using var fixture = CreateCallbacks();
        var callbacks = fixture.Callbacks;
        callbacks.ApplySystemAvInfo(new ExternalCoreApi.SystemAvInfo
        {
            Geometry = new ExternalCoreApi.Geometry
            {
                BaseWidth = width,
                BaseHeight = height,
                MaximumWidth = width,
                MaximumHeight = height,
                AspectRatio = ratio
            }
        });
        var pitch = checked((nuint)(width * AtariVideoTestConstants.BytesPerXrgbPixel));
        var pointer = Marshal.AllocHGlobal(checked((int)(pitch * height)));
        try
        {
            callbacks.Video(pointer, width, height, pitch);
            var frame = Assert.IsType<VideoFrame>(callbacks.LatestVideoFrame);

            Assert.Equal(checked((int)width), frame.Width);
            Assert.Equal(checked((int)height), frame.Height);
            Assert.Equal(ratio, frame.AspectRatio);
            Assert.True(frame.Timestamp > TimeSpan.Zero);
        }
        finally
        {
            Marshal.FreeHGlobal(pointer);
        }
    }

    [Fact]
    public void BufferSet_ReturnsReplacedAndDisposedBuffers()
    {
        var pool = new TrackingBytePool();
        using (var buffers = new AtariVideoBufferSet(pool))
        {
            _ = buffers.Rent(AtariVideoTestConstants.SmallBufferLength);
            _ = buffers.Rent(AtariVideoTestConstants.SmallBufferLength);
            _ = buffers.Rent(AtariVideoTestConstants.LargeBufferLength);
            Assert.Equal(AtariVideoTestConstants.ReplacedBufferReturnCount, pool.ReturnCount);
        }
        Assert.Equal(AtariVideoTestConstants.TotalBufferReturnCount, pool.ReturnCount);
    }

    private static CallbackFixture CreateCallbacks()
    {
        var root = Path.Combine(Path.GetTempPath(), $"gwgui-atari-video-{Guid.NewGuid():N}");
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

    private sealed class TrackingBytePool : ArrayPool<byte>
    {
        public int ReturnCount { get; private set; }
        public override byte[] Rent(int minimumLength) => new byte[minimumLength];
        public override void Return(byte[] array, bool clearArray = false) => ReturnCount++;
    }
}

internal static class AtariVideoTestConstants
{
    internal const int FirstByte = 0;
    internal const int BytesPerXrgbPixel = 4;
    internal const int SinglePixelPitch = BytesPerXrgbPixel;
    internal const uint SinglePixelWidth = 1;
    internal const uint SinglePixelHeight = 1;
    internal const uint PaddedWidth = 2;
    internal const uint PaddedHeight = 2;
    internal const int PaddedPitch = 12;
    internal const int PaddedFrameLength = PaddedPitch * (int)PaddedHeight;
    internal const long SequenceStep = 1;
    internal const uint FirstDynamicWidth = 2;
    internal const uint SecondDynamicWidth = 4;
    internal const uint DynamicHeight = 2;
    internal const int FirstDynamicPitch = 8;
    internal const int SecondDynamicPitch = 16;
    internal const int DynamicFrameLength = SecondDynamicPitch * (int)DynamicHeight;
    internal const float DynamicRatio = 2f;
    internal const int SmallBufferLength = 8;
    internal const int LargeBufferLength = 32;
    internal const int ReplacedBufferReturnCount = 1;
    internal const int TotalBufferReturnCount = 3;
    internal const uint StWidth = 640;
    internal const uint StHeight = 400;
    internal const float StRatio = 1.6f;
    internal const uint EightBitWidth = 336;
    internal const uint StandardWidth = 320;
    internal const uint StandardHeight = 240;
    internal const float EightBitRatio = 1.4f;
    internal const float FourThreeRatio = 4f / 3f;
    internal const uint LynxWidth = 160;
    internal const uint LynxHeight = 102;
    internal const float LynxRatio = 1.5686275f;
}
