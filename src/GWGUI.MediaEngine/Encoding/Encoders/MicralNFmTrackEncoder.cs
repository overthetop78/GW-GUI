using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

public sealed class MicralNFmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.MicralNFm;
    public override string DisplayName => FluxCodecDisplayNames.MicralNFm;
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
    private static byte Update(byte checksum, byte data)
    {
        var carrySource = ((data ^ checksum) ^ MicralNFmFormat.ComplementMask) & ((data + checksum) ^ data);
        return (byte)(checksum + data + ((carrySource & MicralNFmFormat.CarryMask) != 0 ? 1 : 0));
    }
}
