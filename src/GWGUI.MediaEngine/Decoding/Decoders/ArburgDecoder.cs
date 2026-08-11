using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant les blocs de données FM et les blocs système du format Arburg.</summary>
public sealed class ArburgDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => ArburgFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => ArburgFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    /// <param name="revolution">Révolution SCP à décoder.</param>
    /// <returns>Résultat regroupant les blocs de données et système Arburg reconnus.</returns>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var dataBlocks = ScanFmData(stream);
        var systemBlocks = ScanSystemData(stream);
        var structures = dataBlocks.Structures.Concat(systemBlocks.Structures).OrderBy(item => item.BitOffset).ToArray();
        var sectors = dataBlocks.Sectors.Concat(systemBlocks.Sectors).ToArray();
        var bytes = dataBlocks.Bytes.Concat(systemBlocks.Bytes).ToArray();
        return new(Id, DisplayName, FluxDecoderConfidence.Calculate(sectors.Length, structures.Length, ArburgFormat.ConfidenceSectorWeight, ArburgFormat.ConfidenceDivisor), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Recherche et décode les blocs de données FM.</summary>
    /// <param name="stream">Flux binaire FM source.</param>
    /// <returns>Structures, secteurs et octets utiles reconnus.</returns>
    private static ArburgScanResult ScanFmData(FluxBitstream stream)
    {
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var markBits = ArburgFormat.DataMarkBitCount;
        const int blockSize = ArburgFormat.DataBlockSize;
        const int usefulSize = ArburgFormat.DataUsefulSize;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, ArburgFormat.DataMark)) continue;
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
        return new(structures, sectors, bytes);
    }

    /// <summary>Recherche et décode les blocs de données système.</summary>
    /// <param name="stream">Flux binaire source.</param>
    /// <returns>Structures, secteurs et octets utiles reconnus.</returns>
    private static ArburgScanResult ScanSystemData(FluxBitstream stream)
    {
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var markBits = ArburgFormat.SystemMarkBitCount;
        const int blockSize = ArburgFormat.SystemBlockSize;
        const int usefulSize = ArburgFormat.SystemUsefulSize;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, ArburgFormat.SystemMark)) continue;
            var decoded = TryDecodeSystemBlock(stream, offset + markBits, blockSize, usefulSize);
            if (decoded is not null) bytes.AddRange(decoded.Value.Data);
            sectors.Add(new(ArburgFormat.LogicalCylinder, ArburgFormat.LogicalHead, ArburgFormat.LogicalSector, ArburgFormat.SectorSizeCode, blockSize, decoded?.Valid, offset, SectorIntegrityKind.Checksum, decoded?.Data));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, decoded is null ? markBits : decoded.Value.EndOffset - offset, FluxStructureDescriptions.WithIntegrity(ArburgFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, ArburgFormat.LogicalCylinder, ArburgFormat.LogicalHead, ArburgFormat.LogicalSector, blockSize, null, ArburgFormat.SystemBlockDescription, ArburgFormat.ChecksumDescription, decoded?.Valid)));
            offset += ArburgFormat.SystemMarkAdvanceBitCount;
        }
        return new(structures, sectors, bytes);
    }

    /// <summary>Décode un bloc système complet et contrôle son checksum.</summary>
    /// <param name="stream">Flux binaire source.</param>
    /// <param name="start">Position du premier bit suivant la marque.</param>
    /// <param name="blockSize">Taille physique attendue.</param>
    /// <param name="usefulSize">Nombre d'octets utiles couverts par le checksum.</param>
    /// <returns>Données utiles, validité et position finale, ou <see langword="null"/> si le codage est invalide ou tronqué.</returns>
    private static (byte[] Data, bool Valid, int EndOffset)? TryDecodeSystemBlock(FluxBitstream stream, int start, int blockSize, int usefulSize)
    {
        var decoded = ArburgSystemCodec.Decode(stream, start, blockSize);
        return decoded is null ? null : (decoded.Value.Bytes.Take(usefulSize).ToArray(), ArburgChecksum.IsValid(decoded.Value.Bytes, usefulSize), decoded.Value.EndOffset);
    }

    /// <summary>Décode les octets physiques d'un bloc FM.</summary>
    /// <param name="stream">Flux binaire source.</param>
    /// <param name="offset">Position du premier bit suivant la marque.</param>
    /// <param name="count">Nombre d'octets physiques attendus.</param>
    /// <returns>Octets encodés, ou <see langword="null"/> si une cellule FM est invalide ou tronquée.</returns>
    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++)
            if (!FluxBitReader.TryDecodeFmByte32(stream, offset + index * ArburgFormat.FmEncodedByteBitCount, out result[index])) return null;
        return result;
    }

    /// <summary>Décode un bloc FM complet, rétablit l'ordre des bits et contrôle son checksum.</summary>
    /// <param name="stream">Flux binaire source.</param>
    /// <param name="offset">Position du premier bit suivant la marque.</param>
    /// <param name="blockSize">Taille physique attendue.</param>
    /// <param name="usefulSize">Nombre d'octets utiles couverts par le checksum.</param>
    /// <returns>Données utiles et validité, ou <see langword="null"/> si le codage FM est invalide ou tronqué.</returns>
    private static (byte[] Data, bool Valid)? TryDecodeFmBlock(FluxBitstream stream, int offset, int blockSize, int usefulSize)
    {
        var encoded = TryDecodeFmBytes(stream, offset, blockSize);
        if (encoded is null) return null;
        var block = encoded.Select(Primitives.BitPrimitives.ReverseBits).ToArray();
        return (block.Take(usefulSize).ToArray(), ArburgChecksum.IsValid(block, usefulSize));
    }

    /// <summary>Regroupe les éléments produits par le balayage d'une forme de bloc Arburg.</summary>
    /// <param name="Structures">Structures reconnues.</param>
    /// <param name="Sectors">Secteurs reconstruits.</param>
    /// <param name="Bytes">Octets utiles décodés.</param>
    private sealed record ArburgScanResult(IReadOnlyList<FluxStructure> Structures, IReadOnlyList<DecodedSector> Sectors, IReadOnlyList<byte> Bytes);
}
