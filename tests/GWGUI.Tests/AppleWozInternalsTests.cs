using System.Text;
using GWGUI.MediaEngine.Containers.Apple.Nib;
using GWGUI.MediaEngine.Containers.Apple.Woz;
using GWGUI.MediaEngine.Decoding.Apple;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.BitPacking;
using GWGUI.MediaEngine.Recognition.Apple;

namespace GWGUI.Tests;

/// <summary>Vérifie les définitions et traitements communs utilisés par les lecteurs WOZ et NIB.</summary>
public sealed class AppleWozInternalsTests
{
    /// <summary>Vérifie que les relations entre champs conservent les positions imposées par WOZ.</summary>
    [Fact]
    public void WozLayoutDerivedValuesMatchTheContainerSpecification()
    {
        Assert.Equal(4, WozLayout.HeaderMarkerOffset);
        Assert.Equal(8, WozLayout.CrcOffset);
        Assert.Equal(12, WozLayout.ChunksOffset);
        Assert.Equal(4, WozLayout.ChunkLengthOffset);
        Assert.Equal(8, WozLayout.ChunkHeaderLength);
        Assert.Equal(160, WozLayout.TrackMapLength);
        Assert.Equal(2, WozLayout.Woz2BlockCountOffset);
        Assert.Equal(4, WozLayout.Woz2BitCountOffset);
        Assert.Equal(8, WozLayout.Woz2TrackDescriptorLength);
    }

    /// <summary>Vérifie le CRC32 WOZ avec le vecteur de contrôle CRC32 standard.</summary>
    [Fact]
    public void WozCrc32MatchesKnownValue() => Assert.Equal(0xcbf43926u, WozCrc32.Compute(Encoding.ASCII.GetBytes("123456789")));

    /// <summary>Vérifie la conversion MSB-first et l'arrondi du nombre d'octets requis.</summary>
    [Fact]
    public void BitPackerUnpacksMostSignificantBitFirstAndRoundsByteCount()
    {
        Assert.Equal(1, MsbFirstBitPacker.RequiredByteCount(1));
        Assert.Equal(1, MsbFirstBitPacker.RequiredByteCount(8));
        Assert.Equal(2, MsbFirstBitPacker.RequiredByteCount(9));
        Assert.Equal([true, false, true, false, false], MsbFirstBitPacker.Unpack([0xa0], 5));
    }

    /// <summary>Vérifie le classement des secteurs Apple II standards selon leur nombre et leur intégrité.</summary>
    [Fact]
    public void SelectorScoresStandardAppleSectors()
    {
        var sectors = Enumerable.Range(0, 2).Select(number => new TrackSector(number, Enumerable.Repeat((byte)number, AppleTrackSelectionRules.StandardSectorSize).ToArray())).ToArray();
        var encoded = new AppleIIGcrTrackEncoder().Encode(new(0, 0, sectors));
        var result = new AppleTrackDecodeSelector().Decode(encoded.Bits.ToArray(), 0);

        Assert.Equal(2, result.StandardSectors.Select(sector => sector.Number).Distinct().Count());
        Assert.All(result.StandardSectors, sector => Assert.True(sector.IntegrityValid));
        Assert.Equal(result.StandardSectors.Select(sector => sector.Number).Distinct().Count() * AppleTrackSelectionRules.DistinctSectorScoreWeight + result.StandardSectors.Count(sector => sector.IntegrityValid == true) * AppleTrackSelectionRules.IntegrityScoreWeight + result.StandardSectors.Count, result.StandardScore);
    }

    /// <summary>Vérifie le classement des secteurs RWTS18 selon leur nombre et leur intégrité.</summary>
    [Fact]
    public void SelectorScoresRwts18Sectors()
    {
        var sectors = Enumerable.Range(0, 2).Select(number => new TrackSector(number, Enumerable.Repeat((byte)number, AppleTrackSelectionRules.Rwts18SectorSize).ToArray())).ToArray();
        var encoded = new AppleRwts18TrackEncoder().Encode(new(0, 0, sectors));
        var result = new AppleTrackDecodeSelector().Decode(encoded.Bits.ToArray(), 0);

        Assert.Equal(2, result.Rwts18Sectors.Select(sector => sector.Number).Distinct().Count());
        Assert.All(result.Rwts18Sectors, sector => Assert.True(sector.IntegrityValid));
        Assert.Equal(result.Rwts18Sectors.Select(sector => sector.Number).Distinct().Count() * AppleTrackSelectionRules.DistinctSectorScoreWeight + result.Rwts18Sectors.Count(sector => sector.IntegrityValid == true) * AppleTrackSelectionRules.IntegrityScoreWeight + result.Rwts18Sectors.Count, result.Rwts18Score);
    }

    /// <summary>Vérifie qu'une seule piste RWTS18 ne suffit pas, alors que deux pistes sélectionnent ce format.</summary>
    [Fact]
    public void Rwts18SelectionRequiresMoreThanOneDecodedTrack()
    {
        Assert.NotEqual("apple2.rwts18", NibTrackImageReader.Read(CreateRwts18Nib(1)).FormatId);
        Assert.Equal("apple2.rwts18", NibTrackImageReader.Read(CreateRwts18Nib(AppleTrackSelectionRules.MinimumCredibleRwts18TrackCount)).FormatId);
    }

    /// <summary>Crée en mémoire le nombre demandé de pistes NIB RWTS18 à partir de l'encodeur du moteur.</summary>
    private static byte[] CreateRwts18Nib(int trackCount)
    {
        var result = new byte[trackCount * NibLayout.TrackLengthBytes];
        for (var track = 0; track < trackCount; track++)
        {
            var sectors = Enumerable.Range(0, 6).Select(number => new TrackSector(number, Enumerable.Repeat((byte)(track + number), AppleTrackSelectionRules.Rwts18SectorSize).ToArray())).ToArray();
            var bits = new AppleRwts18TrackEncoder().Encode(new(track, 0, sectors)).Bits;
            var destination = result.AsSpan(track * NibLayout.TrackLengthBytes, NibLayout.TrackLengthBytes);
            for (var bit = 0; bit < bits.Count; bit++) if (bits[bit]) destination[bit / 8] |= (byte)(1 << (7 - bit % 8));
        }
        return result;
    }
}
