using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Amiga MFM.</summary>
public sealed class AmigaMfmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.AmigaMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.AmigaMfm;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int encodedBytes = AmigaMfmFormat.EncodedSectorByteCount; const int headerBytes = AmigaMfmFormat.EncodedHeaderByteCount; const int dataOffset = AmigaMfmFormat.EncodedDataOffset; const int dataBytes = AmigaMfmFormat.SectorByteCount;
        for (var offset = 0; offset + AmigaMfmFormat.SyncBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, AmigaMfmFormat.SyncWord) || !FluxBitReader.Match(stream, offset + AmigaMfmFormat.EncodedByteBitCount, AmigaMfmFormat.SyncWord)) continue;
            var encoded = TryDecodeMfmBytes(stream, offset + AmigaMfmFormat.SyncBitCount, encodedBytes); var available = encoded ?? TryDecodeMfmBytes(stream, offset + AmigaMfmFormat.SyncBitCount, headerBytes);
            bool? headerValid = null; bool? dataValid = null; byte cylinder = 0; byte head = 0; byte number = 0; var length = AmigaMfmFormat.SyncBitCount; byte[]? payload = null;
            if (available is not null)
            {
                var header = DecodeOddEven(available.Take(AmigaMfmFormat.InfoByteCount).ToArray()); cylinder = (byte)(header[1] >> 1); head = (byte)(header[1] & 1); number = header[2];
                var headerParity = CalculateParity(available, 0, AmigaMfmFormat.HeaderParitySourceByteCount); headerValid = header[0] == AmigaMfmFormat.FormatByte && available[AmigaMfmFormat.HeaderParityHighOffset] == headerParity.High && available[AmigaMfmFormat.HeaderParityLowOffset] == headerParity.Low;
                bytes.AddRange(header); length = AmigaMfmFormat.SyncBitCount + available.Length * AmigaMfmFormat.EncodedByteBitCount;
                if (encoded is not null)
                {
                    var parity = CalculateSplitParity(encoded, dataOffset, dataBytes); dataValid = encoded[AmigaMfmFormat.DataParityHighOffset] == parity.High && encoded[AmigaMfmFormat.DataParityLowOffset] == parity.Low;
                    payload = DecodeOddEven(encoded.Skip(dataOffset).Take(dataBytes).ToArray()); bytes.AddRange(payload); length = AmigaMfmFormat.SyncBitCount + encodedBytes * AmigaMfmFormat.EncodedByteBitCount;
                }
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, head, number, AmigaMfmFormat.SectorSizeCode, AmigaMfmFormat.SectorByteCount, integrity, offset, SectorIntegrityKind.Checksum, payload));
            structures.Add(new(FluxStructureKind.AmigaSync, offset, length, FluxStructureDescriptions.Complete("Amiga", FluxStructureKind.AmigaSync, cylinder, head, number, AmigaMfmFormat.SectorByteCount, null, null, headerValid, dataValid, "header checksum", "data checksum")));
            offset += Math.Max(AmigaMfmFormat.SyncBitCount - 1, length - 1);
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 3 + structures.Count) / 44d), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * AmigaMfmFormat.EncodedByteBitCount > stream.Bits.Length) return null; var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * AmigaMfmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }
    /// <summary>Exécute le traitement « Decode Odd Even » propre à ce format.</summary>
    private static byte[] DecodeOddEven(IReadOnlyList<byte> encoded)
    {
        var result = new byte[encoded.Count]; var half = encoded.Count / 2;
        for (var index = 0; index < half; index++)
        {
            var odd = encoded[index]; var even = encoded[index + half]; result[index * 2] = Interleave((byte)(odd >> 4), (byte)(even >> 4)); result[index * 2 + 1] = Interleave((byte)(odd & 15), (byte)(even & 15));
        }
        return result;
    }
    /// <summary>Exécute le traitement « Interleave » propre à ce format.</summary>
    private static byte Interleave(byte odd, byte even)
    {
        byte value = 0; for (var index = 0; index < AmigaMfmFormat.NibbleBitCount; index++) { value |= (byte)(((odd >> (3 - index)) & 1) << (7 - index * 2)); value |= (byte)(((even >> (3 - index)) & 1) << (6 - index * 2)); } return value;
    }
    /// <summary>Exécute le traitement « Calculate Parity » propre à ce format.</summary>
    private static (byte High, byte Low) CalculateParity(IReadOnlyList<byte> encoded, int offset, int count)
    {
        byte high = 0, low = 0; for (var index = 0; index < count; index += 4) { high ^= (byte)(encoded[offset + index] ^ encoded[offset + index + 2]); low ^= (byte)(encoded[offset + index + 1] ^ encoded[offset + index + 3]); } return (high, low);
    }
    /// <summary>Exécute le traitement « Calculate Split Parity » propre à ce format.</summary>
    private static (byte High, byte Low) CalculateSplitParity(IReadOnlyList<byte> encoded, int offset, int count)
    {
        byte high = 0, low = 0; var half = count / 2;
        for (var index = 0; index < half; index += 2) { high ^= (byte)(encoded[offset + index] ^ encoded[offset + half + index]); low ^= (byte)(encoded[offset + index + 1] ^ encoded[offset + half + index + 1]); } return (high, low);
    }
}
