using System.IO;
using System.Text;
using GWGUI.MediaEngine.Containers.Apple.Nib;
using GWGUI.MediaEngine.Containers.Apple.Woz;
using GWGUI.MediaEngine.Decoding.Apple;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.BitPacking;

namespace GWGUI.Tests;

/// <summary>Vérifie les définitions et traitements communs utilisés par les lecteurs WOZ et NIB.</summary>
public sealed class AppleWozInternalsTests
{
    /// <summary>Vérifie les valeurs et les relations entre champs imposées par WOZ.</summary>
    [Fact]
    public void WozLayoutDerivedValuesMatchTheContainerSpecification()
    {
        Assert.Equal("WOZ1"u8.ToArray(), WozFormat.Version1Signature.ToArray());
        Assert.Equal("WOZ2"u8.ToArray(), WozFormat.Version2Signature.ToArray());
        Assert.Equal(new byte[] { 0xff, 0x0a, 0x0d, 0x0a }, WozFormat.HeaderMarker.ToArray());
        Assert.Equal("INFO", WozFormat.InfoChunkId);
        Assert.Equal("TMAP", WozFormat.TrackMapChunkId);
        Assert.Equal("TRKS", WozFormat.TracksChunkId);
        Assert.Equal(1, WozFormat.AppleII525DiskType);
        Assert.Equal(0xedb88320u, WozFormat.Crc32Polynomial);
        Assert.Equal(256, WozLayout.MinimumFileLength);
        Assert.Equal(4, WozLayout.SignatureLength);
        Assert.Equal(4, WozLayout.HeaderMarkerOffset);
        Assert.Equal(4, WozLayout.HeaderMarkerLength);
        Assert.Equal(8, WozLayout.CrcOffset);
        Assert.Equal(4, WozLayout.CrcLength);
        Assert.Equal(12, WozLayout.ChunksOffset);
        Assert.Equal(0, WozLayout.ChunkIdOffset);
        Assert.Equal(4, WozLayout.ChunkIdLength);
        Assert.Equal(4, WozLayout.ChunkLengthOffset);
        Assert.Equal(4, WozLayout.ChunkLengthSize);
        Assert.Equal(8, WozLayout.ChunkHeaderLength);
        Assert.Equal(2, WozLayout.MinimumInfoLength);
        Assert.Equal(1, WozLayout.InfoDiskTypeOffset);
        Assert.Equal(160, WozLayout.TrackMapLength);
        Assert.Equal(40, WozLayout.AppleIITrackCount);
        Assert.Equal(0, WozLayout.FirstAppleIITrackIndex);
        Assert.Equal(4, WozLayout.TrackMapEntriesPerTrack);
        Assert.Equal(0xff, WozLayout.MissingTrackDescriptor);
        Assert.Equal(NibLayout.TrackLengthBytes, WozLayout.Woz1TrackEntryLength);
        Assert.Equal(6648, WozLayout.Woz1BitCountOffset);
        Assert.Equal(2, WozLayout.Woz1BitCountLength);
        Assert.Equal(53184, WozLayout.Woz1MaximumBitCount);
        Assert.Equal(512, WozLayout.Woz2BlockLength);
        Assert.Equal(0, WozLayout.Woz2StartBlockOffset);
        Assert.Equal(0, WozLayout.MissingWoz2StartBlock);
        Assert.Equal(2, WozLayout.Woz2BlockCountOffset);
        Assert.Equal(4, WozLayout.Woz2BitCountOffset);
        Assert.Equal(2, WozLayout.Woz2BlockFieldLength);
        Assert.Equal(4, WozLayout.Woz2BitCountLength);
        Assert.Equal(0, WozLayout.EmptyTrackBitCount);
        Assert.Equal(0, WozLayout.EmptyWoz2BlockCount);
        Assert.Equal(8, WozLayout.Woz2TrackDescriptorLength);
    }

    /// <summary>Vérifie le CRC32 WOZ avec le vecteur de contrôle CRC32 standard.</summary>
    [Fact]
    public void WozCrc32MatchesKnownValue() => Assert.Equal(0xcbf43926u, WozCrc32.Compute(Encoding.ASCII.GetBytes("123456789")));

    /// <summary>Vérifie l'en-tête de chunk et le rejet des identifiants WOZ invalides.</summary>
    [Fact]
    public void WozChunkWriterValidatesAndWritesChunkHeader()
    {
        using var stream = new MemoryStream();
        WozChunkWriter.Write(stream, WozFormat.InfoChunkId, [1, 2, 3]);
        Assert.Equal("INFO", Encoding.ASCII.GetString(stream.ToArray(), 0, 4));
        Assert.Equal(3u, System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(stream.ToArray().AsSpan(4, 4)));
        Assert.Throws<ArgumentException>(() => WozChunkWriter.Write(stream, "BAD", []));
    }

    /// <summary>Vérifie le rejet des collections vides et des pistes trop longues avant toute écriture.</summary>
    [Fact]
    public async Task AppleContainerWritersValidateTracksBeforeWriting()
    {
        await Assert.ThrowsAsync<InvalidDataException>(() => NibWriter.WriteAsync([], "unused.nib"));
        await Assert.ThrowsAsync<InvalidDataException>(() => NibWriter.WriteAsync([new bool[NibLayout.MaximumTrackBitCount + 1]], "unused.nib"));
        await Assert.ThrowsAsync<InvalidDataException>(() => WozWriter.WriteAsync([new bool[WozWriter.MaximumTrackBitCount + 1]], "unused.woz"));
    }

    /// <summary>Vérifie la conversion MSB-first et l'arrondi du nombre d'octets requis.</summary>
    [Fact]
    public void BitPackerUnpacksMostSignificantBitFirstAndRoundsByteCount()
    {
        Assert.Equal(1, MsbFirstBitPacker.RequiredByteCount(1));
        Assert.Equal(1, MsbFirstBitPacker.RequiredByteCount(8));
        Assert.Equal(2, MsbFirstBitPacker.RequiredByteCount(9));
        Assert.Equal([true, false, true, false, false], MsbFirstBitPacker.Unpack([0xa0], 5));
        var packed = new byte[2];
        MsbFirstBitPacker.Pack([true, false, true, false, false, false, false, false, true], packed);
        Assert.Equal([0xa0, 0x80], packed);
        Assert.Throws<ArgumentException>(() => MsbFirstBitPacker.Pack([true, false, true, false, false, false, false, false, true], new byte[1]));
        Assert.Throws<ArgumentOutOfRangeException>(() => MsbFirstBitPacker.Unpack([0x00], 9));
    }

    /// <summary>Vérifie les limites et tailles nommées utilisées pour filtrer les secteurs Apple.</summary>
    [Fact]
    public void AppleTrackSelectionRulesExposeExpectedLimitsAndSizes()
    {
        Assert.Equal((0, 15, 256), (AppleTrackSelectionRules.StandardMinimumSectorNumber, AppleTrackSelectionRules.StandardMaximumSectorNumber, AppleTrackSelectionRules.StandardSectorSize));
        Assert.Equal((0, 5, 768), (AppleTrackSelectionRules.Rwts18MinimumSectorNumber, AppleTrackSelectionRules.Rwts18MaximumSectorNumber, AppleTrackSelectionRules.Rwts18SectorSize));
    }

    /// <summary>Vérifie qu'un conteneur NIB vide est rejeté.</summary>
    [Fact]
    public void NibReaderRejectsEmptyContainer() => Assert.Throws<InvalidDataException>(() => NibReader.Read([]));

    /// <summary>Vérifie le classement des secteurs Apple II standards selon leur nombre et leur intégrité.</summary>
    [Fact]
    public void SelectorScoresStandardAppleSectors()
    {
        var sectors = Enumerable.Range(0, 2).Select(number => new TrackSector(number, Enumerable.Repeat((byte)number, AppleTrackSelectionRules.StandardSectorSize).ToArray())).ToArray();
        var encoded = new AppleIIGcrTrackEncoder().Encode(new(0, 0, sectors, new Dictionary<string, int> { [AppleIIGcrFormat.SectorsPerTrackAttributeName] = AppleIIGcrFormat.SixAndTwoSectorsPerTrack }));
        var result = new AppleTrackDecodeSelector().Decode(encoded.Bits.ToArray(), 0);

        Assert.Equal(2, result.StandardSectors.Select(sector => sector.Number).Distinct().Count());
        Assert.All(result.StandardSectors, sector => Assert.True(sector.IntegrityValid));
        Assert.Equal(result.StandardSectors.Select(sector => sector.Number).Distinct().Count() * AppleTrackSelectionRules.DistinctSectorScoreWeight + result.StandardSectors.Count(sector => sector.IntegrityValid == true) * AppleTrackSelectionRules.IntegrityScoreWeight + result.StandardSectors.Count, result.StandardScore);
    }

    /// <summary>Vérifie le classement des secteurs RWTS18 selon leur nombre et leur intégrité.</summary>
    [Fact]
    public void SelectorScoresRwts18Sectors()
    {
        var sectors = Enumerable.Range(0, AppleTrackSelectionRules.Rwts18MaximumSectorNumber + 1).Select(number => new TrackSector(number, Enumerable.Repeat((byte)number, AppleTrackSelectionRules.Rwts18SectorSize).ToArray())).ToArray();
        var encoded = new AppleRwts18TrackEncoder().Encode(new(0, 0, sectors));
        var result = new AppleTrackDecodeSelector().Decode(encoded.Bits.ToArray(), 0);

        Assert.Equal(AppleTrackSelectionRules.Rwts18MaximumSectorNumber + 1, result.Rwts18Sectors.Select(sector => sector.Number).Distinct().Count());
        Assert.All(result.Rwts18Sectors, sector => Assert.True(sector.IntegrityValid));
        Assert.Equal(result.Rwts18Sectors.Select(sector => sector.Number).Distinct().Count() * AppleTrackSelectionRules.DistinctSectorScoreWeight + result.Rwts18Sectors.Count(sector => sector.IntegrityValid == true) * AppleTrackSelectionRules.IntegrityScoreWeight + result.Rwts18Sectors.Count, result.Rwts18Score);
    }

    /// <summary>Vérifie qu'une seule piste RWTS18 ne suffit pas, alors que deux pistes sélectionnent ce format.</summary>
    [Fact]
    public void Rwts18SelectionRequiresMoreThanOneDecodedTrack()
    {
        Assert.NotEqual("apple2.rwts18", NibReader.Read(CreateRwts18Nib(1)).FormatId);
        Assert.Equal("apple2.rwts18", NibReader.Read(CreateRwts18Nib(AppleTrackSelectionRules.MinimumCredibleRwts18TrackCount)).FormatId);
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
