using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
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
    /// <param name="revolution">Révolution générique dont les intervalles de flux sont décodés en MFM.</param>
    /// <returns>Résultat contenant les structures, les secteurs, les octets décodés et la durée estimée d'une cellule.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        const int headerBits = Aed6200pMfmFormat.HeaderByteCount * MfmEncoding.EncodedByteBitCount;
        var pairedData = new HashSet<int>();
        for (var offset = 0; offset + Aed6200pMfmFormat.HeaderPattern.Count * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, Aed6200pMfmFormat.HeaderPattern)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                if (!TryReadHeader(stream, offset, out var header, out var size, out var headerValid)) continue;
                bytes.AddRange(header);
                var data = ReadDataBlock(stream, offset, headerBits, size, pairedData, structures, bytes);
                if (!data.HeaderCanBeAdded) continue;
                var integrity = CombineIntegrity(headerValid, data.Valid);
                sectors.Add(new(header[Aed6200pMfmFormat.CylinderOffset], 0, header[Aed6200pMfmFormat.SectorOffset], SectorSizeCode.FromByteCount(size), size, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, FluxStructureDescriptions.Complete("AED 6200P", FluxStructureKind.FormatHeader, header[Aed6200pMfmFormat.CylinderOffset], 0, header[Aed6200pMfmFormat.SectorOffset], size, null, null, headerValid, data.Valid)));
                offset = Math.Max(offset + Aed6200pMfmFormat.HeaderPattern.Count * BitPrimitives.BitsPerByte - 1, data.StructureEnd - 1);
            }
            else
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, Aed6200pMfmFormat.HeaderPattern.Count * BitPrimitives.BitsPerByte, FluxStructureDescriptions.Truncated("AED 6200P", FluxStructureKind.FormatHeader, Aed6200pMfmFormat.HeaderAddressMark, null)));
            }
            if (!complete) offset += Aed6200pMfmFormat.HeaderPattern.Count * BitPrimitives.BitsPerByte - 1;
        }
        AddUnpairedDataMarks(stream, pairedData, structures);
        return new(Id, DisplayName, FluxDecoderConfidence.CalculateStandard(sectors.Count, structures.Count), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Tente de lire et de valider un en-tête AED 6200P.</summary>
    /// <param name="stream">Flux binaire MFM source.</param><param name="offset">Position de l'en-tête, en bits.</param><param name="header">En-tête décodé.</param><param name="size">Taille sectorielle déclarée, en octets.</param><param name="valid">Validité de la marque et du CRC d'en-tête.</param><returns><see langword="true"/> lorsque l'en-tête peut être décodé.</returns>
    private static bool TryReadHeader(FluxBitstream stream, int offset, out byte[] header, out int size, out bool valid)
    {
        header = TryDecodeMfmBytes(stream, offset, Aed6200pMfmFormat.HeaderByteCount)!;
        if (header is null)
        {
            size = 0;
            valid = false;
            return false;
        }
        size = (header[Aed6200pMfmFormat.SizeHighOffset] << BitPrimitives.BitsPerByte) | header[Aed6200pMfmFormat.SizeLowOffset];
        valid = header[Aed6200pMfmFormat.HeaderMarkOffset] == Aed6200pMfmFormat.HeaderAddressMark && Primitives.Crc16Calculator.Compute(header) == 0;
        return true;
    }

    /// <summary>Recherche, décode et valide le bloc de données associé à un en-tête.</summary>
    /// <param name="stream">Flux binaire MFM source.</param><param name="headerOffset">Position de l'en-tête, en bits.</param><param name="headerBits">Longueur de l'en-tête, en bits.</param><param name="size">Taille des données, en octets.</param><param name="pairedData">Positions des marques de données déjà appariées.</param><param name="structures">Structures auxquelles ajouter le bloc détecté.</param><param name="bytes">Octets auxquels ajouter la charge utile décodée.</param><returns>Résultat de la lecture du bloc de données.</returns>
    private static AedDataReadResult ReadDataBlock(FluxBitstream stream, int headerOffset, int headerBits, int size, HashSet<int> pairedData, List<FluxStructure> structures, List<byte> bytes)
    {
        var dataOffset = FindDataMark(stream, headerOffset + 1, Math.Min(stream.Bits.Length, headerOffset + Aed6200pMfmFormat.DataSearchWindowByteCount * BitPrimitives.BitsPerByte));
        if (dataOffset < 0) return new(true, null, headerOffset + headerBits);
        pairedData.Add(dataOffset);
        var dataBlockBytes = Aed6200pMfmFormat.DataMarkByteCount + size + Aed6200pMfmFormat.CrcByteCount;
        var dataEnd = (long)dataOffset + dataBlockBytes * MfmEncoding.EncodedByteBitCount;
        if (size <= 0 || dataEnd > stream.Bits.Length)
        {
            structures.Add(new(FluxStructureKind.FormatData, dataOffset, MfmEncoding.EncodedByteBitCount, FluxStructureDescriptions.Truncated("AED 6200P", FluxStructureKind.FormatData, null, "CRC unavailable")));
            return new(true, null, headerOffset + headerBits);
        }
        var data = TryDecodeMfmBytes(stream, dataOffset, dataBlockBytes);
        if (data is null) return new(false, null, headerOffset + headerBits);
        var valid = data[Aed6200pMfmFormat.HeaderMarkOffset] is >= Aed6200pMfmFormat.FirstDataAddressMark and <= Aed6200pMfmFormat.LastDataAddressMark && Primitives.Crc16Calculator.Compute(data) == 0;
        bytes.AddRange(data.Skip(Aed6200pMfmFormat.DataMarkByteCount).Take(size));
        structures.Add(new(FluxStructureKind.FormatData, dataOffset, (int)dataEnd - dataOffset, FluxStructureDescriptions.WithIntegrity("AED 6200P", FluxStructureKind.FormatData, 0, 0, 0, size, data[Aed6200pMfmFormat.HeaderMarkOffset], null, "CRC", valid)));
        return new(true, valid, (int)dataEnd);
    }

    /// <summary>Combine la validité de l'en-tête et celle des données.</summary>
    /// <param name="headerValid">Validité de l'en-tête.</param><param name="dataValid">Validité des données, ou valeur nulle en leur absence.</param><returns>Intégrité globale du secteur.</returns>
    private static bool? CombineIntegrity(bool headerValid, bool? dataValid) => headerValid == false || dataValid == false ? false : dataValid is null ? null : true;

    /// <summary>Ajoute les marques de données qui n'ont été associées à aucun en-tête.</summary>
    /// <param name="stream">Flux binaire MFM source.</param><param name="pairedData">Positions déjà appariées.</param><param name="structures">Structures auxquelles ajouter les marques restantes.</param>
    private static void AddUnpairedDataMarks(FluxBitstream stream, HashSet<int> pairedData, List<FluxStructure> structures)
    {
        for (var offset = 0; offset + MfmEncoding.EncodedByteBitCount <= stream.Bits.Length; offset++)
        {
            if (!Aed6200pMfmFormat.DataPatterns.Any(mark => FluxBitReader.MatchBytes(stream, offset, mark)) || pairedData.Contains(offset)) continue;
            structures.Add(new(FluxStructureKind.FormatData, offset, MfmEncoding.EncodedByteBitCount, FluxStructureDescriptions.UnpairedData("AED 6200P", null, null)));
            offset += MfmEncoding.EncodedByteBitCount - 1;
        }
    }

    /// <summary>Décrit le résultat de la lecture d'un bloc de données AED.</summary>
    /// <param name="HeaderCanBeAdded">Indique si le traitement de l'en-tête peut se poursuivre.</param><param name="Valid">Validité du bloc, ou valeur nulle lorsqu'elle est indisponible.</param><param name="StructureEnd">Position de fin de la structure, en bits.</param>
    private readonly record struct AedDataReadResult(bool HeaderCanBeAdded, bool? Valid, int StructureEnd);

    /// <summary>Recherche la prochaine marque de données.</summary>
    /// <param name="stream">Flux binaire MFM à parcourir.</param>
    /// <param name="start">Offset de départ inclus, exprimé en bits.</param>
    /// <param name="end">Offset de fin exclu, exprimé en bits.</param>
    /// <returns>Offset en bits de la première marque trouvée, ou <c>-1</c> si aucune marque complète n'est présente dans l'intervalle.</returns>
    private static int FindDataMark(FluxBitstream stream, int start, int end)
    {
        for (var offset = Math.Max(0, start); offset + MfmEncoding.EncodedByteBitCount <= end; offset++)
        {
            if (Aed6200pMfmFormat.DataPatterns.Any(mark => FluxBitReader.MatchBytes(stream, offset, mark))) return offset;
        }
        return -1;
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    /// <param name="stream">Flux binaire MFM source.</param>
    /// <param name="offset">Offset du premier octet, exprimé en bits.</param>
    /// <param name="count">Nombre d'octets MFM à décoder.</param>
    /// <returns>Octets décodés, ou <see langword="null"/> dès qu'un octet MFM est incomplet ou invalide.</returns>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * MfmEncoding.EncodedByteBitCount, out result[index])) return null;
        }
        return result;
    }
}
