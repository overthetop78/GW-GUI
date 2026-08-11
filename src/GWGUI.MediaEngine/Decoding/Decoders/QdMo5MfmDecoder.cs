using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Qd Mo5 MFM.</summary>
public sealed class QdMo5MfmDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Header Mark » utilisée par ce codec.</summary>
    private static readonly byte[] HeaderMark = QdMo5MfmFormat.HeaderMark.ToArray();
    /// <summary>Conserve la définition « Data Mark » utilisée par ce codec.</summary>
    private static readonly byte[] DataMark = QdMo5MfmFormat.DataMark.ToArray();
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.QdMo5Mfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.QdMo5Mfm;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><returns>Résultat du décodage QD MO5 MFM.</returns>
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedDataMarks = new HashSet<int>();
        var markBits = HeaderMark.Length * BitPrimitives.BitsPerByte;
        const int headerBits = 10 * BitPrimitives.BitsPerByte + (QdMo5MfmFormat.SectorNumberByteCount + QdMo5MfmFormat.HeaderPaddingByteCount) * 16;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, HeaderMark)) continue;
            if (offset + headerBits > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, FluxStructureDescriptions.Truncated("QD MO5", FluxStructureKind.FormatHeader, null, "sector header")));
                offset += markBits - 1; continue;
            }

            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + markBits, out var high)) continue;
            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + markBits + 16, out var low)) continue;
            var number = (high << BitPrimitives.BitsPerByte) | low; bytes.Add(high); bytes.Add(low);
            var dataOffset = FindNextData(stream, offset + headerBits, QdMo5MfmFormat.DataSearchByteCount * BitPrimitives.BitsPerByte);
            var completeData = dataOffset >= 0 && dataOffset + 10 * BitPrimitives.BitsPerByte + (QdMo5MfmFormat.DataPrefixByteCount + QdMo5MfmFormat.SectorSize + QdMo5MfmFormat.ChecksumByteCount) * 16 <= stream.Bits.Length;
            bool? checksumValid = null;
            if (completeData)
            {
                var block = TryDecodeMfmBytes(stream, dataOffset + 10 * BitPrimitives.BitsPerByte, QdMo5MfmFormat.DataPrefixByteCount + QdMo5MfmFormat.SectorSize + QdMo5MfmFormat.ChecksumByteCount);
                if (block is null) continue;
                byte checksum = 0; var data = new byte[QdMo5MfmFormat.SectorSize];
                for (var index = 0; index < QdMo5MfmFormat.DataPrefixByteCount + QdMo5MfmFormat.SectorSize; index++) { var value = block[index]; checksum += value; if (index > 0) data[index - 1] = value; }
                var stored = block[QdMo5MfmFormat.DataPrefixByteCount + QdMo5MfmFormat.SectorSize]; checksumValid = checksum == stored;
                pairedDataMarks.Add(dataOffset); bytes.AddRange(data);
                structures.Add(new(FluxStructureKind.FormatData, dataOffset, 10 * BitPrimitives.BitsPerByte + 130 * 16, $"{FluxStructureDescriptions.Identity("QD MO5", FluxStructureKind.FormatData, 0, 0, number, QdMo5MfmFormat.SectorSize, null, null)}, {FluxStructureDescriptions.Integrity("checksum", checksumValid)}"));
            }
            sectors.Add(new(0, 0, number, 0, QdMo5MfmFormat.SectorSize, checksumValid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, FluxStructureDescriptions.Complete("QD MO5", FluxStructureKind.FormatHeader, 0, 0, number, QdMo5MfmFormat.SectorSize, null, null, true, checksumValid, "header", "data checksum")));
            offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!pairedDataMarks.Contains(offset) && FluxBitReader.MatchBytes(stream, offset, DataMark)) structures.Add(new(FluxStructureKind.FormatData, offset, markBits, FluxStructureDescriptions.UnpairedData("QD MO5", null, "sector data")));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }

    /// <summary>Recherche la prochaine marque de données avant un nouvel en-tête.</summary>
    /// <param name="stream">Flux source.</param><param name="start">Offset initial en bits.</param><param name="maximumDistance">Distance maximale en bits.</param><returns>Offset trouvé, ou <c>-1</c>.</returns>
    private static int FindNextData(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - DataMark.Length * BitPrimitives.BitsPerByte, start + maximumDistance);
        for (var offset = start; offset <= end; offset++)
        {
            if (FluxBitReader.MatchBytes(stream, offset, DataMark)) return offset;
            if (FluxBitReader.MatchBytes(stream, offset, HeaderMark)) return -1;
        }
        return -1;
    }
}
