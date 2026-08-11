using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Aed6200p MFM.</summary>
public sealed class Aed6200pMfmDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Sector Header » utilisée par ce codec.</summary>
    private static readonly byte[] SectorHeader = Aed6200pMfmFormat.HeaderPattern.ToArray();
    /// <summary>Conserve la définition « Sector Data » utilisée par ce codec.</summary>
    private static readonly byte[][] SectorData = Aed6200pMfmFormat.DataPatterns.Select(pattern => pattern.ToArray()).ToArray();
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.Aed6200pMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.Aed6200pMfm;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int headerBits = Aed6200pMfmFormat.HeaderByteCount * 16; var pairedData = new HashSet<int>();
        for (var offset = 0; offset + SectorHeader.Length * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorHeader)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = TryDecodeMfmBytes(stream, offset, Aed6200pMfmFormat.HeaderByteCount);
                if (header is null) continue;
                var size = (header[4] << BitPrimitives.BitsPerByte) | header[2]; var headerValid = header[0] == Aed6200pMfmFormat.HeaderAddressMark && Primitives.Crc16Calculator.Compute(header) == 0; bytes.AddRange(header);
                var dataOffset = FindDataMark(stream, offset + 1, Math.Min(stream.Bits.Length, offset + 104 * BitPrimitives.BitsPerByte));
                bool? dataValid = null; var structureEnd = offset + headerBits;
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset); var dataBlockBytes = Aed6200pMfmFormat.DataMarkByteCount + size + Aed6200pMfmFormat.CrcByteCount; var dataEnd = (long)dataOffset + dataBlockBytes * 16L;
                    if (size > 0 && dataEnd <= stream.Bits.Length)
                    {
                        var data = TryDecodeMfmBytes(stream, dataOffset, dataBlockBytes);
                        if (data is null) continue;
                        dataValid = data[0] is >= Aed6200pMfmFormat.DeletedDataMark and <= Aed6200pMfmFormat.DataMark && Primitives.Crc16Calculator.Compute(data) == 0; bytes.AddRange(data.Skip(Aed6200pMfmFormat.DataMarkByteCount).Take(size)); structureEnd = (int)dataEnd;
                        structures.Add(new(FluxStructureKind.FormatData, dataOffset, (int)dataEnd - dataOffset, $"{FluxStructureDescriptions.Identity("AED 6200P", FluxStructureKind.FormatData, 0, 0, 0, size, data[0], null)}, {FluxStructureDescriptions.Integrity("CRC", dataValid)}"));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, 16, FluxStructureDescriptions.Truncated("AED 6200P", FluxStructureKind.FormatData, null, "CRC unavailable")));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(header[1], 0, header[3], SizeCode(size), size, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, FluxStructureDescriptions.Complete("AED 6200P", FluxStructureKind.FormatHeader, header[1], 0, header[3], size, null, null, headerValid, dataValid)));
                offset = Math.Max(offset + SectorHeader.Length * BitPrimitives.BitsPerByte - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, SectorHeader.Length * BitPrimitives.BitsPerByte, FluxStructureDescriptions.Truncated("AED 6200P", FluxStructureKind.FormatHeader, Aed6200pMfmFormat.HeaderAddressMark, null)));
            if (!complete) offset += SectorHeader.Length * BitPrimitives.BitsPerByte - 1;
        }
        for (var offset = 0; offset + 16 <= stream.Bits.Length; offset++) if (SectorData.Any(mark => FluxBitReader.MatchBytes(stream, offset, mark)) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, 16, FluxStructureDescriptions.UnpairedData("AED 6200P", null, null))); offset += 15; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Recherche la prochaine marque de données.</summary>
    private static int FindDataMark(FluxBitstream stream, int start, int end)
    {
        for (var offset = Math.Max(0, start); offset + 16 <= end; offset++) if (SectorData.Any(mark => FluxBitReader.MatchBytes(stream, offset, mark))) return offset;
        return -1;
    }

    /// <summary>Détermine le code représentant la taille du secteur.</summary>
    private static byte SizeCode(int size)
    {
        for (byte code = 0; code < 8; code++) if ((128 << code) == size) return code;
        return 0;
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
