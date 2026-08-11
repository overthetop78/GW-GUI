using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les secteurs GCR zonés de 512 octets utilisés par le Commodore 900.</summary>
public sealed class Commodore900GcrDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => Commodore900GcrFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => Commodore900GcrFormat.CodecDisplayName;

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
            structures.Add(new(FluxStructureKind.CommodoreSync, offset, end - offset, FluxStructureDescriptions.UnclassifiedMark(Commodore900GcrFormat.StructureDescriptionName, FluxStructureKind.CommodoreSync, null, Commodore900GcrFormat.SyncDescription)));
            if (CommodoreGcrCodec.TryDecodeBytes(stream.Bits, end, Commodore900GcrFormat.HeaderByteCount) is { } header && header[Commodore900GcrFormat.HeaderMarkOffset] == Commodore900GcrFormat.HeaderMark)
            {
                headers.Add((offset, end + Commodore900GcrFormat.EncodedHeaderBitCount, header)); decodedBytes.AddRange(header);
            }
            else if (CommodoreGcrCodec.TryDecodeBytes(stream.Bits, end, Commodore900GcrFormat.DataRecordByteCount) is { } data && data[Commodore900GcrFormat.DataMarkOffset] == Commodore900GcrFormat.DataMark)
            {
                dataBlocks.Add((offset, end + Commodore900GcrFormat.EncodedDataRecordBitCount, data)); decodedBytes.AddRange(data);
            }
            offset = end;
        }

        var sectors = new List<DecodedSector>();
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index];
            var cylinder = header.Bytes[Commodore900GcrFormat.HeaderCylinderOffset]; var number = header.Bytes[Commodore900GcrFormat.HeaderSectorOffset];
            var headerValid = (byte)(header.Bytes[Commodore900GcrFormat.HeaderMarkOffset] ^ cylinder ^ number ^ header.Bytes[Commodore900GcrFormat.HeaderChecksumOffset]) == 0;
            var next = index + 1 < headers.Count ? headers[index + 1].Offset : int.MaxValue;
            var data = dataBlocks.FirstOrDefault(candidate => candidate.Offset > header.End && candidate.Offset < next);
            var dataValid = data.Bytes is not null && data.Bytes.Aggregate((byte)0, (checksum, value) => (byte)(checksum ^ value)) == 0;
            var payload = data.Bytes?.Skip(Commodore900GcrFormat.DataPayloadOffset).Take(Commodore900GcrFormat.SectorByteCount).ToArray();
            var valid = !headerValid || data.Bytes is null || !dataValid ? false : true;
            sectors.Add(new(cylinder, Commodore900GcrFormat.LogicalHead, number, Commodore900GcrFormat.SectorSizeCode, Commodore900GcrFormat.SectorByteCount, valid, header.Offset, SectorIntegrityKind.Checksum, payload));
            structures.Add(new(FluxStructureKind.CommodoreHeader, header.Offset, Math.Max(Commodore900GcrFormat.MinimumSyncBitCount, header.End - header.Offset),
                FluxStructureDescriptions.Identity(Commodore900GcrFormat.StructureDescriptionName, FluxStructureKind.CommodoreHeader, cylinder, Commodore900GcrFormat.LogicalHead, number, Commodore900GcrFormat.SectorByteCount, null, null)));
            if (data.Bytes is not null)
                structures.Add(new(FluxStructureKind.FormatData, data.Offset, Math.Max(Commodore900GcrFormat.MinimumSyncBitCount, data.End - data.Offset),
                    FluxStructureDescriptions.WithIntegrity(Commodore900GcrFormat.StructureDescriptionName, FluxStructureKind.FormatData, cylinder, Commodore900GcrFormat.LogicalHead, number, Commodore900GcrFormat.SectorByteCount, null, null, Commodore900GcrFormat.ChecksumDescription, dataValid)));
        }
        var validCount = sectors.Count(sector => sector.IntegrityValid == true);
        return new(Id, DisplayName, Math.Min(1, validCount / (double)Commodore900GcrFormat.ExpectedSectorCount), stream.BitCellTicks, structures, decodedBytes, sectors);
    }

}
