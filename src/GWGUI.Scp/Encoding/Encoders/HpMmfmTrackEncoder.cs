namespace GWGUI.Scp.Encoding;

public sealed class HpMmfmTrackEncoder : TrackEncoderBase
{
    public override string Id => "hp.mmfm";
    public override string DisplayName => "HP MMFM";

    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 256) throw new ArgumentException("HP MMFM sectors contain 256 bytes.");
            var encodedSector = (byte)(sector.Number | request.Head << 7);
            byte[] identity = [TrackEncoding.ReverseBits((byte)request.Cylinder), TrackEncoding.ReverseBits(encodedSector)];
            bits.Raw(0x55, 0x55, 0x2a, 0x54);
            bits.Mfm(TrackEncoding.WithCrc(identity));
            bits.Gap(128);
            var payload = sector.Data.ToArray();
            for (var index = 0; index < payload.Length; index += 2) (payload[index], payload[index + 1]) = (payload[index + 1], payload[index]);
            for (var index = 0; index < payload.Length; index++) payload[index] = TrackEncoding.ReverseBits(payload[index]);
            bits.Raw(0x55, 0x55, 0x2a, 0x44);
            bits.Mfm(TrackEncoding.WithCrc(payload));
            bits.Gap(256);
        }
        return bits;
    }
}
