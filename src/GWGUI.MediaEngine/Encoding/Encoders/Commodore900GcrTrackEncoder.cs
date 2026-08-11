using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encodes the zoned 512-byte GCR sectors used by the Commodore 900.</summary>
public sealed class Commodore900GcrTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.Commodore900Gcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.Commodore900Gcr;

    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != Commodore900GcrFormat.SectorByteCount) throw Commodore900GcrFormat.InvalidSectorSize(sector.Data.Count);
            var headerChecksum = (byte)(Commodore900GcrFormat.HeaderMark ^ request.Cylinder ^ sector.Number);
            var dataChecksum = Commodore900GcrFormat.DataMark;
            foreach (var value in sector.Data) dataChecksum ^= value;

            bits.Gap(Commodore900GcrFormat.SyncGapBitCount, true);
            Gcr(bits, [Commodore900GcrFormat.HeaderMark, (byte)request.Cylinder, (byte)sector.Number, headerChecksum]);
            bits.Gap(Commodore900GcrFormat.RecordGapBitCount);
            bits.Gap(Commodore900GcrFormat.SyncGapBitCount, true);
            Gcr(bits, new byte[] { Commodore900GcrFormat.DataMark }.Concat(sector.Data).Append(dataChecksum));
            bits.Gap(Commodore900GcrFormat.RecordGapBitCount);
        }
        return bits;
    }

    /// <summary>Exécute le traitement « GCR » propre à ce format.</summary>
    private static void Gcr(List<bool> bits, IEnumerable<byte> values)
    {
        foreach (var value in values)
            foreach (var nibble in new[] { value >> 4, value & Commodore900GcrFormat.NibbleMask })
                for (var bit = Commodore900GcrFormat.EncodedNibbleBitCount - 1; bit >= 0; bit--)
                    bits.Add((Commodore900GcrFormat.EncodingTable[nibble] & (1 << bit)) != 0);
    }
}
