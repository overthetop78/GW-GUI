using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Aed6200p MFM.</summary>
public sealed class Aed6200pMfmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.Aed6200pMfm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.Aed6200pMfm;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP dont les intervalles de flux sont décodés en MFM.</param>
    /// <returns>Résultat contenant les structures, les secteurs, les octets décodés et la durée estimée d'une cellule.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int headerBits = Aed6200pMfmFormat.HeaderByteCount * 16; var pairedData = new HashSet<int>();
        for (var offset = 0; offset + Aed6200pMfmFormat.HeaderPattern.Count * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, Aed6200pMfmFormat.HeaderPattern)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = TryDecodeMfmBytes(stream, offset, Aed6200pMfmFormat.HeaderByteCount);
                if (header is null) continue;
                var size = (header[Aed6200pMfmFormat.SizeHighOffset] << BitPrimitives.BitsPerByte) | header[Aed6200pMfmFormat.SizeLowOffset]; var headerValid = header[Aed6200pMfmFormat.HeaderMarkOffset] == Aed6200pMfmFormat.HeaderAddressMark && Primitives.Crc16Calculator.Compute(header) == 0; bytes.AddRange(header);
                var dataOffset = FindDataMark(stream, offset + 1, Math.Min(stream.Bits.Length, offset + Aed6200pMfmFormat.DataSearchWindowByteCount * BitPrimitives.BitsPerByte));
                bool? dataValid = null; var structureEnd = offset + headerBits;
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset); var dataBlockBytes = Aed6200pMfmFormat.DataMarkByteCount + size + Aed6200pMfmFormat.CrcByteCount; var dataEnd = (long)dataOffset + dataBlockBytes * 16L;
                    if (size > 0 && dataEnd <= stream.Bits.Length)
                    {
                        var data = TryDecodeMfmBytes(stream, dataOffset, dataBlockBytes);
                        if (data is null) continue;
                        dataValid = data[Aed6200pMfmFormat.HeaderMarkOffset] is >= Aed6200pMfmFormat.FirstDataAddressMark and <= Aed6200pMfmFormat.LastDataAddressMark && Primitives.Crc16Calculator.Compute(data) == 0; bytes.AddRange(data.Skip(Aed6200pMfmFormat.DataMarkByteCount).Take(size)); structureEnd = (int)dataEnd;
                        structures.Add(new(FluxStructureKind.FormatData, dataOffset, (int)dataEnd - dataOffset, $"{FluxStructureDescriptions.Identity("AED 6200P", FluxStructureKind.FormatData, 0, 0, 0, size, data[Aed6200pMfmFormat.HeaderMarkOffset], null)}, {FluxStructureDescriptions.Integrity("CRC", dataValid)}"));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, 16, FluxStructureDescriptions.Truncated("AED 6200P", FluxStructureKind.FormatData, null, "CRC unavailable")));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(header[Aed6200pMfmFormat.CylinderOffset], 0, header[Aed6200pMfmFormat.SectorOffset], SizeCode(size), size, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, FluxStructureDescriptions.Complete("AED 6200P", FluxStructureKind.FormatHeader, header[Aed6200pMfmFormat.CylinderOffset], 0, header[Aed6200pMfmFormat.SectorOffset], size, null, null, headerValid, dataValid)));
                offset = Math.Max(offset + Aed6200pMfmFormat.HeaderPattern.Count * BitPrimitives.BitsPerByte - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, Aed6200pMfmFormat.HeaderPattern.Count * BitPrimitives.BitsPerByte, FluxStructureDescriptions.Truncated("AED 6200P", FluxStructureKind.FormatHeader, Aed6200pMfmFormat.HeaderAddressMark, null)));
            if (!complete) offset += Aed6200pMfmFormat.HeaderPattern.Count * BitPrimitives.BitsPerByte - 1;
        }
        for (var offset = 0; offset + 16 <= stream.Bits.Length; offset++) if (Aed6200pMfmFormat.DataPatterns.Any(mark => FluxBitReader.MatchBytes(stream, offset, mark)) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, 16, FluxStructureDescriptions.UnpairedData("AED 6200P", null, null))); offset += 15; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Recherche la prochaine marque de données.</summary>
    /// <param name="stream">Flux binaire MFM à parcourir.</param>
    /// <param name="start">Offset de départ inclus, exprimé en bits.</param>
    /// <param name="end">Offset de fin exclu, exprimé en bits.</param>
    /// <returns>Offset en bits de la première marque trouvée, ou <c>-1</c> si aucune marque complète n'est présente dans l'intervalle.</returns>
    private static int FindDataMark(FluxBitstream stream, int start, int end)
    {
        for (var offset = Math.Max(0, start); offset + 16 <= end; offset++) if (Aed6200pMfmFormat.DataPatterns.Any(mark => FluxBitReader.MatchBytes(stream, offset, mark))) return offset;
        return -1;
    }

    /// <summary>Détermine le code représentant la taille du secteur.</summary>
    /// <param name="size">Taille du secteur en octets.</param>
    /// <returns>Code de taille correspondant à une puissance de deux à partir de 128 octets, ou zéro en l'absence de correspondance.</returns>
    private static byte SizeCode(int size)
    {
        for (byte code = 0; code < 8; code++) if ((128 << code) == size) return code;
        return 0;
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    /// <param name="stream">Flux binaire MFM source.</param>
    /// <param name="offset">Offset du premier octet, exprimé en bits.</param>
    /// <param name="count">Nombre d'octets MFM à décoder.</param>
    /// <returns>Octets décodés, ou <see langword="null"/> dès qu'un octet MFM est incomplet ou invalide.</returns>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
