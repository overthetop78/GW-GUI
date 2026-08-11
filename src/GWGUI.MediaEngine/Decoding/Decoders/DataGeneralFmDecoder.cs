using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Data General 2F.</summary>
public sealed class DataGeneralFmDecoder : IFluxDecoder
{
    private static readonly byte[] Sync = DataGeneralFmFormat.Sync.ToArray();

    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => DataGeneralFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => DataGeneralFmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution de flux à décoder.</param>
    /// <returns>Résultat du décodage Data General 2F.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var syncOffsets = FindAll(stream, Sync);
        for (var index = 0; index + 1 < syncOffsets.Count; index++)
        {
            if (!TryDecodePair(stream, syncOffsets[index], syncOffsets[index + 1], out var decoded)) continue;
            bytes.AddRange(decoded.Payload);
            sectors.Add(new(decoded.Identity.Cylinder, decoded.Identity.Head, decoded.Identity.Sector, DataGeneralFmFormat.SectorSizeCode, DataGeneralFmFormat.SectorSize, decoded.ChecksumValid, syncOffsets[index], SectorIntegrityKind.Checksum, decoded.Payload));
            structures.Add(new(FluxStructureKind.FormatHeader, syncOffsets[index], DataGeneralFmFormat.EncodedSyncBitCount + DataGeneralFmFormat.IdentityByteCount * DataGeneralFmFormat.EncodedByteBitCount, DataGeneralFmDescriptions.Header(decoded.Identity)));
            structures.Add(new(FluxStructureKind.FormatData, syncOffsets[index + 1], DataGeneralFmFormat.EncodedSyncBitCount + DataGeneralFmFormat.DataBlockByteCount * DataGeneralFmFormat.EncodedByteBitCount, DataGeneralFmDescriptions.Data(decoded.Identity, decoded.ChecksumValid)));
            index++;
        }
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, DataGeneralFmFormat.ConfidenceSectorWeight, DataGeneralFmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Identifie et décode une paire de synchronisations d'en-tête et de données.</summary>
    /// <param name="stream">Flux source.</param>
    /// <param name="headerOffset">Position de la synchronisation d'en-tête.</param>
    /// <param name="dataOffset">Position de la synchronisation des données.</param>
    /// <param name="decoded">Secteur décodé.</param>
    /// <returns><see langword="true"/> si la paire est complète et valide structurellement.</returns>
    private static bool TryDecodePair(FluxBitstream stream, int headerOffset, int dataOffset, out DataGeneralDecodedSector decoded)
    {
        decoded = null!;
        var headerStart = headerOffset + DataGeneralFmFormat.EncodedSyncBitCount;
        var distance = dataOffset - headerStart;
        if (distance < DataGeneralFmFormat.MinimumDataSyncDistanceBits || distance > DataGeneralFmFormat.MaximumDataSyncDistanceBits) return false;
        if (!TryDecodeIdentity(stream, headerStart, out var identity)) return false;
        if (identity.Sector > DataGeneralFmFormat.MaximumSectorNumber) return false;
        var dataStart = dataOffset + DataGeneralFmFormat.EncodedSyncBitCount;
        var block = TryDecodeMfmBytes(stream, dataStart, DataGeneralFmFormat.DataBlockByteCount);
        if (block is null) return false;
        decoded = DecodeData(identity, block);
        return true;
    }

    /// <summary>Décode les deux octets d'identité.</summary>
    /// <param name="stream">Flux source.</param>
    /// <param name="offset">Position du premier octet.</param>
    /// <param name="identity">Identité décodée.</param>
    /// <returns><see langword="true"/> lorsque les deux octets sont disponibles.</returns>
    private static bool TryDecodeIdentity(FluxBitstream stream, int offset, out DataGeneralSectorIdentity identity)
    {
        identity = null!;
        if (!FluxBitReader.TryDecodeMfmByte(stream, offset + DataGeneralFmFormat.CylinderAndHeadOffset * DataGeneralFmFormat.EncodedByteBitCount, out var cylinderAndHead)) return false;
        if (!FluxBitReader.TryDecodeMfmByte(stream, offset + DataGeneralFmFormat.SectorOffset * DataGeneralFmFormat.EncodedByteBitCount, out var sectorByte)) return false;
        identity = new((byte)(cylinderAndHead & DataGeneralFmFormat.CylinderMask), (byte)((cylinderAndHead & DataGeneralFmFormat.HeadMask) >> DataGeneralFmFormat.HeadShift), (byte)(sectorByte >> DataGeneralFmFormat.SectorShift));
        return true;
    }

    /// <summary>Extrait la charge utile et valide son checksum.</summary>
    /// <param name="identity">Identité du secteur.</param>
    /// <param name="block">Bloc de données complet.</param>
    /// <returns>Secteur décodé.</returns>
    private static DataGeneralDecodedSector DecodeData(DataGeneralSectorIdentity identity, IReadOnlyList<byte> block)
    {
        var payload = block.Take(DataGeneralFmFormat.SectorSize).ToArray();
        var storedChecksum = (ushort)((block[DataGeneralFmFormat.ChecksumHighByteOffset] << BitPrimitives.BitsPerByte) | block[DataGeneralFmFormat.ChecksumLowByteOffset]);
        return new(identity, payload, DataGeneralChecksum.Calculate(payload) == storedChecksum);
    }

    /// <summary>Recherche toutes les occurrences du motif dans le flux.</summary>
    /// <param name="stream">Flux source.</param>
    /// <param name="pattern">Motif recherché.</param>
    /// <returns>Positions trouvées, en bits.</returns>
    private static List<int> FindAll(FluxBitstream stream, IReadOnlyList<byte> pattern)
    {
        var offsets = new List<int>();
        for (var offset = 0; offset + pattern.Count * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++)
        {
            if (FluxBitReader.MatchBytes(stream, offset, pattern)) offsets.Add(offset);
        }
        return offsets;
    }

    /// <summary>Tente de décoder une suite d'octets FM.</summary>
    /// <param name="stream">Flux source.</param>
    /// <param name="offset">Position du premier octet.</param>
    /// <param name="count">Nombre d'octets.</param>
    /// <returns>Octets décodés, ou valeur nulle si le bloc est tronqué.</returns>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * DataGeneralFmFormat.EncodedByteBitCount, out result[index])) return null;
        }
        return result;
    }

    /// <summary>Regroupe l'identité, la charge utile et l'état du checksum d'un secteur.</summary>
    /// <param name="Identity">Identité du secteur.</param>
    /// <param name="Payload">Charge utile.</param>
    /// <param name="ChecksumValid">Validité du checksum.</param>
    private sealed record DataGeneralDecodedSector(DataGeneralSectorIdentity Identity, byte[] Payload, bool ChecksumValid);
}
