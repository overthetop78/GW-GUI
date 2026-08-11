using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format HP MMFM.</summary>
public sealed class HpMmfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => HpMmfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => HpMmfmFormat.CodecDisplayName;

    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != HpMmfmFormat.SectorSize) throw HpMmfmFormat.InvalidSectorSize(sector.Data.Count);
            var encodedSector = (byte)(sector.Number | request.Head << HpMmfmFormat.HeadShift);
            byte[] identity = [BitPrimitives.ReverseBits((byte)request.Cylinder), BitPrimitives.ReverseBits(encodedSector)];
            bits.Raw(HpMmfmFormat.SectorSync.ToArray());
            bits.Mfm(TrackEncoding.WithCrc(identity));
            bits.Gap(HpMmfmFormat.HeaderGapBitCount);
            bits.Raw(HpMmfmFormat.DataSync.ToArray());
            bits.Mfm(TrackEncoding.WithCrc(HpMmfmCodec.EncodePayload(sector.Data)));
            bits.Gap(HpMmfmFormat.DataGapBitCount);
        }
        return bits;
    }
}
