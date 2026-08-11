using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Encode les pistes utilisant le format Amiga MFM.</summary>
public sealed class AmigaMfmTrackEncoder : TrackEncoderBase
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.AmigaMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.AmigaMfm;
    /// <summary>Encode les secteurs demandés sous forme de cellules binaires.</summary>
    protected override IReadOnlyList<bool> EncodeBits(TrackEncodeRequest request)
    {
        var bits = TrackEncoding.Bits();
        foreach (var sector in request.Sectors)
        {
            if (sector.Data.Count != AmigaMfmFormat.SectorByteCount) throw AmigaMfmFormat.InvalidSectorSize(sector.Data.Count);
            byte[] info = [AmigaMfmFormat.FormatByte,(byte)(request.Cylinder << 1 | request.Head),(byte)sector.Number,(byte)request.Sectors.Count];
            var headerAndLabel = EncodeOddEven(info).Concat(new byte[AmigaMfmFormat.LabelByteCount]).ToArray();
            var headerParity = Parity(headerAndLabel, false);
            var data = EncodeOddEven(sector.Data);
            var dataParity = Parity(data, true);
            var encoded = headerAndLabel.Concat(new byte[] { 0,0,headerParity.High,headerParity.Low,0,0,dataParity.High,dataParity.Low }).Concat(data);
            bits.Gap(AmigaMfmFormat.LeadingGapBitCount);
            bits.Raw((byte)(AmigaMfmFormat.SyncWord >> 8), (byte)(AmigaMfmFormat.SyncWord & byte.MaxValue), (byte)(AmigaMfmFormat.SyncWord >> 8), (byte)(AmigaMfmFormat.SyncWord & byte.MaxValue));
            bits.Mfm(encoded);
            bits.Gap(AmigaMfmFormat.TrailingGapBitCount);
        }
        return bits;
    }
    /// <summary>Exécute le traitement « Nibble » propre à ce format.</summary>
    private static byte Nibble(byte value, bool odd)
    {
        byte result = 0; var first = odd ? 7 : 6;
        for (var index = 0; index < AmigaMfmFormat.NibbleBitCount; index++) result |= (byte)(((value >> (first - index * 2)) & 1) << (3 - index));
        return result;
    }
    /// <summary>Exécute le traitement « Encode Odd Even » propre à ce format.</summary>
    private static byte[] EncodeOddEven(IReadOnlyList<byte> values)
    {
        if ((values.Count & 1) != 0) throw AmigaMfmFormat.OddEncodedByteCount(values.Count);
        var odd = new List<byte>(); var even = new List<byte>();
        for (var index = 0; index < values.Count; index += 2)
        {
            odd.Add((byte)(Nibble(values[index], true) << 4 | Nibble(values[index + 1], true)));
            even.Add((byte)(Nibble(values[index], false) << 4 | Nibble(values[index + 1], false)));
        }
        return odd.Concat(even).ToArray();
    }
    /// <summary>Exécute le traitement « Parity » propre à ce format.</summary>
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
