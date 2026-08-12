using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;

namespace GWGUI.Tests;

/// <summary>Vérifie les deux chemins de l'encodeur de blocs Arburg.</summary>
public sealed class ArburgTrackEncoderTests
{
    /// <summary>Vérifie les blocs utiles et complets, leur marque, leur checksum et leur aller-retour.</summary>
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void UsefulAndCompleteBlocksRoundTrip(bool system, bool complete)
    {
        var definition = ArburgFormat.Definition(system);
        var useful = Enumerable.Range(0, definition.UsefulSize).Select(index => (byte)(index * (system ? 5 : 7))).ToArray();
        var input = complete ? ArburgChecksum.CreateBlock(useful, definition.TotalSize) : useful;
        if (complete) input[^1] = 0xff;
        var attributes = system ? new Dictionary<string, int> { [ArburgFormat.SystemAttribute] = 1 } : null;
        var encoded = new ArburgTrackEncoder().Encode(new(0, 0, [new TrackSector(1, input, Attributes: attributes)]));
        Assert.True(StartsAt(encoded.Bits, definition.Mark));
        Assert.All(encoded.Bits.TakeLast(ArburgFormat.GapBitCount), Assert.True);
        var decoded = new ArburgDecoder().Decode(encoded.Revolution);
        var sector = Assert.Single(decoded.Sectors!);
        Assert.True(sector.IntegrityValid);
        Assert.Equal(useful, sector.Data);
    }

    /// <summary>Vérifie que chaque type rejette une longueur différente de ses tailles utile et complète.</summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void EncoderRejectsUnsupportedPayloadSize(bool system)
    {
        var definition = ArburgFormat.Definition(system);
        var attributes = system ? new Dictionary<string, int> { [ArburgFormat.SystemAttribute] = 1 } : null;
        Assert.Throws<ArgumentException>(() => new ArburgTrackEncoder().Encode(new(0, 0, [new TrackSector(1, new byte[definition.UsefulSize - 1], Attributes: attributes)])));
    }

    private static bool StartsAt(IReadOnlyList<bool> bits, IReadOnlyList<byte> mark) => Bits(mark).Select((value, index) => bits[index] == value).All(equal => equal);
    private static bool[] Bits(IEnumerable<byte> bytes) => bytes.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & (0x80 >> bit)) != 0)).ToArray();
}
