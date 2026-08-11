using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Centurion MFM.</summary>
public sealed class CenturionMfmDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Sector Mark » utilisée par ce codec.</summary>
    private static readonly byte[] SectorMark = CenturionMfmFormat.SectorMark.ToArray();
    /// <summary>Conserve la définition « Data Mark » utilisée par ce codec.</summary>
    private static readonly byte[] DataMark = CenturionMfmFormat.DataMark.ToArray();
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => CenturionMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => CenturionMfmFormat.CodecDisplayName;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder en MFM Centurion.</param><returns>Résultat contenant les structures, secteurs et octets reconnus.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        var markBits = CenturionMfmFormat.SectorMarkBitCount;
        var headerBits = CenturionMfmFormat.HeaderBitCount;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorMark)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = TryDecodeMfmBytes(stream, offset + markBits, CenturionMfmFormat.HeaderByteCount);
                if (header is null) continue;
                var headerValid = Primitives.Crc16Calculator.Compute(header, CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue) == 0; bytes.AddRange(header);
                var dataOffset = FindDataMark(stream, offset + headerBits + CenturionMfmFormat.DataSearchDistanceBitCount); bool? dataValid = null; var size = 0; var structureEnd = offset + headerBits;
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset); var prefixEnd = dataOffset + CenturionMfmFormat.DataPrefixBitCount;
                    if (prefixEnd <= stream.Bits.Length)
                    {
                        var prefix = TryDecodeMfmBytes(stream, dataOffset + markBits, CenturionMfmFormat.DataPrefixByteCount);
                        if (prefix is null) continue;
                        var key = prefix[CenturionMfmFormat.DataKeyOffset]; size = (prefix[CenturionMfmFormat.DataSizeOffset] << BitPrimitives.BitsPerByte) | prefix[CenturionMfmFormat.DataSizeOffset + 1];
                        var dataEnd = (long)prefixEnd + (size + CenturionMfmFormat.CrcByteCount) * CenturionMfmFormat.EncodedByteBitCount;
                        if (key == CenturionMfmFormat.SupportedDataKey && size > 0 && dataEnd <= stream.Bits.Length)
                        {
                            var block = TryDecodeMfmBytes(stream, dataOffset + markBits + CenturionMfmFormat.EncodedByteBitCount, size + CenturionMfmFormat.DataCrcPrefixByteCount + CenturionMfmFormat.CrcByteCount);
                            if (block is null) continue;
                            dataValid = Primitives.Crc16Calculator.Compute(block, CenturionMfmFormat.CrcPolynomial, CenturionMfmFormat.CrcInitialValue) == 0; bytes.AddRange(block.Skip(CenturionMfmFormat.CrcByteCount).Take(size)); structureEnd = (int)dataEnd;
                            structures.Add(new(FluxStructureKind.FormatData, dataOffset, (int)dataEnd - dataOffset, FluxStructureDescriptions.WithIntegrity(CenturionMfmFormat.StructureDescriptionName, FluxStructureKind.FormatData, header[CenturionMfmFormat.HeaderCylinderOffset], CenturionMfmFormat.LogicalHead, header[CenturionMfmFormat.HeaderSectorOffset], size, null, null, CenturionMfmFormat.CrcDescription, dataValid)));
                        }
                        else structures.Add(new(FluxStructureKind.FormatData, dataOffset, CenturionMfmFormat.DataPrefixBitCount, FluxStructureDescriptions.Truncated(CenturionMfmFormat.StructureDescriptionName, FluxStructureKind.FormatData, null, key == CenturionMfmFormat.SupportedDataKey ? CenturionMfmFormat.UnavailableCrcDescription : CenturionMfmFormat.UnsupportedKeyDescription(key))));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, markBits, FluxStructureDescriptions.Truncated(CenturionMfmFormat.StructureDescriptionName, FluxStructureKind.FormatData, null, CenturionMfmFormat.UnavailableCrcDescription)));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(header[CenturionMfmFormat.HeaderCylinderOffset], CenturionMfmFormat.LogicalHead, header[CenturionMfmFormat.HeaderSectorOffset], SizeCode(size), size, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, FluxStructureDescriptions.Complete(CenturionMfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, header[CenturionMfmFormat.HeaderCylinderOffset], CenturionMfmFormat.LogicalHead, header[CenturionMfmFormat.HeaderSectorOffset], size, null, null, headerValid, dataValid)));
                offset = Math.Max(offset + CenturionMfmFormat.SectorMarkAdvanceBitCount, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, FluxStructureDescriptions.Truncated(CenturionMfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, null, CenturionMfmFormat.SectorMarkDescription)));
            if (!complete) offset += CenturionMfmFormat.SectorMarkAdvanceBitCount;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, DataMark) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, FluxStructureDescriptions.UnpairedData(CenturionMfmFormat.StructureDescriptionName, null, CenturionMfmFormat.DataBlockDescription))); offset += CenturionMfmFormat.DataMarkAdvanceBitCount; }
        return new(Id, DisplayName, FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, CenturionMfmFormat.ConfidenceSectorWeight, CenturionMfmFormat.ConfidenceDivisor), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Recherche la prochaine marque de données.</summary>
    /// <param name="stream">Flux binaire MFM à parcourir.</param><param name="start">Offset de départ inclus, exprimé en bits.</param><returns>Offset de la marque de données en bits, ou <c>-1</c> si elle est absente ou précédée d'une nouvelle marque de secteur.</returns>
    private static int FindDataMark(FluxBitstream stream, int start)
    {
        for (var offset = Math.Max(0, start); offset + CenturionMfmFormat.DataMarkBitCount <= stream.Bits.Length; offset++) { if (FluxBitReader.MatchBytes(stream, offset, SectorMark)) return -1; if (FluxBitReader.MatchBytes(stream, offset, DataMark)) return offset; }
        return -1;
    }

    /// <summary>Détermine le code représentant la taille du secteur.</summary>
    /// <param name="size">Taille du secteur en octets.</param><returns>Code correspondant à une puissance de deux à partir de 128 octets, ou zéro sans correspondance.</returns>
    private static byte SizeCode(int size)
    {
        for (byte code = 0; code < 8; code++) if ((128 << code) == size) return code;
        return 0;
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * CenturionMfmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }
}
