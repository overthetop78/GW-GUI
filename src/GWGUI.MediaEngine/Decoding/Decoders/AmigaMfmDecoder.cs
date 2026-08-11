using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Amiga MFM.</summary>
public sealed class AmigaMfmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => AmigaMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => AmigaMfmFormat.CodecDisplayName;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP dont les intervalles sont décodés selon le format MFM Amiga.</param>
    /// <returns>Résultat contenant les structures, secteurs, octets décodés et la durée estimée d'une cellule.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int encodedBytes = AmigaMfmFormat.EncodedSectorByteCount; const int headerBytes = AmigaMfmFormat.EncodedHeaderByteCount; const int dataOffset = AmigaMfmFormat.EncodedDataOffset; const int dataBytes = AmigaMfmFormat.EncodedDataByteCount;
        for (var offset = 0; offset + AmigaMfmFormat.SyncBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, AmigaMfmFormat.SyncWord) || !FluxBitReader.Match(stream, offset + AmigaMfmFormat.EncodedByteBitCount, AmigaMfmFormat.SyncWord)) continue;
            var encoded = TryDecodeMfmBytes(stream, offset + AmigaMfmFormat.SyncBitCount, encodedBytes); var available = encoded ?? TryDecodeMfmBytes(stream, offset + AmigaMfmFormat.SyncBitCount, headerBytes);
            bool? headerValid = null; bool? dataValid = null; byte cylinder = 0; byte head = 0; byte number = 0; var length = AmigaMfmFormat.SyncBitCount; byte[]? payload = null;
            if (available is not null)
            {
                var header = AmigaMfmCodec.DecodeOddEven(available.Take(AmigaMfmFormat.InfoByteCount).ToArray()); cylinder = (byte)(header[AmigaMfmFormat.TrackAndHeadOffset] >> AmigaMfmFormat.TrackCylinderShift); head = (byte)(header[AmigaMfmFormat.TrackAndHeadOffset] & AmigaMfmFormat.TrackHeadMask); number = header[AmigaMfmFormat.SectorNumberOffset];
                var headerParity = AmigaMfmCodec.CalculateParity(available, 0, AmigaMfmFormat.HeaderParitySourceByteCount); headerValid = header[AmigaMfmFormat.FormatByteOffset] == AmigaMfmFormat.FormatByte && available[AmigaMfmFormat.HeaderParityHighOffset] == headerParity.High && available[AmigaMfmFormat.HeaderParityLowOffset] == headerParity.Low;
                bytes.AddRange(header); length = AmigaMfmFormat.SyncBitCount + available.Length * AmigaMfmFormat.EncodedByteBitCount;
                if (encoded is not null)
                {
                    var parity = AmigaMfmCodec.CalculateSplitParity(encoded, dataOffset, dataBytes); dataValid = encoded[AmigaMfmFormat.DataParityHighOffset] == parity.High && encoded[AmigaMfmFormat.DataParityLowOffset] == parity.Low;
                    payload = AmigaMfmCodec.DecodeOddEven(encoded.Skip(dataOffset).Take(dataBytes).ToArray()); bytes.AddRange(payload); length = AmigaMfmFormat.SyncBitCount + encodedBytes * AmigaMfmFormat.EncodedByteBitCount;
                }
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, head, number, AmigaMfmFormat.SectorSizeCode, AmigaMfmFormat.SectorByteCount, integrity, offset, SectorIntegrityKind.Checksum, payload));
            structures.Add(new(FluxStructureKind.AmigaSync, offset, length, FluxStructureDescriptions.Complete(AmigaMfmFormat.StructureDescriptionName, FluxStructureKind.AmigaSync, cylinder, head, number, AmigaMfmFormat.SectorByteCount, null, null, headerValid, dataValid, "header checksum", "data checksum")));
            offset += Math.Max(AmigaMfmFormat.SyncBitCount - 1, length - 1);
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 3 + structures.Count) / 44d), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    /// <param name="stream">Flux binaire MFM source.</param>
    /// <param name="offset">Offset du premier octet encodé, exprimé en bits.</param>
    /// <param name="count">Nombre d'octets à décoder.</param>
    /// <returns>Octets décodés, ou <see langword="null"/> si la plage est incomplète ou invalide.</returns>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * AmigaMfmFormat.EncodedByteBitCount > stream.Bits.Length) return null; var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * AmigaMfmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }
}
