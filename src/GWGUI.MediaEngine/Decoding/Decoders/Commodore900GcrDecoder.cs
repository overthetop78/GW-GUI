using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les secteurs GCR zonés de 512 octets utilisés par le Commodore 900.</summary>
public sealed class Commodore900GcrDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.Commodore900Gcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.Commodore900Gcr;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder en GCR Commodore 900.</param><returns>Résultat contenant les structures, secteurs et octets reconnus.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var decodedBytes = new List<byte>();
        var headers = new List<(int Offset, int End, byte[] Bytes)>();
        var dataBlocks = new List<(int Offset, int End, byte[] Bytes)>();

        for (var offset = 0; offset < stream.Bits.Length; offset++)
        {
            if (!stream.Bits[offset]) continue;
            var end = offset;
            while (end < stream.Bits.Length && stream.Bits[end]) end++;
            if (end - offset < Commodore900GcrFormat.MinimumSyncBitCount) { offset = end; continue; }
            structures.Add(new(FluxStructureKind.CommodoreSync, offset, end - offset, FluxStructureDescriptions.UnclassifiedMark("Commodore 900", FluxStructureKind.CommodoreSync, null, "GCR sync")));
            if (TryDecodeBytes(stream.Bits, end, Commodore900GcrFormat.HeaderByteCount) is { } header && header[0] == Commodore900GcrFormat.HeaderMark)
            {
                headers.Add((offset, end + Commodore900GcrFormat.HeaderByteCount * Commodore900GcrFormat.EncodedByteBitCount, header)); decodedBytes.AddRange(header);
            }
            else if (TryDecodeBytes(stream.Bits, end, Commodore900GcrFormat.DataRecordByteCount) is { } data && data[0] == Commodore900GcrFormat.DataMark)
            {
                dataBlocks.Add((offset, end + Commodore900GcrFormat.DataRecordByteCount * Commodore900GcrFormat.EncodedByteBitCount, data)); decodedBytes.AddRange(data);
            }
            offset = end;
        }

        var sectors = new List<DecodedSector>();
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index];
            var cylinder = header.Bytes[1]; var number = header.Bytes[2];
            var headerValid = (byte)(header.Bytes[0] ^ cylinder ^ number ^ header.Bytes[3]) == 0;
            var next = index + 1 < headers.Count ? headers[index + 1].Offset : int.MaxValue;
            var data = dataBlocks.FirstOrDefault(candidate => candidate.Offset > header.End && candidate.Offset < next);
            var dataValid = data.Bytes is not null && data.Bytes.Aggregate((byte)0, (checksum, value) => (byte)(checksum ^ value)) == 0;
            var payload = data.Bytes?.Skip(1).Take(Commodore900GcrFormat.SectorByteCount).ToArray();
            var valid = !headerValid || data.Bytes is null || !dataValid ? false : true;
            sectors.Add(new(cylinder, 0, number, Commodore900GcrFormat.SectorSizeCode, Commodore900GcrFormat.SectorByteCount, valid, header.Offset, SectorIntegrityKind.Checksum, payload));
            structures.Add(new(FluxStructureKind.CommodoreHeader, header.Offset, Math.Max(Commodore900GcrFormat.MinimumSyncBitCount, header.End - header.Offset),
                FluxStructureDescriptions.Identity("Commodore 900", FluxStructureKind.CommodoreHeader, cylinder, 0, number, Commodore900GcrFormat.SectorByteCount, null, null)));
            if (data.Bytes is not null)
                structures.Add(new(FluxStructureKind.FormatData, data.Offset, Math.Max(Commodore900GcrFormat.MinimumSyncBitCount, data.End - data.Offset),
                    $"{FluxStructureDescriptions.Identity("Commodore 900", FluxStructureKind.FormatData, cylinder, 0, number, Commodore900GcrFormat.SectorByteCount, null, null)}, {FluxStructureDescriptions.Integrity("checksum", dataValid)}"));
        }
        var validCount = sectors.Count(sector => sector.IntegrityValid == true);
        return new(Id, DisplayName, Math.Min(1, validCount / (double)Commodore900GcrFormat.ExpectedSectorCount), stream.BitCellTicks, structures, decodedBytes, sectors);
    }

    /// <summary>Tente de décoder une suite d'octets du format.</summary>
    /// <param name="bits">Bits GCR source.</param><param name="offset">Offset de départ en bits.</param><param name="count">Nombre d'octets à décoder.</param><returns>Octets décodés, ou <see langword="null"/> si un symbole est incomplet ou invalide.</returns>
    private static byte[]? TryDecodeBytes(IReadOnlyList<bool> bits, int offset, int count)
    {
        if (offset + count * Commodore900GcrFormat.EncodedByteBitCount > bits.Count) return null;
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            if (!TryNibble(bits, offset + index * Commodore900GcrFormat.EncodedByteBitCount, out var high) ||
                !TryNibble(bits, offset + index * Commodore900GcrFormat.EncodedByteBitCount + Commodore900GcrFormat.EncodedNibbleBitCount, out var low)) return null;
            result[index] = (byte)((high << 4) | low);
        }
        return result;
    }

    /// <summary>Exécute le traitement « Try Nibble » propre à ce format.</summary>
    /// <param name="bits">Bits GCR source.</param><param name="offset">Offset du symbole en bits.</param><param name="value">Demi-octet décodé.</param><returns><see langword="true"/> si le symbole est complet et reconnu.</returns>
    private static bool TryNibble(IReadOnlyList<bool> bits, int offset, out int value)
    {
        var code = 0; value = 0;
        for (var bit = 0; bit < Commodore900GcrFormat.EncodedNibbleBitCount; bit++) code = (code << 1) | (bits[offset + bit] ? 1 : 0);
        return Commodore900GcrFormat.DecodingTable.TryGetValue(code, out value);
    }
}
