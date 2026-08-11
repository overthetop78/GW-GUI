using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Tycom FM.</summary>
public sealed class TycomFmDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Header Mark » utilisée par ce codec.</summary>
    private static readonly byte[] HeaderMark = TycomFmFormat.HeaderMark.ToArray();
    /// <summary>Conserve la définition « Data Marks » utilisée par ce codec.</summary>
    private static readonly (byte[] Pattern, byte Mark)[] DataMarks = TycomFmFormat.DataMarks.Select(item => (item.Pattern.ToArray(),item.Mark)).ToArray();
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.TycomFm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.TycomFm;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><returns>Résultat du décodage TYCOM FM.</returns>
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var classifiedData = new HashSet<int>();
        var markBits = HeaderMark.Length * BitPrimitives.BitsPerByte;
        const int headerBits = (TycomFmFormat.HeaderDecodedByteCount + 1) * 32;
        const int sectorSize = TycomFmFormat.SectorSize;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, HeaderMark)) continue;
            if (offset + headerBits > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, FluxStructureDescriptions.Truncated("TYCOM", FluxStructureKind.FormatHeader, null, "sector header"))); offset += markBits - 1; continue;
            }
            var header = TryDecodeFmBytes(stream, offset + HeaderMark.Length * BitPrimitives.BitsPerByte, TycomFmFormat.HeaderDecodedByteCount);
            if (header is null) continue;
            var cylinder = header[0]; var number = header[1];
            var crcHigh = header[2]; var crcLow = header[3];
            if (Primitives.Crc16Calculator.Compute([TycomFmFormat.HeaderAddressMark, cylinder, (byte)number, crcHigh, crcLow], TycomFmFormat.CrcPolynomial, TycomFmFormat.CrcInitialValue) != 0)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"{FluxStructureDescriptions.Identity("TYCOM", FluxStructureKind.FormatHeader, cylinder, 0, number, 0, null, null)}, {FluxStructureDescriptions.Integrity("header CRC", false)}")); offset += markBits - 1; continue;
            }
            bytes.AddRange([cylinder, (byte)number]);

            var data = FindNextDataMark(stream, offset + headerBits, TycomFmFormat.DataSearchByteCount * BitPrimitives.BitsPerByte * 2);
            var completeData = data.Offset >= 0 && data.Offset + (1 + sectorSize + TycomFmFormat.CrcByteCount) * 32 <= stream.Bits.Length;
            bool? dataCrcValid = null;
            if (completeData)
            {
                var block = TryDecodeFmBytes(stream, data.Offset, 1 + sectorSize + TycomFmFormat.CrcByteCount);
                if (block is null) continue;
                ushort crc = TycomFmFormat.CrcInitialValue; var payload = new byte[sectorSize];
                for (var index = 0; index < block.Length; index++) { var value = block[index]; crc = Primitives.Crc16Calculator.Update(crc, value, TycomFmFormat.CrcPolynomial); if (index is > 0 and <= sectorSize) payload[index - 1] = value; }
                dataCrcValid = crc == 0; classifiedData.Add(data.Offset); bytes.AddRange(payload);
                structures.Add(new(FluxStructureKind.FormatData, data.Offset, (1 + sectorSize + 2) * 32, $"{FluxStructureDescriptions.Identity("TYCOM", FluxStructureKind.FormatData, cylinder, 0, number, sectorSize, data.Mark, null)}, {FluxStructureDescriptions.Integrity("CRC", dataCrcValid)}"));
            }
            sectors.Add(new(cylinder, 0, number, 0, sectorSize, dataCrcValid, offset));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, FluxStructureDescriptions.Complete("TYCOM", FluxStructureKind.FormatHeader, cylinder, 0, number, sectorSize, completeData ? data.Mark : null, null, true, dataCrcValid)));
            offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (classifiedData.Contains(offset)) continue;
            foreach (var item in DataMarks) if (FluxBitReader.MatchBytes(stream, offset, item.Pattern)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, FluxStructureDescriptions.UnclassifiedMark("TYCOM", FluxStructureKind.FormatData, item.Mark, "data"))); offset += markBits - 1; break; }
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    /// <summary>Recherche la prochaine marque de données dans la distance autorisée.</summary>
    /// <param name="stream">Flux source.</param><param name="start">Offset initial en bits.</param><param name="maximumDistance">Distance maximale en bits.</param><returns>Offset et marque trouvés, ou un offset négatif.</returns>
    private static (int Offset, byte Mark) FindNextDataMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - HeaderMark.Length * BitPrimitives.BitsPerByte, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) foreach (var item in DataMarks) if (FluxBitReader.MatchBytes(stream, offset, item.Pattern)) return (offset, item.Mark);
        return (-1, 0);
    }

    /// <summary>Tente de décoder une suite d'octets FM.</summary>
    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeFmByte32(stream, offset + index * 32, out result[index])) return null;
        return result;
    }
}
