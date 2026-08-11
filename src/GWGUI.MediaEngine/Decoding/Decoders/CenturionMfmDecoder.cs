using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Centurion MFM.</summary>
public sealed class CenturionMfmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => CenturionMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => CenturionMfmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder en MFM Centurion.</param>
    /// <returns>Résultat contenant les structures, secteurs et octets reconnus.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var pairedData = new HashSet<int>();
        for (var offset = 0; offset + CenturionMfmFormat.SectorMarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, CenturionMfmFormat.SectorMark)) continue;
            if (offset + CenturionMfmFormat.HeaderBitCount > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, CenturionMfmFormat.SectorMarkBitCount, FluxStructureDescriptions.Truncated(CenturionMfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, null, CenturionMfmFormat.SectorMarkDescription)));
                offset += CenturionMfmFormat.SectorMarkAdvanceBitCount;
                continue;
            }
            var header = TryDecodeHeader(stream, offset + CenturionMfmFormat.SectorMarkBitCount);
            if (header is null) continue;
            bytes.AddRange(header.Value.Bytes);
            var cylinder = header.Value.Bytes[CenturionMfmFormat.HeaderCylinderOffset];
            var sector = header.Value.Bytes[CenturionMfmFormat.HeaderSectorOffset];
            var dataOffset = FindDataMark(stream, offset + CenturionMfmFormat.HeaderBitCount + CenturionMfmFormat.DataSearchDistanceBitCount);
            CenturionDataResult? data = null;
            var structureEnd = offset + CenturionMfmFormat.HeaderBitCount;
            if (dataOffset >= 0)
            {
                pairedData.Add(dataOffset);
                data = TryDecodeData(stream, dataOffset, cylinder, sector, structures);
                if (data?.Fatal == true) continue;
                if (data?.Data is not null) bytes.AddRange(data.Data);
                if (data is not null) structureEnd = data.EndOffset;
            }
            var size = data?.Size ?? 0;
            bool? integrity = header.Value.Valid == false || data?.Valid == false ? false : data?.Valid is null ? null : true;
            sectors.Add(new(cylinder, CenturionMfmFormat.LogicalHead, sector, SectorSizeCode.FromByteCount(size), size, integrity, offset, SectorIntegrityKind.Crc, data?.Data));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, CenturionMfmFormat.HeaderBitCount, CenturionMfmDescriptions.Header(cylinder, sector, size, header.Value.Valid, data?.Valid)));
            offset = Math.Max(offset + CenturionMfmFormat.SectorMarkAdvanceBitCount, structureEnd - 1);
        }
        AddUnpairedData(stream, structures, pairedData);
        return new(Id, DisplayName, FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, CenturionMfmFormat.ConfidenceSectorWeight, CenturionMfmFormat.ConfidenceDivisor), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Décode les quatre octets d'en-tête et contrôle leur CRC.</summary>
    /// <param name="stream">Flux binaire MFM.</param>
    /// <param name="offset">Position du premier octet encodé suivant la marque.</param>
    /// <returns>En-tête et validité de son CRC, ou <see langword="null"/> si le codage est invalide.</returns>
    private static (byte[] Bytes, bool Valid)? TryDecodeHeader(FluxBitstream stream, int offset)
    {
        var header = TryDecodeMfmBytes(stream, offset, CenturionMfmFormat.HeaderByteCount);
        return header is null ? null : (header, Crc16Calculator.Compute(header, CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue) == 0);
    }

    /// <summary>Lit, valide et décrit un bloc de données associé à un en-tête.</summary>
    /// <param name="stream">Flux binaire MFM.</param>
    /// <param name="dataOffset">Position de la marque de données.</param>
    /// <param name="cylinder">Cylindre lu dans l'en-tête.</param>
    /// <param name="sector">Secteur lu dans l'en-tête.</param>
    /// <param name="structures">Collection recevant la description du bloc.</param>
    /// <returns>Résultat de la lecture, ou <see langword="null"/> lorsqu'aucun résultat n'est disponible.</returns>
    private static CenturionDataResult? TryDecodeData(FluxBitstream stream, int dataOffset, byte cylinder, byte sector, List<FluxStructure> structures)
    {
        var prefixEnd = dataOffset + CenturionMfmFormat.DataPrefixBitCount;
        if (prefixEnd > stream.Bits.Length)
        {
            structures.Add(new(FluxStructureKind.FormatData, dataOffset, CenturionMfmFormat.DataMarkBitCount, CenturionMfmDescriptions.TruncatedData()));
            return new(0, null, null, dataOffset + CenturionMfmFormat.DataMarkBitCount, false);
        }
        var prefix = TryDecodeMfmBytes(stream, dataOffset + CenturionMfmFormat.DataMarkBitCount, CenturionMfmFormat.DataPrefixByteCount);
        if (prefix is null) return new(0, null, null, prefixEnd, true);
        var key = prefix[CenturionMfmFormat.DataKeyOffset];
        var size = (prefix[CenturionMfmFormat.DataSizeOffset] << BitPrimitives.BitsPerByte) | prefix[CenturionMfmFormat.DataSizeOffset + 1];
        var dataEnd = (long)prefixEnd + (size + CenturionMfmFormat.CrcByteCount) * CenturionMfmFormat.EncodedByteBitCount;
        if (key != CenturionMfmFormat.SupportedDataKey || size <= 0 || dataEnd > stream.Bits.Length)
        {
            structures.Add(new(FluxStructureKind.FormatData, dataOffset, CenturionMfmFormat.DataPrefixBitCount, CenturionMfmDescriptions.TruncatedData(key)));
            return new(size, null, null, prefixEnd, false);
        }
        var block = TryDecodeMfmBytes(stream, dataOffset + CenturionMfmFormat.DataMarkBitCount + CenturionMfmFormat.EncodedByteBitCount, size + CenturionMfmFormat.DataCrcPrefixByteCount + CenturionMfmFormat.CrcByteCount);
        if (block is null) return new(size, null, null, (int)dataEnd, true);
        var valid = Crc16Calculator.Compute(block, CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue) == 0;
        var data = block.Skip(CenturionMfmFormat.DataCrcPrefixByteCount).Take(size).ToArray();
        structures.Add(new(FluxStructureKind.FormatData, dataOffset, (int)dataEnd - dataOffset, CenturionMfmDescriptions.Data(cylinder, sector, size, key, valid)));
        return new(size, valid, data, (int)dataEnd, false);
    }

    /// <summary>Recherche la prochaine marque de données sans franchir un nouvel en-tête.</summary>
    /// <param name="stream">Flux binaire MFM.</param>
    /// <param name="start">Position de départ incluse.</param>
    /// <returns>Position de la marque de données, ou -1 si elle est absente ou précédée d'un nouvel en-tête.</returns>
    private static int FindDataMark(FluxBitstream stream, int start)
    {
        for (var offset = Math.Max(0, start); offset + CenturionMfmFormat.DataMarkBitCount <= stream.Bits.Length; offset++)
        {
            if (FluxBitReader.MatchBytes(stream, offset, CenturionMfmFormat.SectorMark)) return -1;
            if (FluxBitReader.MatchBytes(stream, offset, CenturionMfmFormat.DataMark)) return offset;
        }
        return -1;
    }

    /// <summary>Ajoute les marques de données qui ne sont associées à aucun en-tête.</summary>
    /// <param name="stream">Flux binaire MFM.</param>
    /// <param name="structures">Collection recevant les structures.</param>
    /// <param name="pairedData">Positions des marques déjà associées.</param>
    private static void AddUnpairedData(FluxBitstream stream, List<FluxStructure> structures, IReadOnlySet<int> pairedData)
    {
        for (var offset = 0; offset + CenturionMfmFormat.DataMarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, CenturionMfmFormat.DataMark) || pairedData.Contains(offset)) continue;
            structures.Add(new(FluxStructureKind.FormatData, offset, CenturionMfmFormat.DataMarkBitCount, CenturionMfmDescriptions.UnpairedData()));
            offset += CenturionMfmFormat.DataMarkAdvanceBitCount;
        }
    }

    /// <summary>Décode une suite d'octets MFM avec la primitive commune.</summary>
    /// <param name="stream">Flux binaire MFM.</param>
    /// <param name="offset">Position du premier octet encodé.</param>
    /// <param name="count">Nombre d'octets attendus.</param>
    /// <returns>Octets décodés, ou <see langword="null"/> si une cellule est invalide ou tronquée.</returns>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++)
            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * CenturionMfmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }

    /// <summary>Regroupe le résultat de lecture d'un bloc de données Centurion.</summary>
    /// <param name="Size">Taille déclarée de la charge utile.</param>
    /// <param name="Valid">Validité du CRC, ou valeur nulle lorsqu'elle est indisponible.</param>
    /// <param name="Data">Charge utile, ou valeur nulle lorsqu'elle n'a pas été décodée.</param>
    /// <param name="EndOffset">Position suivant le bloc lu.</param>
    /// <param name="Fatal">Indique que l'en-tête courant doit être abandonné pour conserver le comportement de décodage.</param>
    private sealed record CenturionDataResult(int Size, bool? Valid, byte[]? Data, int EndOffset, bool Fatal);
}
