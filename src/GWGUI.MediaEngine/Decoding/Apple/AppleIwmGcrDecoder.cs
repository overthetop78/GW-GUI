using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode le format sectoriel GCR commun aux contrôleurs Apple IWM Macintosh et Lisa FileWare.</summary>
public abstract class AppleIwmGcrDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique de la spécialisation.</summary>
    public abstract string Id { get; }
    /// <summary>Obtient le nom affiché de la spécialisation.</summary>
    public abstract string DisplayName { get; }

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder en NRZI Macintosh.</param><returns>Résultat du décodage.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution) => DecodeCore(revolution, FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals));

    /// <summary>Décode directement les bits d'une piste Macintosh.</summary>
    /// <param name="bits">Bits de la piste.</param><returns>Résultat du décodage.</returns>
    internal FluxDecodeResult DecodeBits(bool[] bits) => DecodeCore(new FluxRevolution((uint)bits.Length, []), new FluxBitstream(bits, 1));

    /// <summary>Exécute le traitement « Decode At Bit Cell » propre à ce format.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><param name="bitCellTicks">Durée imposée d'une cellule en ticks.</param><returns>Résultat du décodage.</returns>
    public FluxDecodeResult DecodeAtBitCell(FluxRevolution revolution, double bitCellTicks) =>
        DecodeCore(revolution, FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals, bitCellTicks));

    /// <summary>Exécute le traitement « Decode Core » propre à ce format.</summary>
    /// <param name="revolution">Révolution source.</param><param name="stream">Flux binaire décodé.</param><returns>Structures, secteurs et octets reconnus.</returns>
    private FluxDecodeResult DecodeCore(FluxRevolution revolution, FluxBitstream stream)
    {
        var trackBitLength = stream.Bits.Length;
        stream = stream.WithCircularTail(AppleIwmGcrFormat.CircularTailBitCount);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        const int markBits = AppleIwmGcrFormat.MarkBitCount; const int headerSymbols = AppleIwmGcrFormat.HeaderSymbolCount; const int dataSymbols = AppleIwmGcrFormat.DataSymbolCount;
        for (var offset = 0; offset < trackBitLength && offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, AppleIwmGcrFormat.AddressMark)) continue;
            var header = TryReadSymbols(stream, offset + markBits, headerSymbols); bool? headerValid = null; byte cylinder = 0, head = 0, number = 0, format = 0;
            if (header is not null && header.All(AppleIIGcrFormat.InverseSixAndTwoTable.ContainsKey))
            {
                var values = header.Select(value => AppleIIGcrFormat.InverseSixAndTwoTable[value]).ToArray();
                cylinder = (byte)(((values[2] & AppleIwmGcrFormat.CylinderHighBitMask) << AppleIwmGcrFormat.CylinderHighBitShift) | (values[0] & AppleIwmGcrFormat.SixBitMask)); head = (byte)((values[2] >> AppleIwmGcrFormat.HeadBitShift) & AppleIwmGcrFormat.HeadBitMask); number = values[1]; format = values[3];
                headerValid = (byte)((values[0] ^ values[1] ^ values[2] ^ values[3]) & AppleIwmGcrFormat.SixBitMask) == values[4];
            }
            var headerEnd = offset + markBits + (header is null ? 0 : headerSymbols * BitPrimitives.BitsPerByte);
            var dataOffset = headerValid == true ? FindMark(stream, headerEnd, Math.Min(stream.Bits.Length, headerEnd + AppleIwmGcrFormat.DataSearchBitCount), AppleIwmGcrFormat.DataMark) : -1;
            bool? dataValid = null; byte[]? sectorData = null; byte[]? sectorTag = null; var structureEnd = headerEnd;
            if (dataOffset >= 0)
            {
                pairedData.Add(dataOffset); var encoded = TryReadSymbols(stream, dataOffset + markBits, dataSymbols);
                if (encoded is not null && encoded.All(AppleIIGcrFormat.InverseSixAndTwoTable.ContainsKey))
                {
                    var values = encoded.Select(value => AppleIIGcrFormat.InverseSixAndTwoTable[value]).ToArray(); var decoded = AppleIwmGcrCodec.Decode(values.AsSpan(1, AppleIwmGcrFormat.EncodedPayloadSymbolCount), out var checksum);
                    dataValid = checksum[3] == values[AppleIwmGcrFormat.PackedChecksumSymbolOffset] && checksum[2] == values[AppleIwmGcrFormat.ThirdChecksumSymbolOffset] && checksum[1] == values[AppleIwmGcrFormat.SecondChecksumSymbolOffset] && checksum[0] == values[AppleIwmGcrFormat.FirstChecksumSymbolOffset];
                    sectorTag = decoded.Take(AppleIwmGcrFormat.TagByteCount).ToArray(); sectorData = decoded.Skip(AppleIwmGcrFormat.TagByteCount).Take(AppleIwmGcrFormat.SectorByteCount).ToArray(); bytes.AddRange(sectorData); structureEnd = dataOffset + markBits + dataSymbols * BitPrimitives.BitsPerByte;
                    structures.Add(new(FluxStructureKind.AppleData, dataOffset, structureEnd - dataOffset, $"{FluxStructureDescriptions.Identity("Apple Macintosh", FluxStructureKind.AppleData, cylinder, head, number, AppleIwmGcrFormat.SectorByteCount, null, null)}, {FluxStructureDescriptions.Integrity("checksum", dataValid)}"));
                }
                else structures.Add(new(FluxStructureKind.AppleData, dataOffset, markBits, FluxStructureDescriptions.Truncated("Apple Macintosh", FluxStructureKind.AppleData, null, "checksum unavailable")));
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, head, number, AppleIwmGcrFormat.SectorSizeCode, AppleIwmGcrFormat.SectorByteCount, integrity, offset, SectorIntegrityKind.Checksum, sectorData, sectorTag, format));
            structures.Add(new(FluxStructureKind.AppleAddress, offset, Math.Max(markBits, headerEnd - offset), FluxStructureDescriptions.Complete("Apple Macintosh", FluxStructureKind.AppleAddress, cylinder, head, number, AppleIwmGcrFormat.SectorByteCount, null, null, headerValid, dataValid, "address checksum", "data checksum")));
            offset = headerValid == true ? Math.Max(offset + markBits - 1, structureEnd - 1) : offset + markBits - 1;
        }
        for (var offset = 0; offset < trackBitLength && offset + markBits <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, AppleIwmGcrFormat.DataMark) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.AppleData, offset, markBits, FluxStructureDescriptions.UnpairedData("Apple Macintosh", null, "data prologue"))); offset += markBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 24d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    /// <summary>Exécute le traitement « Try Read Symbols » propre à ce format.</summary>
    /// <param name="stream">Flux binaire source.</param><param name="offset">Offset de départ en bits.</param><param name="count">Nombre de symboles à lire.</param><returns>Symboles lus, ou <see langword="null"/> si la plage est incomplète.</returns>
    private static byte[]? TryReadSymbols(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * BitPrimitives.BitsPerByte > stream.Bits.Length) return null;
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeByte(stream, offset + index * BitPrimitives.BitsPerByte, out result[index])) return null;
        return result;
    }
    /// <summary>Exécute le traitement « Find Mark » propre à ce format.</summary>
    /// <param name="stream">Flux à parcourir.</param><param name="start">Offset de départ inclus, en bits.</param><param name="end">Offset de fin exclu, en bits.</param><param name="mark">Marque recherchée.</param><returns>Offset de la marque en bits, ou <c>-1</c>.</returns>
    private static int FindMark(FluxBitstream stream, int start, int end, IReadOnlyList<byte> mark)
    {
        for (var offset = start; offset + mark.Count * BitPrimitives.BitsPerByte <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, mark)) return offset;
        return -1;
    }
}
