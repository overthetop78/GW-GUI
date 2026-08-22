using GWGUI.MediaEngine.Decoding;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.FileSystems.Apple.Macintosh.Hfs;

namespace GWGUI.Tests;

/// <summary>Vérifie la sélection, les limites et les codecs de l'encodeur de pistes Apple II.</summary>
public sealed class AppleIIGcrTrackEncoderTests
{
    /// <summary>Vérifie que les pistes complètes sélectionnent automatiquement les deux formats et se redécodent sans perte.</summary>
    [Theory]
    [InlineData(AppleIIGcrFormat.FiveAndThreeSectorsPerTrack)]
    [InlineData(AppleIIGcrFormat.SixAndTwoSectorsPerTrack)]
    public void CompleteTracksInferTheirFormatAndRoundTrip(int sectorCount)
    {
        var sectors = Sectors(sectorCount);
        var encoded = new FluxEncoderRegistry().Encode(FluxCodecIds.AppleIIGcr, new TrackEncodeRequest(12, 0, sectors));
        var decoded = new FluxDecoderRegistry().Decode(FluxCodecIds.AppleIIGcr, encoded.Revolution);
        Assert.Equal(sectorCount, decoded.Sectors!.Count);
        Assert.All(sectors, expected => Assert.Equal(expected.Data, Assert.Single(decoded.Sectors, actual => actual.Number == expected.Number).Data));
    }

    /// <summary>Vérifie qu'une piste partielle exige l'indication explicite du format et n'accepte que treize ou seize secteurs.</summary>
    [Fact]
    public void PartialTracksRequireAValidExplicitFormat()
    {
        var encoder = new AppleIIGcrTrackEncoder();
        var sectors = Sectors(2);
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, sectors)));
        Assert.NotEmpty(encoder.Encode(new(0, 0, sectors, Format(AppleIIGcrFormat.FiveAndThreeSectorsPerTrack))).Bits);
        Assert.NotEmpty(encoder.Encode(new(0, 0, sectors, Format(AppleIIGcrFormat.SixAndTwoSectorsPerTrack))).Bits);
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, sectors, Format(14))));
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, Sectors(AppleIIGcrFormat.FiveAndThreeSectorsPerTrack), Format(AppleIIGcrFormat.SixAndTwoSectorsPerTrack))));
    }

    /// <summary>Vérifie les limites du volume, du cylindre, du secteur et de la charge utile.</summary>
    [Fact]
    public void EncoderRejectsUnrepresentableAddressesAndInvalidPayloads()
    {
        var encoder = new AppleIIGcrTrackEncoder();
        Assert.NotEmpty(encoder.Encode(new(byte.MaxValue, 0, [Sector(byte.MaxValue)], Format(AppleIIGcrFormat.SixAndTwoSectorsPerTrack, byte.MaxValue))).Bits);
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [Sector(0)], Format(AppleIIGcrFormat.SixAndTwoSectorsPerTrack, -1))));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [Sector(0)], Format(AppleIIGcrFormat.SixAndTwoSectorsPerTrack, byte.MaxValue + 1))));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [Sector(-1)], Format(AppleIIGcrFormat.SixAndTwoSectorsPerTrack))));
        Assert.Throws<ArgumentOutOfRangeException>(() => encoder.Encode(new(0, 0, [Sector(byte.MaxValue + 1)], Format(AppleIIGcrFormat.SixAndTwoSectorsPerTrack))));
        Assert.Throws<ArgumentException>(() => encoder.Encode(new(0, 0, [new TrackSector(0, new byte[AppleIIGcrFormat.SectorSize - 1])], Format(AppleIIGcrFormat.SixAndTwoSectorsPerTrack))));
    }

    /// <summary>Vérifie les longueurs, contrôles et allers-retours des primitives Apple II communes.</summary>
    [Fact]
    public void CommonCodecsProduceExpectedAddressesAndPayloads()
    {
        const byte volume = 254;
        const byte cylinder = 17;
        const byte sector = 9;
        var address = AppleIIGcrCodec.EncodeAddress(volume, cylinder, sector);
        Assert.Equal(AppleIIGcrFormat.EncodedAddressByteCount, address.Length);
        Assert.Equal([volume, cylinder, sector, (byte)(volume ^ cylinder ^ sector)], Enumerable.Range(0, AppleIIGcrFormat.AddressValueCount).Select(index => AppleIIGcrCodec.DecodeFourAndFour(address[index * 2], address[index * 2 + 1])).ToArray());
        var source = Sector(0).Data;
        var sixAndTwo = AppleIIGcrCodec.EncodeSixAndTwo(source);
        var fiveAndThree = AppleIIGcrCodec.EncodeFiveAndThree(source);
        Assert.Equal(AppleIIGcrFormat.SixAndTwoEncodedByteCount, sixAndTwo.Length);
        Assert.Equal(AppleIIGcrFormat.FiveAndThreeEncodedByteCount, fiveAndThree.Length);
        Assert.Equal(source, AppleIIGcrCodec.TryDecodeSixAndTwo(Bits(sixAndTwo), 0)!.Value.Data);
        Assert.Equal(source, AppleIIGcrCodec.TryDecodeFiveAndThree(Bits(fiveAndThree), 0)!.Value.Data);
        Assert.Throws<ArgumentException>(() => AppleIIGcrCodec.EncodeSixAndTwo(new byte[AppleIIGcrFormat.SectorSize - 1]));
        Assert.Throws<ArgumentException>(() => AppleIIGcrCodec.EncodeFiveAndThree(new byte[AppleIIGcrFormat.SectorSize + 1]));
    }

    /// <summary>Vérifie que chaque variante écrit ses prologues et l'épilogue communs.</summary>
    [Theory]
    [InlineData(AppleIIGcrFormat.FiveAndThreeSectorsPerTrack)]
    [InlineData(AppleIIGcrFormat.SixAndTwoSectorsPerTrack)]
    public void EncodedTracksContainTheirNamedMarkers(int sectorsPerTrack)
    {
        var bits = new AppleIIGcrTrackEncoder().Encode(new(0, 0, [Sector(0)], Format(sectorsPerTrack))).Bits;
        var addressPrologue = sectorsPerTrack == AppleIIGcrFormat.FiveAndThreeSectorsPerTrack ? AppleIIGcrFormat.FiveAndThreeAddressPrologueBytes : AppleIIGcrFormat.SixAndTwoAddressPrologueBytes;
        Assert.True(Contains(bits, Bits(addressPrologue)));
        Assert.True(Contains(bits, Bits(AppleIIGcrFormat.AddressToDataSeparatorBytes)));
        Assert.True(Contains(bits, Bits(AppleIIGcrFormat.EpilogueBytes)));
    }

    private static IReadOnlyDictionary<string, int> Format(int sectorsPerTrack, int volume = AppleIIGcrFormat.DefaultVolume) => new Dictionary<string, int> { [AppleIIGcrFormat.SectorsPerTrackAttributeName] = sectorsPerTrack, [AppleIIGcrFormat.VolumeAttributeName] = volume };
    private static TrackSector[] Sectors(int count) => Enumerable.Range(0, count).Select(Sector).ToArray();
    private static TrackSector Sector(int number) => new(number, Enumerable.Range(0, AppleIIGcrFormat.SectorSize).Select(index => (byte)(number * 19 + index * 31)).ToArray());
    private static bool[] Bits(IEnumerable<byte> bytes) => bytes.SelectMany(value => Enumerable.Range(0, 8).Select(bit => (value & (0x80 >> bit)) != 0)).ToArray();
    private static bool Contains(IReadOnlyList<bool> source, IReadOnlyList<bool> pattern) => Enumerable.Range(0, source.Count - pattern.Count + 1).Any(offset => Enumerable.Range(0, pattern.Count).All(index => source[offset + index] == pattern[index]));
}
