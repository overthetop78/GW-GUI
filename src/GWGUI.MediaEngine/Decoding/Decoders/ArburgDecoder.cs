using GWGUI.MediaEngine.Flux;
using GWGUI.MediaEngine.Decoding.Definitions;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Arburg.</summary>
public sealed class ArburgDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Data Mark » utilisée par ce codec.</summary>
    private static readonly byte[] DataMark = ArburgFormat.DataMark.ToArray();
    /// <summary>Conserve la définition « System Mark » utilisée par ce codec.</summary>
    private static readonly byte[] SystemMark = ArburgFormat.SystemMark.ToArray();
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => ArburgFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => ArburgFormat.CodecDisplayName;
    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param><returns>Résultat regroupant les blocs de données et système Arburg reconnus.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        ScanFmData(stream, structures, sectors, bytes);
        ScanSystemData(stream, structures, sectors, bytes);
        return new(Id, DisplayName, FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, ArburgFormat.ConfidenceSectorWeight, ArburgFormat.ConfidenceDivisor), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    /// <summary>Analyse les blocs de données FM.</summary>
    /// <param name="stream">Flux binaire FM source.</param><param name="structures">Structures auxquelles ajouter les blocs reconnus.</param><param name="sectors">Secteurs auxquels ajouter les blocs reconstruits.</param><param name="bytes">Octets auxquels ajouter les données décodées.</param>
    private static void ScanFmData(FluxBitstream stream, List<FluxStructure> structures, List<DecodedSector> sectors, List<byte> bytes)
    {
        var markBits = ArburgFormat.DataMarkBitCount; const int blockSize = ArburgFormat.DataBlockSize, usefulSize = ArburgFormat.DataUsefulSize;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, DataMark)) continue;
            var complete = offset + markBits + blockSize * ArburgFormat.FmEncodedByteBitCount <= stream.Bits.Length;
            (byte[] Data, bool Valid)? decoded = null;
            if (complete)
            {
                decoded = TryDecodeFmBlock(stream, offset + markBits, blockSize, usefulSize);
                if (decoded is null) continue;
                bytes.AddRange(decoded.Value.Data);
            }
            sectors.Add(new(ArburgFormat.LogicalCylinder, ArburgFormat.LogicalHead, ArburgFormat.LogicalSector, ArburgFormat.SectorSizeCode, blockSize, decoded?.Valid, offset, SectorIntegrityKind.Checksum, decoded?.Data));
            structures.Add(new(FluxStructureKind.FormatData, offset, complete ? markBits + blockSize * ArburgFormat.FmEncodedByteBitCount : markBits, FluxStructureDescriptions.WithIntegrity(ArburgFormat.StructureDescriptionName, FluxStructureKind.FormatData, ArburgFormat.LogicalCylinder, ArburgFormat.LogicalHead, ArburgFormat.LogicalSector, blockSize, null, ArburgFormat.DataBlockDescription, ArburgFormat.ChecksumDescription, decoded?.Valid)));
            offset += ArburgFormat.DataMarkAdvanceBitCount;
        }
    }

    /// <summary>Analyse les blocs de données système.</summary>
    /// <param name="stream">Flux binaire source.</param><param name="structures">Structures auxquelles ajouter les blocs reconnus.</param><param name="sectors">Secteurs auxquels ajouter les blocs reconstruits.</param><param name="bytes">Octets auxquels ajouter les données décodées.</param>
    private static void ScanSystemData(FluxBitstream stream, List<FluxStructure> structures, List<DecodedSector> sectors, List<byte> bytes)
    {
        var markBits = ArburgFormat.SystemMarkBitCount; const int blockSize = ArburgFormat.SystemBlockSize, usefulSize = ArburgFormat.SystemUsefulSize;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SystemMark)) continue;
            var decoded = TryDecodeSystemBlock(stream, offset + markBits, blockSize, usefulSize);
            if (decoded is not null)
            {
                bytes.AddRange(decoded.Value.Data);
            }
            sectors.Add(new(ArburgFormat.LogicalCylinder, ArburgFormat.LogicalHead, ArburgFormat.LogicalSector, ArburgFormat.SectorSizeCode, blockSize, decoded?.Valid, offset, SectorIntegrityKind.Checksum, decoded?.Data));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, decoded is null ? markBits : decoded.Value.EndOffset - offset, FluxStructureDescriptions.WithIntegrity(ArburgFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, ArburgFormat.LogicalCylinder, ArburgFormat.LogicalHead, ArburgFormat.LogicalSector, blockSize, null, ArburgFormat.SystemBlockDescription, ArburgFormat.ChecksumDescription, decoded?.Valid)));
            offset += ArburgFormat.SystemMarkAdvanceBitCount;
        }
    }

    /// <summary>Tente de décoder les octets d'un bloc système.</summary>
    private static (byte[] Data, bool Valid, int EndOffset)? TryDecodeSystemBlock(FluxBitstream stream, int start, int blockSize, int usefulSize)
    {
        var decoded = ArburgSystemCodec.Decode(stream, start, blockSize);
        return decoded is null ? null : (decoded.Value.Bytes.Take(usefulSize).ToArray(), ArburgChecksum.IsValid(decoded.Value.Bytes, usefulSize), decoded.Value.EndOffset);
    }

    /// <summary>Tente de décoder une suite d'octets FM.</summary>
    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeFmByte32(stream, offset + index * 32, out result[index])) return null;
        return result;
    }

    private static (byte[] Data, bool Valid)? TryDecodeFmBlock(FluxBitstream stream, int offset, int blockSize, int usefulSize)
    {
        var encoded = TryDecodeFmBytes(stream, offset, blockSize);
        if (encoded is null) return null;
        var block = encoded.Select(Primitives.BitPrimitives.ReverseBits).ToArray();
        return (block.Take(usefulSize).ToArray(), ArburgChecksum.IsValid(block, usefulSize));
    }
}
