using GWGUI.MediaEngine.Images.Reading.Decoding.Definitions;
using GWGUI.MediaEngine.Functions;
using GWGUI.MediaEngine.Images.Models.Sectors;
using GWGUI.MediaEngine.Images.Writing.Encoding;
using GWGUI.MediaEngine.Images.Models.Flux;

namespace GWGUI.MediaEngine.Images.Reading.Decoding;
/// <summary>Décode les pistes utilisant le format NorthStar MFM.</summary>
public sealed class NorthstarMfmDecoder : IFluxDecoder
{
    internal IReadOnlyList<DecodedSector> DecodeHardSectors(FluxRevolution revolution, bool fm,
        int cylinder, int head, int sectorCount)
    {
        var stream = fm ? FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals)
            : FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var sync = fm ? TrackBitEncoding.EncodeCompactFm(0, NorthstarMfmFormat.AddressMark)
            : TrackBitEncoding.EncodeCompactMfm(0, NorthstarMfmFormat.AddressMark);
        var size = fm ? 256 : NorthstarMfmFormat.SectorSize;
        var cells = checked((int)Math.Round(revolution.IndexTimeTicks / stream.BitCellTicks));
        var sectors = new List<DecodedSector>();
        for (var number = 0; number < sectorCount; number++)
        {
            var start = cells * number / sectorCount;
            var end = Math.Min(stream.Bits.Length, cells * (number + 1) / sectorCount);
            DecodedSector? candidate = null;
            for (var offset = start; offset + (size + (fm ? 3 : 4)) * 16 <= end; offset++)
            {
                if (!FluxBitReader.MatchBytes(stream, offset, sync)) continue;
                var dataOffset = offset + (fm ? 2 : 3) * 16;
                var data = TryDecodeMfmBytes(stream, dataOffset, size);
                if (data is null || !FluxBitReader.TryDecodeMfmByte(stream, dataOffset + size * 16, out var checksum)) continue;
                var valid = RotatingChecksumCalculator.Compute(data) == checksum;
                candidate = new(checked((byte)cylinder), checked((byte)head), number, (byte)(fm ? 1 : 2), size, valid,
                    offset, SectorIntegrityKind.Checksum, Data: data);
                if (valid) break;
            }
            if (candidate is not null) sectors.Add(candidate);
        }
        return sectors;
    }

    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => NorthstarMfmFormat.CodecId;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => NorthstarMfmFormat.CodecDisplayName;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(FluxRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var decodedBytes = new List<byte>();
        for (var offset = 0; offset + NorthstarMfmFormat.MarkBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, NorthstarMfmFormat.SectorMark))
                continue;
            var identity = TryDecodeIdentity(stream, offset + NorthstarMfmFormat.MarkBitCount);
            var block = identity is null ? null : TryDecodeBlock(stream, offset + NorthstarMfmFormat.MarkBitCount, identity);
            if (block is not null)
            {
                decodedBytes.Add(identity!.PackedValue);
                decodedBytes.AddRange(block.Data);
            }

            if (identity is not null)
                sectors.Add(new(identity.Cylinder, NorthstarMfmFormat.LogicalHead, identity.Sector, NorthstarMfmFormat.SectorSizeCode, NorthstarMfmFormat.SectorSize, block?.ChecksumValid, offset, SectorIntegrityKind.Checksum, Data: block?.Data));
            var description = block is not null 
                ? $"{FluxStructureDescriptions.Identity(NorthstarMfmFormat.StructureDescriptionName,
                    FluxStructureKind.FormatHeader,
                    identity!.Cylinder,
                    NorthstarMfmFormat.LogicalHead,
                    identity.Sector,
                    NorthstarMfmFormat.SectorSize,
                    NorthstarMfmFormat.AddressMark,
                    null)}, {FluxStructureDescriptions.Integrity("checksum",
                    block.ChecksumValid)}" 
                : identity is not null 
                ? $"{FluxStructureDescriptions.Identity(NorthstarMfmFormat.StructureDescriptionName,
                    FluxStructureKind.FormatHeader,
                    identity.Cylinder,
                    NorthstarMfmFormat.LogicalHead,
                    identity.Sector,
                    NorthstarMfmFormat.SectorSize,
                    NorthstarMfmFormat.AddressMark,
                    null)}, {FluxStructureDescriptions.Integrity("checksum",
                    null)}" 
                : FluxStructureDescriptions.Truncated(NorthstarMfmFormat.StructureDescriptionName, FluxStructureKind.FormatHeader, null, "hard-sector block");
            var length = block is not null ? NorthstarMfmFormat.FullBlockBitCount : identity is not null ? NorthstarMfmFormat.MarkBitCount + NorthstarMfmFormat.IdentityBitCount : NorthstarMfmFormat.MarkBitCount;
            structures.Add(new(FluxStructureKind.FormatHeader, offset, length, description));
            offset += NorthstarMfmFormat.ScanAdvance;
        }

        var confidence = FluxDecoderConfidence.Calculate(sectors.Count, structures.Count, NorthstarMfmFormat.ConfidenceSectorWeight, NorthstarMfmFormat.ConfidenceDivisor);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, decodedBytes, sectors);
    }

    /// <summary>Lit l'octet d'identité et en dépaquette l'adresse.</summary>
    internal static NorthstarMfmIdentity? TryDecodeIdentity(FluxBitstream stream, int offset)
    {
        if (!FluxBitReader.TryDecodeMfmByte(stream, offset, out var packed))
            return null;
        var address = NorthstarMfmAddress.Unpack(packed);
        return new(address.Cylinder, address.Sector, packed);
    }

    /// <summary>Lit la charge utile et le checksum d'un bloc complet.</summary>
    internal static NorthstarMfmBlock? TryDecodeBlock(FluxBitstream stream, int offset, NorthstarMfmIdentity identity)
    {
        var data = TryDecodeMfmBytes(stream, offset + NorthstarMfmFormat.IdentityBitCount, NorthstarMfmFormat.SectorSize);
        if (data is null || !FluxBitReader.TryDecodeMfmByte(stream, offset + NorthstarMfmFormat.IdentityBitCount + NorthstarMfmFormat.PayloadBitCount, out var storedChecksum))
            return null;
        return new(identity, data, storedChecksum, RotatingChecksumCalculator.Compute(data) == storedChecksum);
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * NorthstarMfmFormat.EncodedByteBitCount > stream.Bits.Length)
            return null;
        var result = new byte[count];
        for (var index = 0; index < count; index++)
            if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * NorthstarMfmFormat.EncodedByteBitCount, out result[index]))
                return null;
        return result;
    }
}
