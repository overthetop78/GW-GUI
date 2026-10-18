using System.Buffers.Binary;
using System.IO;
using System.Text;
using GWGUI.Emulation.Enums;
using GWGUI.Emulation.Functions;

namespace GWGUI.Tests.Emulation;

public sealed class RuntimeStatusProtocolTests
{
    private const int BufferedFrames = 17;
    private const long Overruns = 0x1122334455667788L;
    private const long Underruns = 0x2233445566778899L;

    [Theory]
    [InlineData(null)]
    [InlineData(EmulationVideoStandard.Ntsc)]
    [InlineData(EmulationVideoStandard.Pal)]
    public void RuntimeFragmentKeepsItsTwentyFourByteLayout(EmulationVideoStandard? standard)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
        EmulationHostProtocolFunctions.WriteRuntimeStatus(writer, standard, BufferedFrames, Overruns, Underruns);
        writer.Flush();
        var expectedRegion = standard is null ? -1 : (int)standard.Value;
        Assert.Equal(GoldenPacket(expectedRegion), stream.ToArray());
        var region = -99;
        var buffered = -99;
        long overruns = -99, underruns = -99;
        stream.Position = 0;
        using var reader = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
        EmulationHostProtocolFunctions.ReadRuntimeStatus(reader,
            value => region = value, value => buffered = value,
            value => overruns = value, value => underruns = value);
        Assert.Equal(expectedRegion, region);
        Assert.Equal(BufferedFrames, buffered);
        Assert.Equal(Overruns, overruns);
        Assert.Equal(Underruns, underruns);
        Assert.Equal(24, stream.Length);
        Assert.Equal(stream.Length, stream.Position);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 1)]
    [InlineData(8, 2)]
    [InlineData(15, 2)]
    [InlineData(16, 3)]
    [InlineData(23, 3)]
    public void TruncatedFragmentsKeepEarlierAssignmentsInTheirOriginalOrder(int length, int completed)
    {
        using var stream = new MemoryStream(GoldenPacket(-1).AsSpan(0, length).ToArray());
        using var reader = new BinaryReader(stream);
        var calls = new List<string>();
        Assert.Throws<EndOfStreamException>(() =>
            EmulationHostProtocolFunctions.ReadRuntimeStatus(reader,
                value => { Assert.Equal(-1, value); calls.Add("region"); },
                value => { Assert.Equal(BufferedFrames, value); calls.Add("buffered"); },
                value => { Assert.Equal(Overruns, value); calls.Add("overruns"); },
                value => { Assert.Equal(Underruns, value); calls.Add("underruns"); }));
        Assert.Equal(new[] { "region", "buffered", "overruns", "underruns" }.Take(completed), calls);
    }

    [Fact]
    public void CallbackFailureStopsBeforeTheNextReadAndKeepsTheReaderBorrowed()
    {
        using var stream = new MemoryStream(GoldenPacket(-1));
        using var reader = new BinaryReader(stream);
        var expected = new InvalidOperationException("region decoder failed");
        var laterAssignment = false;
        var actual = Assert.Throws<InvalidOperationException>(() =>
            EmulationHostProtocolFunctions.ReadRuntimeStatus(reader,
                _ => throw expected,
                _ => laterAssignment = true,
                _ => laterAssignment = true,
                _ => laterAssignment = true));
        Assert.Same(expected, actual);
        Assert.False(laterAssignment);
        Assert.Equal(4, stream.Position);
        Assert.True(stream.CanRead);
    }

    private static byte[] GoldenPacket(int region)
    {
        var packet = new byte[24];
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(0, 4), region);
        BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(4, 4), BufferedFrames);
        BinaryPrimitives.WriteInt64LittleEndian(packet.AsSpan(8, 8), Overruns);
        BinaryPrimitives.WriteInt64LittleEndian(packet.AsSpan(16, 8), Underruns);
        return packet;
    }
}

