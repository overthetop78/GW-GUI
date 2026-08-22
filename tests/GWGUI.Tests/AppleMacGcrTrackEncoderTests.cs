using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;

namespace GWGUI.Tests;

/// <summary>Vérifie les champs, limites et codecs communs de l'encodage Apple IWM GCR.</summary>
public sealed class AppleMacGcrTrackEncoderTests
{
    /// <summary>Vérifie l'identité et le format par défaut de l'encodeur Macintosh.</summary>
    [Fact]
    public void EncoderExposesMacintoshIdentityAndDefaultFormat()
    {
        var encoder = new AppleMacGcrTrackEncoder();
        Assert.Equal(FluxCodecIds.AppleMacGcr, encoder.Id);
        Assert.Equal(FluxCodecDisplayNames.AppleMacGcr, encoder.DisplayName);
        var decoded = Decode(encoder, 0, 0, Sector());
        Assert.Equal(AppleIwmGcrFormat.DefaultFormat, Assert.Single(decoded.Sectors!).FormatCode);
    }

    /// <summary>Vérifie la composition des bits bas et hauts du cylindre dans le champ d'adresse.</summary>
    [Theory]
    [InlineData(3, 0, 3, 0)]
    [InlineData(130, 1, 2, 34)]
    public void AddressFieldContainsExpectedCylinderAndHead(int cylinder, int head, byte lowCylinder, byte sideAndHighCylinder)
    {
        var encoded = new AppleMacGcrTrackEncoder().Encode(new(cylinder, head, [Sector()]));
        var markOffset = Find(encoded.Bits, Bits(AppleIwmGcrFormat.AddressMark));
        Assert.True(markOffset >= 0);
        var symbols = Enumerable.Range(0, AppleIwmGcrFormat.HeaderSymbolCount).Select(index => ReadByte(encoded.Bits, markOffset + AppleIwmGcrFormat.MarkBitCount + index * 8)).ToArray();
        byte[] values = [lowCylinder, 3, sideAndHighCylinder, AppleIwmGcrFormat.DefaultFormat];
        var checksum = (byte)(values.Aggregate(0, (current, value) => current ^ value) & AppleIwmGcrFormat.SixBitMask);
        Assert.Equal(values.Append(checksum).Select(value => AppleIwmGcrFormat.SixAndTwoTable[value]), symbols);
    }

    /// <summary>Vérifie les douze tags, les données et l'aller-retour public Macintosh et Lisa.</summary>
    [Theory]
    [InlineData(FluxCodecIds.AppleMacGcr)]
    [InlineData(FluxCodecIds.AppleLisaFileWareGcr)]
    public void PublicEncodersRoundTripTagsAndData(string codecId)
    {
        var attributes = Enumerable.Range(0, AppleIwmGcrFormat.TagByteCount).ToDictionary(AppleIwmGcrFormat.TagAttributeName, index => index * 17);
        var sector = Sector(attributes);
        var encoded = new FluxEncoderRegistry().Encode(codecId, new(2, 0, [sector]));
        var decoded = new FluxDecoderRegistry().Decode(codecId, encoded.Revolution);
        var actual = Assert.Single(decoded.Sectors!);
        Assert.Equal(sector.Data, actual.Data);
        Assert.Equal(Enumerable.Range(0, AppleIwmGcrFormat.TagByteCount).Select(index => (byte)(index * 17)), actual.Tag);
        Assert.True(actual.IntegrityValid);
    }

    /// <summary>Vérifie que toutes les valeurs converties ou indexées sont rejetées hors de leur plage.</summary>
    [Fact]
    public void EncoderRejectsInvalidSizeCoordinatesFormatSectorAndTags()
    {
        var encoder = new AppleMacGcrTrackEncoder();
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(3, new byte[AppleIwmGcrFormat.SectorByteCount - 1])])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(TrackEncodingLimits.MaximumCylinder + 1, 0, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, AppleIwmGcrFormat.MaximumHead + 1, [Sector()])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [Sector(number: AppleIwmGcrFormat.MaximumSixBitValue + 1)])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [Sector()], new Dictionary<string, int> { [AppleIwmGcrFormat.FormatAttributeName] = AppleIwmGcrFormat.MaximumSixBitValue + 1 })));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [Sector(new Dictionary<string, int> { [AppleIwmGcrFormat.TagAttributeName(0)] = -1 })])));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [Sector(new Dictionary<string, int> { [AppleIwmGcrFormat.TagAttributeName(0)] = AppleIwmGcrFormat.MaximumTagValue + 1 })])));
    }

    /// <summary>Vérifie les longueurs exactes et l'aller-retour du codec 6-and-2 commun.</summary>
    [Fact]
    public void CommonCodecRequiresAndRestoresOneTaggedSector()
    {
        var source = Enumerable.Range(0, AppleIwmGcrFormat.TaggedSectorByteCount).Select(index => (byte)(index * 29 + 7)).ToArray();
        var encoded = AppleIwmGcrCodec.Encode(source);
        Assert.Equal(AppleIwmGcrFormat.EncodedPayloadSymbolCount + AppleIwmGcrFormat.ChecksumSymbolCount, encoded.Length);
        var symbols = encoded.Take(AppleIwmGcrFormat.EncodedPayloadSymbolCount).Select(value => AppleIIGcrFormat.InverseSixAndTwoTable[value]).ToArray();
        Assert.Equal(source, AppleIwmGcrCodec.Decode(symbols, out _));
        Assert.Throws<ArgumentException>(() => AppleIwmGcrCodec.Encode(source.SkipLast(1).ToArray()));
        Assert.Throws<ArgumentException>(() => AppleIwmGcrCodec.Encode(source.Append((byte)0).ToArray()));
    }

    private static FluxDecodeResult Decode(ITrackEncoder encoder, int cylinder, int head, TrackSector sector) => new FluxDecoderRegistry().Decode(encoder.Id, encoder.Encode(new(cylinder, head, [sector])).Revolution);
    private static TrackSector Sector(IReadOnlyDictionary<string, int>? attributes = null, int number = 3) => new(number, Enumerable.Range(0, AppleIwmGcrFormat.SectorByteCount).Select(index => (byte)(index * 37 + 11)).ToArray(), Attributes: attributes);
    private static bool[] Bits(IEnumerable<byte> bytes) => bytes.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & (0x80 >> bit)) != 0)).ToArray();
    private static int Find(IReadOnlyList<bool> source, IReadOnlyList<bool> pattern) => Enumerable.Range(0, source.Count - pattern.Count + 1).FirstOrDefault(offset => Enumerable.Range(0, pattern.Count).All(index => source[offset + index] == pattern[index]), -1);
    private static byte ReadByte(IReadOnlyList<bool> bits, int offset) => (byte)Enumerable.Range(0, 8).Aggregate(0, (value, bit) => (value << 1) | (bits[offset + bit] ? 1 : 0));
}
