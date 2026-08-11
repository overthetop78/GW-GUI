using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Commodore GCR.</summary>
public sealed class CommodoreGcrDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.CommodoreGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.CommodoreGcr;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder en GCR Commodore.</param><returns>Résultat contenant les structures, secteurs et octets reconnus.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var bytes = new List<byte>(); var sectors = new List<DecodedSector>();
        var headers = new List<(int SyncOffset, int DataOffset, int EndOffset, byte[]? Bytes)>(); var dataBlocks = new List<(int SyncOffset, int DataOffset, int EndOffset, byte[]? Bytes)>();
        for (var offset = 0; offset < stream.Bits.Length; offset++)
        {
            if (!stream.Bits[offset]) continue; var end = offset; while (end < stream.Bits.Length && stream.Bits[end]) end++;
            var length = end - offset;
            if (length >= CommodoreGcrFormat.MinimumSyncBitCount)
            {
            structures.Add(new(FluxStructureKind.CommodoreSync, offset, length, FluxStructureDescriptions.UnclassifiedMark("Commodore", FluxStructureKind.CommodoreSync, null, "GCR sync")));
                if (CommodoreGcrCodec.TryDecodeByte(stream.Bits, end, out var value))
                {
                    if (value == CommodoreGcrFormat.HeaderMark) { var decoded = CommodoreGcrCodec.TryDecodeBytes(stream.Bits, end, CommodoreGcrFormat.HeaderByteCount); headers.Add((offset, end, decoded is null ? end + CommodoreGcrFormat.EncodedByteBitCount : end + CommodoreGcrFormat.HeaderByteCount * CommodoreGcrFormat.EncodedByteBitCount, decoded)); if (decoded is not null) bytes.AddRange(decoded); else bytes.Add(value); }
                    else if (value == CommodoreGcrFormat.DataMark) { var decoded = CommodoreGcrCodec.TryDecodeBytes(stream.Bits, end, CommodoreGcrFormat.DataRecordByteCount); dataBlocks.Add((offset, end, decoded is null ? end + CommodoreGcrFormat.EncodedByteBitCount : end + CommodoreGcrFormat.DataRecordByteCount * CommodoreGcrFormat.EncodedByteBitCount, decoded)); if (decoded is not null) bytes.AddRange(decoded); else bytes.Add(value); }
                    else bytes.Add(value);
                }
            }
            offset = end;
        }
        foreach (var block in dataBlocks)
        {
            bool? valid = null;
            if (block.Bytes is not null) { byte checksum = 0; for (var index = 1; index < CommodoreGcrFormat.DataRecordByteCount; index++) checksum ^= block.Bytes[index]; valid = checksum == 0; }
            structures.Add(new(FluxStructureKind.FormatData, block.SyncOffset, Math.Max(CommodoreGcrFormat.MinimumSyncBitCount, block.EndOffset - block.SyncOffset), $"{FluxStructureDescriptions.Identity("Commodore", FluxStructureKind.FormatData, 0, 0, 0, CommodoreGcrFormat.SectorByteCount, null, "data block")}, {FluxStructureDescriptions.Integrity("checksum", valid)}"));
        }
        for (var headerIndex = 0; headerIndex < headers.Count; headerIndex++)
        {
            var block = headers[headerIndex];
            bool? headerValid = null; byte cylinder = 0; byte number = 0;
            if (block.Bytes is not null)
            {
                cylinder = block.Bytes[3]; number = block.Bytes[2]; headerValid = block.Bytes[0] == CommodoreGcrFormat.HeaderMark && (byte)(block.Bytes[1] ^ block.Bytes[2] ^ block.Bytes[3] ^ block.Bytes[4] ^ block.Bytes[5]) == 0;
            }
            var nextHeaderOffset = headerIndex + 1 < headers.Count ? headers[headerIndex + 1].SyncOffset : int.MaxValue;
            var data = dataBlocks.FirstOrDefault(candidate => candidate.SyncOffset > block.EndOffset && candidate.SyncOffset < nextHeaderOffset); bool? dataValid = null;
            if (data.Bytes is not null) { byte checksum = 0; for (var index = 1; index < CommodoreGcrFormat.DataRecordByteCount; index++) checksum ^= data.Bytes[index]; dataValid = checksum == 0; }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            var payload = data.Bytes is null ? null : data.Bytes.Skip(1).Take(CommodoreGcrFormat.SectorByteCount).ToArray();
            sectors.Add(new(cylinder, 0, number, CommodoreGcrFormat.SectorSizeCode, CommodoreGcrFormat.SectorByteCount, integrity, block.SyncOffset, SectorIntegrityKind.Checksum, payload));
            structures.Add(new(FluxStructureKind.CommodoreHeader, block.SyncOffset, Math.Max(CommodoreGcrFormat.MinimumSyncBitCount, block.EndOffset - block.SyncOffset), FluxStructureDescriptions.Complete("Commodore", FluxStructureKind.CommodoreHeader, cylinder, 0, number, CommodoreGcrFormat.SectorByteCount, null, null, headerValid, dataValid, "header checksum", "data checksum")));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 42d), stream.BitCellTicks, structures, bytes, sectors);
    }

}
