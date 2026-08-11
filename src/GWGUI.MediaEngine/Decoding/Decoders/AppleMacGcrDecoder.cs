using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes GCR des disquettes Apple Macintosh.</summary>
public class AppleMacGcrDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Inverse » utilisée par ce codec.</summary>
    private static readonly Dictionary<byte, byte> Inverse = AppleMacGcrFormat.SixAndTwoTable.Select((value, index) => (value, index)).ToDictionary(item => item.value, item => (byte)item.index);
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public virtual string Id => FluxCodecIds.AppleMacGcr;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public virtual string DisplayName => FluxCodecDisplayNames.AppleMacGcr;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder en NRZI Macintosh.</param><returns>Résultat du décodage.</returns>
    public FluxDecodeResult Decode(ScpRevolution revolution) => DecodeCore(revolution, FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals));

    /// <summary>Décode directement les bits d'une piste Macintosh.</summary>
    /// <param name="bits">Bits de la piste.</param><returns>Résultat du décodage.</returns>
    internal FluxDecodeResult DecodeBits(bool[] bits) => DecodeCore(new ScpRevolution((uint)bits.Length, 0, []), new FluxBitstream(bits, 1));

    /// <summary>Exécute le traitement « Decode At Bit Cell » propre à ce format.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><param name="bitCellTicks">Durée imposée d'une cellule en ticks.</param><returns>Résultat du décodage.</returns>
    public FluxDecodeResult DecodeAtBitCell(ScpRevolution revolution, double bitCellTicks) =>
        DecodeCore(revolution, FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals, bitCellTicks));

    /// <summary>Exécute le traitement « Decode Core » propre à ce format.</summary>
    /// <param name="revolution">Révolution source.</param><param name="stream">Flux binaire décodé.</param><returns>Structures, secteurs et octets reconnus.</returns>
    private FluxDecodeResult DecodeCore(ScpRevolution revolution, FluxBitstream stream)
    {
        var trackBitLength = stream.Bits.Length;
        stream = stream.WithCircularTail(AppleMacGcrFormat.CircularTailBitCount);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        const int markBits = AppleMacGcrFormat.MarkBitCount; const int headerSymbols = AppleMacGcrFormat.HeaderSymbolCount; const int dataSymbols = AppleMacGcrFormat.DataSymbolCount;
        for (var offset = 0; offset < trackBitLength && offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, AppleMacGcrFormat.AddressMark)) continue;
            var header = TryReadSymbols(stream, offset + markBits, headerSymbols); bool? headerValid = null; byte cylinder = 0, head = 0, number = 0;
            if (header is not null && header.All(Inverse.ContainsKey))
            {
                var values = header.Select(value => Inverse[value]).ToArray();
                cylinder = (byte)(((values[2] & AppleMacGcrFormat.CylinderHighBitMask) << AppleMacGcrFormat.CylinderHighBitShift) | (values[0] & AppleMacGcrFormat.SixBitMask)); head = (byte)((values[2] >> AppleMacGcrFormat.HeadBitShift) & AppleMacGcrFormat.HeadBitMask); number = values[1];
                headerValid = (byte)((values[0] ^ values[1] ^ values[2] ^ values[3]) & AppleMacGcrFormat.SixBitMask) == values[4];
            }
            var headerEnd = offset + markBits + (header is null ? 0 : headerSymbols * BitPrimitives.BitsPerByte);
            var dataOffset = headerValid == true ? FindMark(stream, headerEnd, Math.Min(stream.Bits.Length, headerEnd + AppleMacGcrFormat.DataSearchBitCount), AppleMacGcrFormat.DataMark) : -1;
            bool? dataValid = null; byte[]? sectorData = null; byte[]? sectorTag = null; var structureEnd = headerEnd;
            if (dataOffset >= 0)
            {
                pairedData.Add(dataOffset); var encoded = TryReadSymbols(stream, dataOffset + markBits, dataSymbols);
                if (encoded is not null && encoded.All(Inverse.ContainsKey))
                {
                    var values = encoded.Select(value => Inverse[value]).ToArray(); var decoded = DecodeSixAndTwo(values.AsSpan(1, AppleMacGcrFormat.EncodedPayloadSymbolCount), out var checksum);
                    var checksumOffset = AppleMacGcrFormat.DataSymbolCount - AppleMacGcrFormat.ChecksumSymbolCount; dataValid = checksum[3] == values[checksumOffset] && checksum[2] == values[checksumOffset + 1] && checksum[1] == values[checksumOffset + 2] && checksum[0] == values[checksumOffset + 3];
                    sectorTag = decoded.Take(AppleMacGcrFormat.TagByteCount).ToArray(); sectorData = decoded.Skip(AppleMacGcrFormat.TagByteCount).Take(AppleMacGcrFormat.SectorByteCount).ToArray(); bytes.AddRange(sectorData); structureEnd = dataOffset + markBits + dataSymbols * BitPrimitives.BitsPerByte;
                    structures.Add(new(FluxStructureKind.AppleData, dataOffset, structureEnd - dataOffset, $"{FluxStructureDescriptions.Identity("Apple Macintosh", FluxStructureKind.AppleData, cylinder, head, number, AppleMacGcrFormat.SectorByteCount, null, null)}, {FluxStructureDescriptions.Integrity("checksum", dataValid)}"));
                }
                else structures.Add(new(FluxStructureKind.AppleData, dataOffset, markBits, FluxStructureDescriptions.Truncated("Apple Macintosh", FluxStructureKind.AppleData, null, "checksum unavailable")));
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, head, number, AppleMacGcrFormat.SectorSizeCode, AppleMacGcrFormat.SectorByteCount, integrity, offset, SectorIntegrityKind.Checksum, sectorData, sectorTag));
            structures.Add(new(FluxStructureKind.AppleAddress, offset, Math.Max(markBits, headerEnd - offset), FluxStructureDescriptions.Complete("Apple Macintosh", FluxStructureKind.AppleAddress, cylinder, head, number, AppleMacGcrFormat.SectorByteCount, null, null, headerValid, dataValid, "address checksum", "data checksum")));
            offset = headerValid == true ? Math.Max(offset + markBits - 1, structureEnd - 1) : offset + markBits - 1;
        }
        for (var offset = 0; offset < trackBitLength && offset + markBits <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, AppleMacGcrFormat.DataMark) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.AppleData, offset, markBits, FluxStructureDescriptions.UnpairedData("Apple Macintosh", null, "data prologue"))); offset += markBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 24d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    /// <summary>Exécute le traitement « Decode Six And Two » propre à ce format.</summary>
    /// <param name="symbols">Symboles GCR six-et-deux.</param><param name="checksum">Somme de contrôle reconstruite.</param><returns>Bloc tagué décodé.</returns>
    private static byte[] DecodeSixAndTwo(ReadOnlySpan<byte> symbols, out byte[] checksum)
    {
        var b1 = new byte[AppleMacGcrFormat.GroupByteCount]; var b2 = new byte[AppleMacGcrFormat.GroupByteCount]; var b3 = new byte[AppleMacGcrFormat.GroupByteCount]; var source = 0;
        for (var index = 0; index <= AppleMacGcrFormat.LastGroupIndex; index++)
        {
            var w4 = symbols[source++]; var w1 = symbols[source++]; var w2 = symbols[source++]; var w3 = index == AppleMacGcrFormat.LastGroupIndex ? (byte)0 : symbols[source++];
            b1[index] = (byte)((w1 & AppleMacGcrFormat.SixBitMask) | ((w4 << AppleMacGcrFormat.ThirdChecksumShift) & AppleMacGcrFormat.EncodedHighBitsMask)); b2[index] = (byte)((w2 & AppleMacGcrFormat.SixBitMask) | ((w4 << AppleMacGcrFormat.SecondChecksumShift) & AppleMacGcrFormat.EncodedHighBitsMask)); b3[index] = (byte)((w3 & AppleMacGcrFormat.SixBitMask) | ((w4 << AppleMacGcrFormat.FirstChecksumShift) & AppleMacGcrFormat.EncodedHighBitsMask));
        }
        var output = new byte[AppleMacGcrFormat.TaggedSectorByteCount]; uint c1 = 0, c2 = 0, c3 = 0; var destination = 0;
        for (var index = 0; ; index++)
        {
            c1 = (c1 & AppleMacGcrFormat.ChecksumByteMask) << 1; if ((c1 & AppleMacGcrFormat.ChecksumCarryBit) != 0) c1++;
            var value = (byte)(b1[index] ^ c1); c3 += value; if ((c1 & AppleMacGcrFormat.ChecksumCarryBit) != 0) { c3++; c1 &= AppleMacGcrFormat.ChecksumByteMask; } output[destination++] = value;
            value = (byte)(b2[index] ^ c3); c2 += value; if (c3 > AppleMacGcrFormat.ChecksumByteMask) { c2++; c3 &= AppleMacGcrFormat.ChecksumByteMask; } output[destination++] = value;
            if (destination == AppleMacGcrFormat.TaggedSectorByteCount) break;
            value = (byte)(b3[index] ^ c2); c1 += value; if (c2 > AppleMacGcrFormat.ChecksumByteMask) { c1++; c2 &= AppleMacGcrFormat.ChecksumByteMask; } output[destination++] = value;
        }
        checksum = [(byte)(c1 & AppleMacGcrFormat.SixBitMask), (byte)(c2 & AppleMacGcrFormat.SixBitMask), (byte)(c3 & AppleMacGcrFormat.SixBitMask), (byte)(((c1 & AppleMacGcrFormat.ChecksumHighBitsMask) >> AppleMacGcrFormat.FirstChecksumShift) | ((c2 & AppleMacGcrFormat.ChecksumHighBitsMask) >> AppleMacGcrFormat.SecondChecksumShift) | ((c3 & AppleMacGcrFormat.ChecksumHighBitsMask) >> AppleMacGcrFormat.ThirdChecksumShift))];
        return output;
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
