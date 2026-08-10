namespace GWGUI.MediaEngine.Encoding;

public sealed class MembrainMfmTrackEncoder : TrackEncoderBase
{
    public override string Id => "membrain.mfm";
    public override string DisplayName => "Membrain MFM";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 512) throw new ArgumentException("Membrain sectors contain 512 bytes.");
            var cylinderHigh = (byte)(request.Cylinder >> 3);
            var packed = (byte)((request.Cylinder & 7) << 5 | request.Head << 4 | sector.Number & 15);
            byte[] header = [0xa1, 0xfe, cylinderHigh, packed];
            var headerCrc = TrackEncoding.Crc16(header, 0x8005, 0);
            bits.RawHex("44895554");
            bits.Mfm([header[2], header[3], (byte)(headerCrc >> 8), (byte)headerCrc]);
            bits.Gap(64);
            const byte mark = 0xf8;
            var dataCrc = TrackEncoding.Crc16(new[] { (byte)0xa1, mark }.Concat(sector.Data), 0x8005, 0);
            bits.RawHex("4489554A");
            bits.Mfm(sector.Data.Concat([(byte)(dataCrc >> 8), (byte)dataCrc]));
            bits.Gap(128);
        }
        return bits;
    }
}
