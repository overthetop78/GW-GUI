namespace GWGUI.MediaEngine.Encoding;

public sealed class AmigaMfmTrackEncoder : TrackEncoderBase
{
    public override string Id => FluxCodecIds.AmigaMfm;
    public override string DisplayName => "Amiga MFM";
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != 512) throw new ArgumentException("Amiga sectors contain 512 bytes.");
            byte[] info = [0xff,(byte)(request.Cylinder << 1 | request.Head),(byte)sector.Number,(byte)request.Sectors.Count];
            var headerAndLabel = EncodeOddEven(info).Concat(new byte[16]).ToArray();
            var headerParity = Parity(headerAndLabel, false);
            var data = EncodeOddEven(sector.Data);
            var dataParity = Parity(data, true);
            var encoded = headerAndLabel.Concat(new byte[] { 0,0,headerParity.High,headerParity.Low,0,0,dataParity.High,dataParity.Low }).Concat(data);
            bits.Gap(100);
            bits.RawHex("44894489");
            bits.Mfm(encoded);
            bits.Gap(128);
        }
        return bits;
    }
    private static byte Nibble(byte value, bool odd)
    {
        byte result = 0; var first = odd ? 7 : 6;
        for (var index = 0; index < 4; index++) result |= (byte)(((value >> (first - index * 2)) & 1) << (3 - index));
        return result;
    }
    private static byte[] EncodeOddEven(IReadOnlyList<byte> values)
    {
        if ((values.Count & 1) != 0) throw new ArgumentException("Amiga odd/even encoding requires an even byte count.");
        var odd = new List<byte>(); var even = new List<byte>();
        for (var index = 0; index < values.Count; index += 2)
        {
            odd.Add((byte)(Nibble(values[index], true) << 4 | Nibble(values[index + 1], true)));
            even.Add((byte)(Nibble(values[index], false) << 4 | Nibble(values[index + 1], false)));
        }
        return odd.Concat(even).ToArray();
    }
    private static (byte High, byte Low) Parity(IReadOnlyList<byte> encoded, bool split)
    {
        byte high = 0, low = 0;
        if (split)
        {
            var half = encoded.Count / 2;
            for (var index = 0; index < half; index += 2) { high ^= (byte)(encoded[index] ^ encoded[half + index]); low ^= (byte)(encoded[index + 1] ^ encoded[half + index + 1]); }
        }
        else for (var index = 0; index < encoded.Count; index += 4) { high ^= (byte)(encoded[index] ^ encoded[index + 2]); low ^= (byte)(encoded[index + 1] ^ encoded[index + 3]); }
        return (high, low);
    }
}
