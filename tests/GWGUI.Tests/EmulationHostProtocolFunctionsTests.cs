using GWGUI.Emulation;
using GWGUI.Emulation.Common;
using GWGUI.MediaEngine.Exploration.Scp;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace GWGUI.Tests;

public sealed class EmulationHostProtocolFunctionsTests
{
    private const string TestHostName = "Test";
    private const int TestSampleRate = 44_100;

    [Fact]
    public void Input_RoundTripsThroughCommonProtocolFunctions()
    {
        var input = new EmulationInputSnapshot(
            new HashSet<EmulationKey> { EmulationKey.A, EmulationKey.Return },
            new EmulationPointerState(4, -3, 1, true, false, true),
            [new EmulationControllerState(7, 1, 2, 3, 4, 5, 6)
            {
                DeviceId = "gameinput:device",
                Controls = new EmulationControllerControls(new Dictionary<string, float>
                {
                    ["Axis7"] = .75f,
                    ["Button32"] = 1f
                })
            }]);

        var result = RoundTrip(
            writer => EmulationHostProtocolFunctions.WriteInput(writer, input),
            reader => EmulationHostProtocolFunctions.ReadInput(reader, TestHostName));

        Assert.Equal(input.Keys.Order(), result.Keys.Order());
        Assert.Equal(input.Pointer, result.Pointer);
        Assert.Equal(input.Controllers, result.Controllers);
    }

    [Fact]
    public void VideoAndAudio_RoundTripThroughCommonProtocolFunctions()
    {
        var frame = new VideoFrame(new byte[] { 1, 2, 3, 4 }, 2, 2, 2,
            EmulationPixelFormat.Xrgb8888, 1.25f, 9, TimeSpan.FromMilliseconds(12));
        var audio = new[]
        {
            new AudioChunk(new short[] { -2, 3, 4, -5 }, TestSampleRate, 2, 10, TimeSpan.FromMilliseconds(20))
        };

        var frameResult = RoundTrip(
            writer => EmulationHostProtocolFunctions.WriteFrame(writer, frame),
            reader => EmulationHostProtocolFunctions.ReadFrame(reader, TestHostName));
        var audioResult = RoundTrip(
            writer => EmulationHostProtocolFunctions.WriteAudio(writer, audio),
            reader => EmulationHostProtocolFunctions.ReadAudio(reader, TestHostName));

        Assert.NotNull(frameResult);
        Assert.Equal(frame.Pixels.ToArray(), frameResult.Pixels.ToArray());
        Assert.Equal(frame with { Pixels = ReadOnlyMemory<byte>.Empty }, frameResult with { Pixels = ReadOnlyMemory<byte>.Empty });
        Assert.Single(audioResult);
        Assert.Equal(audio[0].InterleavedStereo.ToArray(), audioResult[0].InterleavedStereo.ToArray());
        Assert.Equal(audio[0] with { InterleavedStereo = ReadOnlyMemory<short>.Empty },
            audioResult[0] with { InterleavedStereo = ReadOnlyMemory<short>.Empty });
    }

    [Fact]
    public void SharedVideo_RoundTripsThroughCommonProtocolFunctions()
    {
        var frame = new VideoFrame(new byte[] { 8, 7, 6, 5 }, 2, 2, 2,
            EmulationPixelFormat.Rgb565, 1f, 3, TimeSpan.FromMilliseconds(4));
        using var memory = MemoryMappedFile.CreateNew(null, EmulationHostProtocolConstants.VideoMapCapacity);
        using var map = memory.CreateViewAccessor(0, EmulationHostProtocolConstants.VideoMapCapacity);

        var result = RoundTrip(
            writer => EmulationHostProtocolFunctions.WriteSharedFrame(writer, frame, map, TestHostName),
            reader => EmulationHostProtocolFunctions.ReadSharedFrame(reader, map, TestHostName));

        Assert.NotNull(result);
        Assert.Equal(frame.Pixels.ToArray(), result.Pixels.ToArray());
        Assert.Equal(frame with { Pixels = ReadOnlyMemory<byte>.Empty }, result with { Pixels = ReadOnlyMemory<byte>.Empty });
    }

    [Fact]
    public void InvalidPayloadLength_ReportsNamedHost()
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true)) writer.Write(-1);
        stream.Position = 0;
        using var reader = new BinaryReader(stream);

        var error = Assert.Throws<InvalidDataException>(() =>
            EmulationHostProtocolFunctions.ReadBytes(reader, TestHostName));

        Assert.Contains(TestHostName, error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Utf8String_ReleasesItsAllocationAndCanBeDisposedTwice()
    {
        var value = new ExternalCoreUtf8String("Atari et Amiga");
        Assert.Equal("Atari et Amiga", Marshal.PtrToStringUTF8(value.Pointer));

        value.Dispose();
        value.Dispose();

        Assert.Equal(nint.Zero, value.Pointer);
    }

    private static TResult RoundTrip<TResult>(Action<BinaryWriter> write, Func<BinaryReader, TResult> read)
    {
        using var stream = new MemoryStream();
        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true)) write(writer);
        stream.Position = 0;
        using var reader = new BinaryReader(stream);
        return read(reader);
    }
}
