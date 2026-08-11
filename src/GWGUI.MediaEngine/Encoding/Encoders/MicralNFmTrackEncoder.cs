using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Micral NFM.</summary>
public sealed class MicralNFmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.MicralNFm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.MicralNFm;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != MicralNFmFormat.SectorSize) throw MicralNFmFormat.InvalidSectorSize(sector.Data.Count);
            byte checksum = 0;
            foreach (var value in sector.Data) checksum = Update(checksum, value);
            bits.Raw(MicralNFmFormat.SectorMark.ToArray()); bits.Fm(new byte[] {(byte)sector.Number,(byte)request.Cylinder}.Concat(sector.Data).Append(checksum));
            bits.Gap(MicralNFmFormat.GapBitCount);
        }
        return bits;
    }
    /// <summary>Met à jour la somme de contrôle avec une donnée supplémentaire.</summary>
    private static byte Update(byte checksum, byte data)
    {
        var carrySource = ((data ^ checksum) ^ MicralNFmFormat.ComplementMask) & ((data + checksum) ^ data);
        return (byte)(checksum + data + ((carrySource & MicralNFmFormat.CarryMask) != 0 ? 1 : 0));
    }
}
