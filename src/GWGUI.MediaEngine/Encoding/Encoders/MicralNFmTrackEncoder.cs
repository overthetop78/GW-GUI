using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Micral NFM.</summary>
public sealed class MicralNFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => MicralNFmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => MicralNFmFormat.CodecDisplayName;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != MicralNFmFormat.SectorSize) throw MicralNFmFormat.InvalidSectorSize(sector.Data.Count);
            var checksum = MicralNChecksum.Compute(sector.Data);
            bits.Raw(MicralNFmFormat.SectorMark.ToArray());
            bits.Fm(new byte[] { (byte)sector.Number, (byte)request.Cylinder }.Concat(sector.Data).Append(checksum));
            bits.Gap(MicralNFmFormat.GapBitCount);
        }
        return bits;
    }
}
