using GWGUI.MediaEngine.Decoding.Definitions;
using GWGUI.MediaEngine.Flux;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Micral N FM.</summary>
public sealed class MicralNFmDecoder : IFluxDecoder
{
    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => MicralNFmFormat.CodecId;

    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => MicralNFmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var decodedBytes = new List<byte>();
        for (var offset = 0; offset + MicralNFmFormat.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, MicralNFmFormat.SectorMark)) continue;
            var block = TryDecodeBlock(stream, offset);
            if (block is null)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, MicralNFmFormat.MarkBitCount, FluxStructureDescriptions.Truncated(MicralNFmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, null, "hard-sector block, checksum unavailable")));
                offset += MicralNFmFormat.ScanAdvance;
                continue;
            }
            decodedBytes.AddRange(block.Data);
            sectors.Add(new(block.Cylinder, MicralNFmFormat.LogicalHead, block.Sector, MicralNFmFormat.SectorSizeCode, MicralNFmFormat.SectorSize, block.ChecksumValid, offset, SectorIntegrityKind.Checksum, Data: block.Data));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, MicralNFmFormat.BlockBitCount, $"{FluxStructureDescriptions.Identity(MicralNFmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, block.Cylinder, MicralNFmFormat.LogicalHead, block.Sector, MicralNFmFormat.SectorSize, MicralNFmFormat.AddressMark, null)}, {FluxStructureDescriptions.Integrity("checksum", block.ChecksumValid)}"));
            offset += MicralNFmFormat.ScanAdvance;
        }
        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, MicralNFmFormat.ConfidenceSectorWeight, MicralNFmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, decodedBytes, sectors);
    }

    /// <summary>Lit le numéro de secteur, le cylindre, la charge utile et le checksum d'un bloc.</summary>
    internal static MicralNFmBlock? TryDecodeBlock(FluxBitstream stream, int offset)
    {
        if (offset + MicralNFmFormat.BlockBitCount > stream.Bits.Length) return null;
        var bytes = TryDecodeFmBytes(stream, offset + MicralNFmFormat.MarkBitCount, MicralNFmFormat.BytesAfterMark);
        if (bytes is null) return null;
        var data = bytes.AsSpan(MicralNFmFormat.DataOffset, MicralNFmFormat.SectorSize).ToArray();
        var storedChecksum = bytes[MicralNFmFormat.ChecksumOffset];
        return new(bytes[MicralNFmFormat.SectorOffset], bytes[MicralNFmFormat.CylinderOffset], data, storedChecksum, MicralNChecksum.Compute(data) == storedChecksum);
    }

    /// <summary>Tente de décoder une suite d'octets FM.</summary>
    private static byte[]? TryDecodeFmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * MicralNFmFormat.EncodedByteBitCount, out result[index])) return null;
        return result;
    }
}
