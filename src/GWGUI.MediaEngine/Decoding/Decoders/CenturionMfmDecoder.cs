using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Centurion MFM.</summary>
public sealed class CenturionMfmDecoder : SignatureMfmDecoder
{
    /// <summary>Conserve la définition « Sector Mark » utilisée par ce codec.</summary>
    private static readonly byte[] SectorMark = CenturionMfmFormat.SectorMark.ToArray();
    /// <summary>Conserve la définition « Data Mark » utilisée par ce codec.</summary>
    private static readonly byte[] DataMark = CenturionMfmFormat.DataMark.ToArray();
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public override string Id => FluxCodecIds.CenturionMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public override string DisplayName => FluxCodecDisplayNames.CenturionMfm;
    /// <summary>Expose les motifs binaires reconnus dans le flux.</summary>
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "Centurion sector mark"), (DataMark, FluxStructureKind.FormatData, "Centurion data mark")];

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        var markBits = SectorMark.Length * BitPrimitives.BitsPerByte;
        var headerBits = markBits + CenturionMfmFormat.HeaderByteCount * 16;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = TryDecodeMfmBytes(stream, offset + markBits, CenturionMfmFormat.HeaderByteCount);
                if (header is null) continue;
                var headerValid = Primitives.Crc16Calculator.Compute(header, CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue) == 0; bytes.AddRange(header);
                var dataOffset = FindDataMark(stream, offset + headerBits + 400); bool? dataValid = null; var size = 0; var structureEnd = offset + headerBits;
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset); var prefixEnd = dataOffset + markBits + CenturionMfmFormat.DataPrefixByteCount * 16;
                    if (prefixEnd <= stream.Bits.Length)
                    {
                        var prefix = TryDecodeMfmBytes(stream, dataOffset + markBits, CenturionMfmFormat.DataPrefixByteCount);
                        if (prefix is null) continue;
                        var key = prefix[0]; size = (prefix[1] << BitPrimitives.BitsPerByte) | prefix[2];
                        var dataEnd = (long)prefixEnd + (size + 2L) * 16;
                        if (key == CenturionMfmFormat.SupportedDataKey && size > 0 && dataEnd <= stream.Bits.Length)
                        {
                            var block = TryDecodeMfmBytes(stream, dataOffset + markBits + 16, size + 4);
                            if (block is null) continue;
                            dataValid = Primitives.Crc16Calculator.Compute(block, CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue) == 0; bytes.AddRange(block.Skip(CenturionMfmFormat.CrcByteCount).Take(size)); structureEnd = (int)dataEnd;
                            structures.Add(new(FluxStructureKind.FormatData, dataOffset, (int)dataEnd - dataOffset, $"{FluxStructureDescriptions.Identity("Centurion", FluxStructureKind.FormatData, header[0], 0, header[1], size, null, null)}, {FluxStructureDescriptions.Integrity("CRC", dataValid)}"));
                        }
                        else structures.Add(new(FluxStructureKind.FormatData, dataOffset, markBits + CenturionMfmFormat.DataPrefixByteCount * 16, FluxStructureDescriptions.Truncated("Centurion", FluxStructureKind.FormatData, null, key == CenturionMfmFormat.SupportedDataKey ? "CRC unavailable" : $"unsupported key {key}")));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, markBits, FluxStructureDescriptions.Truncated("Centurion", FluxStructureKind.FormatData, null, "CRC unavailable")));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(header[0], 0, header[1], SizeCode(size), size, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, FluxStructureDescriptions.Complete("Centurion", FluxStructureKind.FormatHeader, header[0], 0, header[1], size, null, null, headerValid, dataValid)));
                offset = Math.Max(offset + markBits - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, FluxStructureDescriptions.Truncated("Centurion", FluxStructureKind.FormatHeader, null, "sector mark")));
            if (!complete) offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, DataMark) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, FluxStructureDescriptions.UnpairedData("Centurion", null, "data block"))); offset += markBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Recherche la prochaine marque de données.</summary>
    private static int FindDataMark(FluxBitstream stream, int start)
    {
        for (var offset = Math.Max(0, start); offset + DataMark.Length * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++) { if (FluxBitReader.MatchBytes(stream, offset, SectorMark)) return -1; if (FluxBitReader.MatchBytes(stream, offset, DataMark)) return offset; }
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
