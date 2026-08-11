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
            if (!HasSynchronizationAt(stream, offset)) continue;
            var encoded = FluxBitReader.TryDecodeMfmBytes(stream, offset + AmigaMfmFormat.SyncBitCount, encodedBytes); var available = encoded ?? FluxBitReader.TryDecodeMfmBytes(stream, offset + AmigaMfmFormat.SyncBitCount, headerBytes);
            var header = DecodeAndValidateHeader(available, bytes);
            var data = DecodeAndValidateData(encoded, bytes);
            var length = data?.Length ?? header?.Length ?? AmigaMfmFormat.SyncBitCount;
            AddSectorAndStructure(offset, length, header, data, sectors, structures);
            offset += Math.Max(AmigaMfmFormat.SyncBitCount - 1, length - 1);
        }
        return new(Id, DisplayName, FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, AmigaMfmFormat.ConfidenceSectorWeight, AmigaMfmFormat.ConfidenceDivisor), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static bool HasSynchronizationAt(FluxBitstream stream, int offset) => FluxBitReader.Match(stream, offset, AmigaMfmFormat.SyncWord) && FluxBitReader.Match(stream, offset + AmigaMfmFormat.EncodedByteBitCount, AmigaMfmFormat.SyncWord);

    private static AmigaHeaderDecodeResult? DecodeAndValidateHeader(byte[]? available, List<byte> bytes)
    {
        if (available is null) return null;
        var header = AmigaMfmCodec.DecodeOddEven(available.Take(AmigaMfmFormat.InfoByteCount).ToArray());
        var cylinder = (byte)(header[AmigaMfmFormat.TrackAndHeadOffset] >> AmigaMfmFormat.TrackCylinderShift);
        var head = (byte)(header[AmigaMfmFormat.TrackAndHeadOffset] & AmigaMfmFormat.TrackHeadMask);
        var number = header[AmigaMfmFormat.SectorNumberOffset];
        var parity = AmigaMfmCodec.CalculateParity(available, 0, AmigaMfmFormat.HeaderParitySourceByteCount);
        var valid = header[AmigaMfmFormat.FormatByteOffset] == AmigaMfmFormat.FormatByte && available[AmigaMfmFormat.HeaderParityHighOffset] == parity.High && available[AmigaMfmFormat.HeaderParityLowOffset] == parity.Low;
        bytes.AddRange(header);
        return new(cylinder, head, number, valid, AmigaMfmFormat.SyncBitCount + available.Length * AmigaMfmFormat.EncodedByteBitCount);
    }

    private static AmigaDataDecodeResult? DecodeAndValidateData(byte[]? encoded, List<byte> bytes)
    {
        if (encoded is null) return null;
        var parity = AmigaMfmCodec.CalculateSplitParity(encoded, AmigaMfmFormat.EncodedDataOffset, AmigaMfmFormat.EncodedDataByteCount);
        var valid = encoded[AmigaMfmFormat.DataParityHighOffset] == parity.High && encoded[AmigaMfmFormat.DataParityLowOffset] == parity.Low;
        var payload = AmigaMfmCodec.DecodeOddEven(encoded.Skip(AmigaMfmFormat.EncodedDataOffset).Take(AmigaMfmFormat.EncodedDataByteCount).ToArray());
        bytes.AddRange(payload);
        return new(payload, valid, AmigaMfmFormat.SyncBitCount + AmigaMfmFormat.EncodedSectorByteCount * AmigaMfmFormat.EncodedByteBitCount);
    }

    private static void AddSectorAndStructure(int offset, int length, AmigaHeaderDecodeResult? header, AmigaDataDecodeResult? data, List<DecodedSector> sectors, List<FluxStructure> structures)
    {
        var cylinder = header?.Cylinder ?? 0;
        var head = header?.Head ?? 0;
        var number = header?.Sector ?? 0;
        var integrity = header?.Valid == false || data?.Valid == false ? false : data is null ? null : true;
        sectors.Add(new(cylinder, head, number, SectorSizeCode.FromByteCount(AmigaMfmFormat.SectorByteCount), AmigaMfmFormat.SectorByteCount, integrity, offset, SectorIntegrityKind.Checksum, data?.Payload));
        structures.Add(new(FluxStructureKind.AmigaSync, offset, length, FluxStructureDescriptions.CompleteWithChecksums(AmigaMfmFormat.StructureDescriptionName, FluxStructureKind.AmigaSync, cylinder, head, number, AmigaMfmFormat.SectorByteCount, header?.Valid, data?.Valid)));
    }

    private sealed record AmigaHeaderDecodeResult(byte Cylinder, byte Head, byte Sector, bool Valid, int Length);
    private sealed record AmigaDataDecodeResult(byte[] Payload, bool Valid, int Length);
}
