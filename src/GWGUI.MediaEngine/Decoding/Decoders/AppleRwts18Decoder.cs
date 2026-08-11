using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Decoding.Apple;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode le format de piste Brøderbund RWTS18 de Roland Gustafsson : six secteurs physiques de 768 octets, formés chacun de trois pages indépendantes de 256 octets.</summary>
public sealed class AppleRwts18Decoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.AppleRwts18;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.AppleRwts18;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder en NRZI RWTS18.</param><returns>Résultat du décodage.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution) => DecodeCore(FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals));
    /// <summary>Décode directement les bits d'une piste RWTS18.</summary>
    /// <param name="bits">Bits de la piste.</param><returns>Résultat du décodage.</returns>
    internal FluxDecodeResult DecodeBits(bool[] bits) => DecodeCore(new FluxBitstream(bits, 1));

    /// <summary>Exécute le traitement « Decode Core » propre à ce format.</summary>
    /// <param name="source">Flux binaire source.</param><returns>Structures, secteurs et octets RWTS18 reconnus.</returns>
    private FluxDecodeResult DecodeCore(FluxBitstream source)
    {
        var trackBitLength = source.Bits.Length;
        var stream = source.WithCircularTail(AppleRwts18Format.CircularTailBitCount);
        var structures = new List<FluxStructure>();
        var decodedBytes = new List<byte>();
        var sectors = new List<DecodedSector>();

        for (var offset = 0; offset + AppleRwts18Format.AddressMarkBitCount <= trackBitLength; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, AppleRwts18Format.EncodedAddressMark, AppleRwts18Format.AddressMarkBitCount)) continue;
            var cursor = offset + AppleRwts18Format.AddressMarkBitCount;
            var address = AppleBitLatch.TryReadBytes(stream.Bits, ref cursor, AppleRwts18Format.AddressByteCount);
            if (address is null || !AppleIIGcrFormat.InverseSixAndTwoTable.TryGetValue(address[0], out var track) ||
                !AppleIIGcrFormat.InverseSixAndTwoTable.TryGetValue(address[1], out var sector) ||
                !AppleIIGcrFormat.InverseSixAndTwoTable.TryGetValue(address[2], out var checksum) || address[3] != AppleRwts18Format.AddressTrailer ||
                sector >= AppleRwts18Format.SectorCount || (byte)(track ^ sector) != checksum)
                continue;

            var data = TryReadData(stream.Bits, cursor);
            var integrity = data?.Valid;
            var payload = data?.Data;
            structures.Add(new(FluxStructureKind.AppleAddress, offset, cursor - offset, $"{FluxStructureDescriptions.Identity("Apple II RWTS18", FluxStructureKind.AppleAddress, track, 0, sector, AppleRwts18Format.SectorByteCount, null, null)}, {FluxStructureDescriptions.Integrity("address checksum", true)}"));
            if (data is not null)
            {
                structures.Add(new(FluxStructureKind.AppleData, data.Value.StartOffset,
                    data.Value.EndOffset - data.Value.StartOffset,
                    $"{FluxStructureDescriptions.Identity("Apple II RWTS18", FluxStructureKind.AppleData, track, 0, sector, AppleRwts18Format.SectorByteCount, null, null)}, {FluxStructureDescriptions.Integrity("checksum", data.Value.Valid)}"));
                decodedBytes.AddRange(data.Value.Data);
            }
            sectors.Add(new(track, 0, sector, SectorSizeCode.FromByteCount(AppleRwts18Format.SectorByteCount), AppleRwts18Format.SectorByteCount, integrity, offset, SectorIntegrityKind.Checksum, payload));
            offset = Math.Max(offset + AppleRwts18Format.AddressMarkAdvanceBitCount, (data?.EndOffset ?? cursor) - 1);
        }

        var valid = sectors.Count(sector => sector.IntegrityValid == true);
        var confidence = sectors.Count == 0 ? 0 : Math.Min(1, valid / (double)AppleRwts18Format.ConfidenceCompleteSectorDivisor + sectors.Count / AppleRwts18Format.ConfidenceDetectedSectorDivisor);
        return new(Id, DisplayName, confidence, source.BitCellTicks, structures, decodedBytes, sectors);
    }

    /// <summary>Exécute le traitement « Try Read Data » propre à ce format.</summary>
    private static (byte[] Data, bool Valid, int StartOffset, int EndOffset)? TryReadData(IReadOnlyList<bool> bits, int offset)
    {
        var cursor = offset;
        var stream = AppleBitLatch.TryReadBytes(bits, ref cursor, AppleRwts18Format.DataReadWindowByteCount);
        if (stream is null) return null;
        // The first byte is a modifiable Brøderbund identifier. Find it by the
        // following uninterrupted run of 1025 valid GCR symbols and D4 epilogue.
        for (var start = 0; start + AppleRwts18Format.DataRecordByteCount <= stream.Length; start++)
        {
            var values = new byte[AppleRwts18Format.PayloadWithChecksumSymbolCount];
            var validSymbols = true;
            for (var index = 0; index < values.Length; index++)
            {
                if (AppleIIGcrFormat.InverseSixAndTwoTable.TryGetValue(stream[start + AppleRwts18Format.PayloadOffset + index], out values[index])) continue;
                validSymbols = false;
                break;
            }
            if (!validSymbols || stream[start + AppleRwts18Format.DataEpilogueOffset] != AppleRwts18Format.DataEpilogue) continue;
            var decoded = AppleRwts18Codec.DecodePayload(values);
            var startOffset = offset + start * BitPrimitives.BitsPerByte;
            var endOffset = offset + (start + AppleRwts18Format.DataRecordByteCount) * BitPrimitives.BitsPerByte;
            return (decoded.Data, decoded.Valid, startOffset, endOffset);
        }
        return null;
    }

}
