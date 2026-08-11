namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encodes the zoned 512-byte GCR sectors used by the Commodore 900.</summary>
public sealed class Commodore900GcrTrackEncoder : TrackEncoderBase
{
    private static readonly int[] Table =
        [0x0a, 0x0b, 0x12, 0x13, 0x0e, 0x0f, 0x16, 0x17, 0x09, 0x19, 0x1a, 0x1b, 0x0d, 0x1d, 0x1e, 0x15];

    public override string Id => FluxCodecIds.Commodore900Gcr;
    public override string DisplayName => FluxCodecDisplayNames.Commodore900Gcr;

    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 512) throw new ArgumentException("Commodore 900 sectors contain 512 bytes.");
            var headerChecksum = (byte)(0x08 ^ request.Cylinder ^ sector.Number);
            var dataChecksum = (byte)0x07;
            foreach (var value in sector.Data) dataChecksum ^= value;

            bits.Gap(40, true);
            Gcr(bits, [0x08, (byte)request.Cylinder, (byte)sector.Number, headerChecksum]);
            bits.Gap(120);
            bits.Gap(40, true);
            Gcr(bits, new byte[] { 0x07 }.Concat(sector.Data).Append(dataChecksum));
            bits.Gap(120);
        }
        return bits;
    }

    private static void Gcr(List<bool> bits, IEnumerable<byte> values)
    {
        foreach (var value in values)
            foreach (var nibble in new[] { value >> 4, value & 15 })
                for (var bit = 4; bit >= 0; bit--)
                    bits.Add((Table[nibble] & (1 << bit)) != 0);
    }
}
