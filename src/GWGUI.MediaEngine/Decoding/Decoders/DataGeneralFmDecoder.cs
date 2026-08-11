using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding;
using GWGUI.MediaEngine.Encoding.Definitions;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

/// <summary>Décode les pistes utilisant le format Data General FM.</summary>
public sealed class DataGeneralFmDecoder : IFluxDecoder
{
    /// <summary>Conserve la définition « Sync » utilisée par ce codec.</summary>
    private static readonly byte[] Sync = DataGeneralFmFormat.Sync.ToArray();

    /// <summary>Obtient l'identifiant technique du codec.</summary>
    public string Id => FluxCodecIds.DataGeneralFm;
    /// <summary>Obtient le nom affiché du codec.</summary>
    public string DisplayName => FluxCodecDisplayNames.DataGeneralFm;

    /// <summary>Décode une révolution de flux et restitue ses structures et secteurs.</summary>
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveFm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>();
        var sectors = new List<DecodedSector>();
        var bytes = new List<byte>();
        var syncOffsets = FindAll(stream, Sync);

        for (var index = 0; index + 1 < syncOffsets.Count; index++)
        {
            var headerOffset = syncOffsets[index];
            var headerStart = headerOffset + 32;
            if (headerStart + 32 > stream.Bits.Length) continue;
            if (!FluxBitReader.TryDecodeMfmByte(stream, headerStart, out var cylinderByte)) continue;
            if (!FluxBitReader.TryDecodeMfmByte(stream, headerStart + 16, out var sectorByte)) continue;
            var cylinder = (byte)(cylinderByte & DataGeneralFmFormat.CylinderMask);
            var head = (byte)(cylinderByte >> DataGeneralFmFormat.HeadShift);
            var sectorNumber = sectorByte >> DataGeneralFmFormat.SectorShift;
            if (sectorNumber > 7) continue;

            var dataOffset = syncOffsets[index + 1];
            if (dataOffset - headerStart > 256 || dataOffset <= headerStart + 31) continue;
            var dataStart = dataOffset + 32;
            const int dataBytes = DataGeneralFmFormat.SectorSize + DataGeneralFmFormat.ChecksumByteCount;
            bool? valid = null;
            if (dataStart + dataBytes * 16 <= stream.Bits.Length)
            {
                var block = TryDecodeMfmBytes(stream, dataStart, dataBytes);
                if (block is null) continue;
                var stored = (ushort)((block[DataGeneralFmFormat.SectorSize] << BitPrimitives.BitsPerByte) | block[DataGeneralFmFormat.SectorSize + 1]);
                valid = Checksum(block.AsSpan(0, DataGeneralFmFormat.SectorSize)) == stored;
                bytes.AddRange(block.AsSpan(0, DataGeneralFmFormat.SectorSize).ToArray());
                structures.Add(new(FluxStructureKind.FormatData, dataOffset, 32 + dataBytes * 16,
                    $"Data General C{cylinder} H{head} R{sectorNumber}, 512 bytes, checksum {(valid == true ? "valid" : "invalid")}"));
            }

            sectors.Add(new(cylinder, head, sectorNumber, 2, DataGeneralFmFormat.SectorSize, valid, headerOffset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, headerOffset, 64, $"Data General C{cylinder} H{head} R{sectorNumber}"));
            index++;
        }

        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 24d), stream.BitCellTicks, structures, bytes, sectors);
    }

    /// <summary>Recherche toutes les occurrences du motif dans le flux.</summary>
    private static List<int> FindAll(FluxBitstream stream, IReadOnlyList<byte> pattern)
    {
        var offsets = new List<int>();
        for (var offset = 0; offset + pattern.Count * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, pattern)) offsets.Add(offset);
        return offsets;
    }

    /// <summary>Calcule la somme de contrôle du bloc fourni.</summary>
    private static ushort Checksum(ReadOnlySpan<byte> data)
    {
        ushort value = 0;
        for (var index = 0; index <= data.Length; index++)
        {
            var input = index < data.Length ? data[index] : (byte)0;
            value = (ushort)(((value & 0xff) ^ (value >> BitPrimitives.BitsPerByte)) | (((value & 0xff) ^ input) << BitPrimitives.BitsPerByte));
        }
        return value;
    }

    /// <summary>Tente de décoder une suite d'octets MFM.</summary>
    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
