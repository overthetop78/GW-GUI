using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using DiscUtils.Fat;
using DiscUtils.Streams;
using GWGUI.Emulation.HardDisks.Containers;
using GWGUI.Emulation.HardDisks.FileSystems;

namespace GWGUI.Tests.Emulation.HardDisks;

public sealed class SparseBundleTests
{
    [Fact]
    public void BandsReconstructTheFormattedDiskAndItsFileAcrossBandBoundaries()
    {
        var payload = new byte[2 * 1024 * 1024 + 17]; new Random(42).NextBytes(payload);
        var members = Capture(16L << 20, content =>
        {
            FatVolumeFormatter.Format(content, "DATA");
            using var fs = new FatFileSystem(content);
            using var file = fs.OpenFile("DATA.BIN", FileMode.Create, FileAccess.Write);
            file.Write(payload);
        }, 1 << 20);
        using var raw = Reassemble(members);
        using var reopened = new FatFileSystem(raw);
        Assert.Equal("DATA", reopened.VolumeLabel.Trim());
        using var read = reopened.OpenFile("DATA.BIN", FileMode.Open, FileAccess.Read);
        var actual = new byte[payload.Length]; read.ReadExactly(actual);
        Assert.Equal(payload, actual);
        Assert.True(members.Keys.Count(name => name.StartsWith("bands/", StringComparison.Ordinal)) >= 3);
    }

    [Fact]
    public void HexBandNamesPartialLastBandAndUnallocatedRegionsRetainTheirOffsets()
    {
        const int band = 1 << 20;
        var members = Capture(17L * band + 512, content =>
        {
            content.Position = 10L * band - 1; content.WriteByte(11); content.WriteByte(12);
            content.Position = 17L * band + 511; content.WriteByte(13);
        }, band);
        Assert.Single(members["bands/a"]);
        Assert.Equal(512, members["bands/11"].Length);
        Assert.False(members.ContainsKey("bands/10"));
        using var raw = Reassemble(members);
        raw.Position = 10L * band - 1; Assert.Equal(11, raw.ReadByte()); Assert.Equal(12, raw.ReadByte());
        raw.Position = 16L * band; Assert.Equal(0, raw.ReadByte());
        raw.Position = raw.Length - 1; Assert.Equal(13, raw.ReadByte());
    }

    [Fact]
    public void EmptyTerabyteNeedsOnlyMetadataTokenAndAnEmptyBand()
    {
        var members = Capture(1L << 40);
        Assert.Equal(4, members.Count);
        Assert.Empty(members["bands/0"]); Assert.Empty(members["token"]);
        Assert.Equal(members["Info.plist"], members["Info.bckup"]);
        using var raw = Reassemble(members);
        Assert.Equal(1L << 40, raw.Length);
        raw.Position = raw.Length - 1; Assert.Equal(0, raw.ReadByte());
    }

    [Fact]
    public void InvalidPlansOrInitializersEmitNothing()
    {
        var count = 0;
        void Emit(string name, Stream source) => count++;
        Assert.Throws<ArgumentOutOfRangeException>(() => SparseBundleImageWriter.Write(513, Emit));
        Assert.Throws<ArgumentOutOfRangeException>(() => SparseBundleImageWriter.Write((1L << 40) + 512, Emit));
        Assert.Throws<ArgumentOutOfRangeException>(() => SparseBundleImageWriter.Write(512, Emit, bandBytes: 3 << 20));
        Assert.Throws<IOException>(() => SparseBundleImageWriter.Write(512, Emit, _ => throw new IOException()));
        Assert.Throws<InvalidOperationException>(() => SparseBundleImageWriter.Write(512, Emit, stream => stream.SetLength(0)));
        Assert.Equal(0, count);
    }

    private static Dictionary<string, byte[]> Capture(long capacity, Action<Stream>? initialize = null,
        int bandBytes = SparseBundleImageWriter.DefaultBandBytes)
    {
        var members = new Dictionary<string, byte[]>();
        SparseBundleImageWriter.Write(capacity, (name, source) =>
        {
            using var copy = new MemoryStream(); source.CopyTo(copy); members.Add(name, copy.ToArray());
        }, initialize, bandBytes);
        return members;
    }

    internal static SparseMemoryStream Reassemble(Dictionary<string, byte[]> members)
    {
        using var info = new MemoryStream(members["Info.plist"]);
        using var xml = XmlReader.Create(info, new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, XmlResolver = null });
        var elements = XDocument.Load(xml).Root!.Element("dict")!.Elements().ToArray();
        var values = Enumerable.Range(0, elements.Length / 2).ToDictionary(i => elements[2 * i].Value, i => elements[2 * i + 1].Value);
        Assert.Equal("com.apple.diskimage.sparsebundle", values["diskimage-bundle-type"]);
        Assert.Equal("1", values["bundle-backingstore-version"]);
        var bandBytes = long.Parse(values["band-size"], CultureInfo.InvariantCulture);
        var result = new SparseMemoryStream(); result.SetLength(long.Parse(values["size"], CultureInfo.InvariantCulture));
        foreach (var (name, bytes) in members.Where(pair => pair.Key.StartsWith("bands/", StringComparison.Ordinal)))
        {
            var index = long.Parse(name[6..], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            Assert.True(bytes.LongLength <= bandBytes);
            result.Position = index * bandBytes; Assert.True(result.Position + bytes.Length <= result.Length);
            result.Write(bytes);
        }
        result.Position = 0; return result;
    }
}
