using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

public sealed class Victor9kGcrDecoder : IFluxDecoder
{
    public string Id => FluxCodecIds.Victor9kGcr; public string DisplayName => FluxCodecDisplayNames.Victor9kGcr;

    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveDoubledNrzi(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        const int markBits = Victor9kGcrFormat.MarkBitCount; const int headerBytes = Victor9kGcrFormat.HeaderByteCount; const int sectorBytes = Victor9kGcrFormat.SectorByteCount; const int decodedDataBytes = Victor9kGcrFormat.DecodedDataByteCount;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, Victor9kGcrFormat.HeaderMark)) continue;
            var header = TryDecodeBytes(stream.Bits, offset + Victor9kGcrFormat.EncodedDataStartBitOffset, headerBytes); bool? headerValid = null; byte cylinder = 0; byte number = 0;
            if (header is not null)
            {
                cylinder = header.Value.Bytes[1]; number = header.Value.Bytes[2]; headerValid = cylinder + number == header.Value.Bytes[3]; bytes.AddRange(header.Value.Bytes);
            }
            var dataOffset = FindMark(stream, header?.EndOffset ?? offset + markBits, Math.Min(stream.Bits.Length, offset + Victor9kGcrFormat.DataSearchEncodedByteCount * BitPrimitives.BitsPerByte * Victor9kGcrFormat.EncodedCellStride), Victor9kGcrFormat.DataMark);
            bool? dataValid = null; var structureEnd = header?.EndOffset ?? offset + markBits;
            if (dataOffset >= 0)
            {
                pairedData.Add(dataOffset); var data = TryDecodeBytes(stream.Bits, dataOffset + Victor9kGcrFormat.EncodedDataStartBitOffset, decodedDataBytes);
                if (data is not null)
                {
                    ushort checksum = 0; for (var index = 0; index < sectorBytes; index++) checksum += data.Value.Bytes[index + 1];
                    var stored = (ushort)(data.Value.Bytes[sectorBytes + 1] | data.Value.Bytes[sectorBytes + 2] << BitPrimitives.BitsPerByte); dataValid = checksum == stored; structureEnd = data.Value.EndOffset;
                    bytes.AddRange(data.Value.Bytes.Skip(1).Take(sectorBytes));
                    structures.Add(new(FluxStructureKind.FormatData, dataOffset, data.Value.EndOffset - dataOffset, $"Victor 9000 data block, {Victor9kGcrFormat.SectorByteCount} bytes, checksum {(dataValid == true ? "valid" : "invalid")}"));
                }
                else structures.Add(new(FluxStructureKind.FormatData, dataOffset, markBits, "Victor 9000 data block, checksum unavailable"));
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, 0, number, Victor9kGcrFormat.SectorSizeCode, sectorBytes, integrity, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, Math.Max(markBits, (header?.EndOffset ?? offset + markBits) - offset), $"Victor 9000 C{cylinder} H0 R{number}, header {(headerValid is null ? "unavailable" : headerValid == true ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
            offset = Math.Max(offset + markBits - 1, structureEnd - 1);
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, Victor9kGcrFormat.DataMark) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, "Unpaired Victor 9000 data block")); offset += markBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 24d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int FindMark(FluxBitstream stream, int start, int end, IReadOnlyList<byte> mark)
    {
        for (var offset = Math.Max(0, start); offset + mark.Count * BitPrimitives.BitsPerByte <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, mark)) return offset;
        return -1;
    }

    private static (byte[] Bytes, int EndOffset)? TryDecodeBytes(IReadOnlyList<bool> bits, int start, int count)
    {
        var result = new byte[count]; var offset = start;
        for (var index = 0; index < count; index++)
        {
            if (!TryDecodeNibble(bits, ref offset, out var high) || !TryDecodeNibble(bits, ref offset, out var low)) return null;
            result[index] = (byte)((high << 4) | low);
        }
        return (result, offset);
    }

    private static bool TryDecodeNibble(IReadOnlyList<bool> bits, ref int offset, out byte value)
    {
        var code = 0; value = 0;
        for (var bit = 0; bit < Victor9kGcrFormat.EncodedNibbleBitCount; bit++, offset += Victor9kGcrFormat.EncodedCellStride) { if (offset >= bits.Count) return false; code = (code << 1) | (bits[offset] ? 1 : 0); }
        if (!Victor9kGcrFormat.DecodingTable.TryGetValue(code, out var decoded)) return false; value = (byte)decoded; return true;
    }
}
