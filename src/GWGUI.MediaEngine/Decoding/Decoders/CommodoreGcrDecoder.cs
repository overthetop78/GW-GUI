using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Commodore GCR.</summary>
public sealed class CommodoreGcrDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => CommodoreGcrFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => CommodoreGcrFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution de flux à décoder en GCR Commodore.</param>
    /// <returns>Résultat contenant les structures, secteurs et octets reconnus.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals);
        var collection = CollectBlocks(stream);
        var pairing = PairBlocks(collection);
        var structures = collection.Structures.Concat(pairing.Structures).OrderBy(structure => structure.BitOffset).ToArray();
        var confidence = FluxDecoderConfidence.Calculate(pairing.Sectors.Count, structures.Length, CommodoreGcrFormat.ConfidenceSectorWeight, CommodoreGcrFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, collection.DecodedBytes, pairing.Sectors);
    }

    /// <summary>Balaye les synchronisations et collecte les en-têtes et blocs de données reconnus.</summary>
    /// <param name="stream">Flux binaire GCR.</param>
    /// <returns>Blocs, synchronisations et octets décodés.</returns>
    private static CommodoreGcrBlockCollection CollectBlocks(FluxBitstream stream)
    {
        var headers = new List<CommodoreGcrHeaderBlock>();
        var dataBlocks = new List<CommodoreGcrDataBlock>();
        var structures = new List<FluxStructure>();
        var decodedBytes = new List<byte>();
        for (var offset = 0; offset < stream.Bits.Length; offset++)
        {
            if (!stream.Bits[offset]) continue;
            var end = offset;
            while (end < stream.Bits.Length && stream.Bits[end]) end++;
            var length = end - offset;
            if (length < CommodoreGcrFormat.MinimumSyncBitCount)
            {
                offset = end;
                continue;
            }
            structures.Add(new(FluxStructureKind.CommodoreSync, offset, length, CommodoreGcrDescriptions.Sync()));
            if (CommodoreGcrCodec.TryDecodeByte(stream.Bits, end, out var mark))
            {
                if (mark == CommodoreGcrFormat.HeaderMark) CollectHeader(stream.Bits, offset, end, headers, decodedBytes);
                else if (mark == CommodoreGcrFormat.DataMark) CollectData(stream.Bits, offset, end, dataBlocks, decodedBytes);
                else decodedBytes.Add(mark);
            }
            offset = end;
        }
        return new(headers, dataBlocks, structures, decodedBytes);
    }

    /// <summary>Décode et conserve un en-tête rencontré après une synchronisation.</summary>
    /// <param name="bits">Bits de la piste.</param>
    /// <param name="syncOffset">Position de la synchronisation.</param>
    /// <param name="dataOffset">Position du premier symbole GCR.</param>
    /// <param name="headers">Collection recevant l'en-tête.</param>
    /// <param name="decodedBytes">Collection recevant les octets décodés.</param>
    private static void CollectHeader(IReadOnlyList<bool> bits, int syncOffset, int dataOffset, ICollection<CommodoreGcrHeaderBlock> headers, List<byte> decodedBytes)
    {
        var decoded = CommodoreGcrCodec.TryDecodeBytes(bits, dataOffset, CommodoreGcrFormat.HeaderByteCount);
        if (decoded is null)
        {
            decodedBytes.Add(CommodoreGcrFormat.HeaderMark);
            headers.Add(new(syncOffset, dataOffset + CommodoreGcrFormat.EncodedByteBitCount, null, 0, 0, 0, 0, null));
            return;
        }
        decodedBytes.AddRange(decoded);
        var headerValid = decoded[CommodoreGcrFormat.HeaderMarkOffset] == CommodoreGcrFormat.HeaderMark && CommodoreGcrChecksum.IsValid(decoded.Skip(CommodoreGcrFormat.HeaderChecksumOffset));
        headers.Add(new(syncOffset, dataOffset + CommodoreGcrFormat.EncodedHeaderBitCount, decoded, decoded[CommodoreGcrFormat.HeaderTrackOffset], decoded[CommodoreGcrFormat.HeaderSectorOffset], decoded[CommodoreGcrFormat.HeaderDiskId2Offset], decoded[CommodoreGcrFormat.HeaderDiskId1Offset], headerValid));
    }

    /// <summary>Décode et conserve un bloc de données rencontré après une synchronisation.</summary>
    /// <param name="bits">Bits de la piste.</param>
    /// <param name="syncOffset">Position de la synchronisation.</param>
    /// <param name="dataOffset">Position du premier symbole GCR.</param>
    /// <param name="dataBlocks">Collection recevant le bloc.</param>
    /// <param name="decodedBytes">Collection recevant les octets décodés.</param>
    private static void CollectData(IReadOnlyList<bool> bits, int syncOffset, int dataOffset, ICollection<CommodoreGcrDataBlock> dataBlocks, List<byte> decodedBytes)
    {
        var decoded = CommodoreGcrCodec.TryDecodeBytes(bits, dataOffset, CommodoreGcrFormat.DataRecordByteCount);
        if (decoded is null)
        {
            decodedBytes.Add(CommodoreGcrFormat.DataMark);
            dataBlocks.Add(new(syncOffset, dataOffset + CommodoreGcrFormat.EncodedByteBitCount, null, null));
            return;
        }
        decodedBytes.AddRange(decoded);
        dataBlocks.Add(new(syncOffset, dataOffset + CommodoreGcrFormat.EncodedDataRecordBitCount, decoded, ValidateData(decoded)));
    }

    /// <summary>Valide la marque et le checksum d'un bloc de données complet.</summary>
    /// <param name="data">Bloc de données décodé.</param>
    /// <returns><see langword="true"/> si la marque et le checksum sont valides.</returns>
    private static bool ValidateData(IReadOnlyList<byte> data) => data[CommodoreGcrFormat.DataMarkOffset] == CommodoreGcrFormat.DataMark && CommodoreGcrChecksum.IsValid(data.Skip(CommodoreGcrFormat.DataPayloadOffset));

    /// <summary>Apparie chaque en-tête au premier bloc de données précédant l'en-tête suivant.</summary>
    /// <param name="collection">Blocs collectés pendant le balayage.</param>
    /// <returns>Secteurs et structures produits par l'appariement.</returns>
    private static CommodoreGcrPairingResult PairBlocks(CommodoreGcrBlockCollection collection)
    {
        var sectors = new List<DecodedSector>();
        var structures = collection.DataBlocks.Select(block => new FluxStructure(FluxStructureKind.FormatData, block.SyncOffset, Math.Max(CommodoreGcrFormat.MinimumSyncBitCount, block.EndOffset - block.SyncOffset), CommodoreGcrDescriptions.Data(block.ChecksumValid))).ToList();
        for (var index = 0; index < collection.Headers.Count; index++)
        {
            var header = collection.Headers[index];
            var nextHeaderOffset = index + 1 < collection.Headers.Count ? collection.Headers[index + 1].SyncOffset : int.MaxValue;
            var data = collection.DataBlocks.FirstOrDefault(candidate => candidate.SyncOffset > header.EndOffset && candidate.SyncOffset < nextHeaderOffset);
            bool? dataValid = data?.ChecksumValid;
            bool? integrity = header.ChecksumValid == false || dataValid == false ? false : dataValid is null ? null : true;
            var payload = data?.Bytes?.Skip(CommodoreGcrFormat.DataPayloadOffset).Take(CommodoreGcrFormat.SectorByteCount).ToArray();
            sectors.Add(new(header.Track, CommodoreGcrFormat.LogicalHead, header.Sector, CommodoreGcrFormat.SectorSizeCode, CommodoreGcrFormat.SectorByteCount, integrity, header.SyncOffset, SectorIntegrityKind.Checksum, payload));
            structures.Add(new(FluxStructureKind.CommodoreHeader, header.SyncOffset, Math.Max(CommodoreGcrFormat.MinimumSyncBitCount, header.EndOffset - header.SyncOffset), CommodoreGcrDescriptions.Header(header.Track, header.Sector, header.ChecksumValid, dataValid)));
        }
        return new(sectors, structures);
    }

    /// <summary>Représente un en-tête Commodore GCR décodé.</summary>
    /// <param name="SyncOffset">Position de la synchronisation.</param>
    /// <param name="EndOffset">Position suivant l'en-tête.</param>
    /// <param name="Bytes">Octets décodés, ou valeur nulle si l'en-tête est tronqué.</param>
    /// <param name="Track">Piste.</param>
    /// <param name="Sector">Secteur.</param>
    /// <param name="DiskId2">Second identifiant de disque.</param>
    /// <param name="DiskId1">Premier identifiant de disque.</param>
    /// <param name="ChecksumValid">Validité du checksum.</param>
    private sealed record CommodoreGcrHeaderBlock(int SyncOffset, int EndOffset, byte[]? Bytes, byte Track, byte Sector, byte DiskId2, byte DiskId1, bool? ChecksumValid);
    /// <summary>Représente un bloc de données Commodore GCR décodé.</summary>
    /// <param name="SyncOffset">Position de la synchronisation.</param>
    /// <param name="EndOffset">Position suivant le bloc.</param>
    /// <param name="Bytes">Octets décodés, ou valeur nulle si le bloc est tronqué.</param>
    /// <param name="ChecksumValid">Validité du checksum.</param>
    private sealed record CommodoreGcrDataBlock(int SyncOffset, int EndOffset, byte[]? Bytes, bool? ChecksumValid);
    /// <summary>Regroupe les éléments collectés pendant le balayage.</summary>
    /// <param name="Headers">En-têtes.</param>
    /// <param name="DataBlocks">Blocs de données.</param>
    /// <param name="Structures">Synchronisations reconnues.</param>
    /// <param name="DecodedBytes">Octets décodés.</param>
    private sealed record CommodoreGcrBlockCollection(IReadOnlyList<CommodoreGcrHeaderBlock> Headers, IReadOnlyList<CommodoreGcrDataBlock> DataBlocks, IReadOnlyList<FluxStructure> Structures, IReadOnlyList<byte> DecodedBytes);
    /// <summary>Regroupe les secteurs et structures produits par l'appariement.</summary>
    /// <param name="Sectors">Secteurs reconstruits.</param>
    /// <param name="Structures">Structures produites.</param>
    private sealed record CommodoreGcrPairingResult(IReadOnlyList<DecodedSector> Sectors, IReadOnlyList<FluxStructure> Structures);
}
