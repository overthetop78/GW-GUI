using GWGUI.MediaEngine.Formats.Floppy.Hfe;

namespace GWGUI.Tests.Media.ImageContainers;
internal static class HfeContainerScenarios
{
    public static async Task Chunks()
    {
        var bits=new bool[2056]; bits[0]=bits[2048]=true;
        var image=new HfeImage(0,1,1,0,250,[new(0,0,bits,80)]);
        var files=new GWGUI.Tests.Application.TestInfrastructure.MemoryImageFiles(); var writer=new HfeWriter(files);
        await writer.WriteAsync(image,"output"); var bytes=files.Files["output"];
        Assert.Equal(2048,bytes.Length); Assert.Equal(1,bytes[1024]); Assert.Equal(1,bytes[1536]);
        Assert.Equal(bits,Assert.Single(new HfeReader().Read(bytes).Tracks).Bits);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(()=>writer.WriteAsync(image,"cancelled",new CancellationToken(true))); Assert.False(files.Files.ContainsKey("cancelled"));
        bytes[8]=1; Assert.Throws<NotSupportedException>(()=>new HfeReader().Read(bytes));
        Assert.Throws<InvalidDataException>(()=>HfeWriter.Build(new HfeImage(0,1,1,0,250,[image.Tracks[0],image.Tracks[0]])));
    }

    private static byte[] Bytes()
    {
        var bytes = new byte[1536];
        "HXCPICFE"u8.CopyTo(bytes); bytes[9] = 1; bytes[10] = 2; bytes[12] = 250; bytes[18] = 1;
        bytes[512] = 2; bytes[514] = 2;
        bytes[1024] = 0x01; bytes[1280] = 0x02;
        return bytes;
    }
    public static void ReadWrite()
    {
        var image = new HfeReader().Read(Bytes());
        Assert.Equal(1, image.Cylinders); Assert.Equal(2, image.Heads); Assert.Equal(250, image.BitRate);
        Assert.Equal(new[] { true, false, false, false, false, false, false, false }, image.Tracks[0].Bits);
        Assert.Equal(new[] { false, true, false, false, false, false, false, false }, image.Tracks[1].Bits);
        Assert.All(image.Tracks, track => Assert.Equal(80u, track.BitCellTicks));
        var written = HfeWriter.Build(image);
        Assert.Equal("HXCPICFE"u8.ToArray(), written[..8]);
        Assert.Equal(new byte[] { 2, 0, 2, 0 }, written[512..516]);
        Assert.Equal(0x01, written[1024]); Assert.Equal(0x02, written[1280]);
        Assert.All(written[1025..1280], value => Assert.Equal(0x88, value));
        Assert.Equal(image.Tracks[1].Bits, new HfeReader().Read(written).Tracks[1].Bits);
    }

    public static void Version3()
    {
        var bytes = new byte[1536];
        "HXCHFEV3"u8.CopyTo(bytes); bytes[9] = 1; bytes[10] = 2; bytes[12] = 250; bytes[18] = 1;
        bytes[512] = 2; bytes[514] = 8;
        bytes[1024] = HfeFormat.Version3IndexOpcode;
        bytes[1025] = HfeFormat.Version3BitRateOpcode;
        bytes[1026] = 0x12;
        bytes[1027] = HfeFormat.Version3RandomOpcode;
        bytes[1280] = 0x01;
        bytes[1281] = HfeFormat.Version3SkipBitsOpcode;
        bytes[1282] = 0x20;
        bytes[1283] = 0x80;

        var image = new HfeReader().Read(bytes);
        Assert.Equal(2, image.Tracks.Count);
        var first = image.Tracks[0];
        Assert.Equal(8, first.Bits.Count);
        Assert.Contains(first.Features, feature => feature.Kind == GWGUI.MediaEngine.Representations.Flux.TrackFeatureKind.WeakRegion);
        Assert.Contains(first.Features, feature => feature.Kind == GWGUI.MediaEngine.Representations.Flux.TrackFeatureKind.IndexMark);
        Assert.Single(first.Timing);
        Assert.Equal(80u, first.BitCellTicks);
        Assert.Equal(12, image.Tracks[1].Bits.Count);
    }

    public static void Invalid(int variant)
    {
        var bytes = Bytes();
        switch (variant) { case 0: bytes[0] = 0; break; case 1: bytes[10] = 3; break; case 2: bytes[514] = 1; break; default: bytes[512] = 20; break; }
        Assert.Throws<InvalidDataException>(() => new HfeReader().Read(bytes));
    }
}
