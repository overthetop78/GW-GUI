using GWGUI.MediaEngine.Containers.Scp;
using GWGUI.MediaEngine.Encoding.Definitions;
using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

public sealed class AppleGcrDecoder : IFluxDecoder
{
    private static readonly Dictionary<byte, byte> InverseSixAndTwo = AppleIIGcrFormat.SixAndTwoTable.Select((value, index) => (value, index)).ToDictionary(x => x.value, x => (byte)x.index);
    private static readonly Dictionary<byte, byte> InverseFiveAndThree = AppleIIGcrFormat.FiveAndThreeTable.Select((value, index) => (value, index)).ToDictionary(x => x.value, x => (byte)x.index);
    public string Id => FluxCodecIds.AppleIIGcr; public string DisplayName => FluxCodecDisplayNames.AppleIIGcr;
    public FluxDecodeResult Decode(ScpRevolution revolution) => DecodeCore(FluxTransitionDecoder.DecodeNrzi(revolution.FluxIntervals));

    internal FluxDecodeResult DecodeBits(bool[] bits) => DecodeCore(new FluxBitstream(bits, 1));

    private FluxDecodeResult DecodeCore(FluxBitstream stream)
    {
        var trackBitLength = stream.Bits.Length;
        stream = stream.WithCircularTail(AppleIIGcrFormat.CircularTailBitCount);
        var structures = new List<FluxStructure>(); var bytes = new List<byte>(); var sectors = new List<DecodedSector>(); var pairedData = new HashSet<int>();
        for (var offset = 0; offset < trackBitLength && offset + AppleIIGcrFormat.PrologueBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, AppleIIGcrFormat.SixAndTwoAddressPrologue, AppleIIGcrFormat.PrologueBitCount)) continue;
            var address = TryReadBytes(stream.Bits, offset + AppleIIGcrFormat.PrologueBitCount, AppleIIGcrFormat.EncodedAddressByteCount); bool? headerValid = null; byte volume = 0; byte cylinder = 0; byte number = 0;
            if (address is not null)
            {
                volume = DecodeFourAndFour(address[0], address[1]); cylinder = DecodeFourAndFour(address[2], address[3]); number = DecodeFourAndFour(address[4], address[5]);
                var checksum = DecodeFourAndFour(address[6], address[7]); headerValid = (byte)(volume ^ cylinder ^ number) == checksum; bytes.AddRange([volume, cylinder, number, checksum]);
            }
            var headerEnd = offset + AppleIIGcrFormat.PrologueBitCount + (address is null ? 0 : AppleIIGcrFormat.EncodedAddressBitCount);
            // The third address-epilogue byte is not reliable on every protected NIB
            // image. Pair the address with the following data prologue instead: the
            // address and data checksums still provide the required integrity checks.
            var dataOffset = Find(stream, headerEnd, Math.Min(stream.Bits.Length, headerEnd + AppleIIGcrFormat.DataSearchBitCount), AppleIIGcrFormat.DataPrologue); bool? dataValid = null; var structureEnd = headerEnd;
            byte[]? sectorData = null;
            if (dataOffset >= 0)
            {
                pairedData.Add(dataOffset); var data = TryDecodeSixAndTwo(stream.Bits, dataOffset + AppleIIGcrFormat.PrologueBitCount);
                if (data is not null)
                {
                    dataValid = data.Value.Valid; structureEnd = data.Value.EndOffset; sectorData = data.Value.Data; bytes.AddRange(sectorData);
                    structures.Add(new(FluxStructureKind.AppleData, dataOffset, data.Value.EndOffset - dataOffset, $"Apple II data block, 256 bytes, checksum {(dataValid == true ? "valid" : "invalid")}"));
                }
                else structures.Add(new(FluxStructureKind.AppleData, dataOffset, AppleIIGcrFormat.PrologueBitCount, "Apple II data block, checksum unavailable"));
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, 0, number, AppleIIGcrFormat.SectorSizeCode, AppleIIGcrFormat.SectorSize, integrity, offset, SectorIntegrityKind.Checksum, sectorData));
            structures.Add(new(FluxStructureKind.AppleAddress, offset, Math.Max(AppleIIGcrFormat.PrologueBitCount, headerEnd - offset), $"Apple II V{volume} T{cylinder} S{number}, address checksum {(headerValid is null ? "unavailable" : headerValid == true ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
            offset = headerValid == true ? Math.Max(offset + AppleIIGcrFormat.PrologueBitCount - 1, structureEnd - 1) : offset + AppleIIGcrFormat.PrologueBitCount - 1;
        }
        DecodeFiveAndThree(stream, trackBitLength, structures, bytes, sectors, pairedData);
        for (var offset = 0; offset < trackBitLength && offset + AppleIIGcrFormat.PrologueBitCount <= stream.Bits.Length; offset++) if (FluxBitReader.Match(stream, offset, AppleIIGcrFormat.DataPrologue, AppleIIGcrFormat.PrologueBitCount) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.AppleData, offset, AppleIIGcrFormat.PrologueBitCount, "Unpaired Apple II data prologue D5 AA AD")); offset += AppleIIGcrFormat.PrologueBitCount - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 32d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static void DecodeFiveAndThree(FluxBitstream stream, int trackBitLength, List<FluxStructure> structures,
        List<byte> bytes, List<DecodedSector> sectors, HashSet<int> pairedData)
    {
        for (var offset = 0; offset < trackBitLength && offset + AppleIIGcrFormat.PrologueBitCount <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.Match(stream, offset, AppleIIGcrFormat.FiveAndThreeAddressPrologue, AppleIIGcrFormat.PrologueBitCount)) continue;
            var address = TryReadBytes(stream.Bits, offset + AppleIIGcrFormat.PrologueBitCount, AppleIIGcrFormat.EncodedAddressByteCount); bool? headerValid = null;
            byte volume = 0, cylinder = 0, number = 0;
            if (address is not null)
            {
                volume = DecodeFourAndFour(address[0], address[1]); cylinder = DecodeFourAndFour(address[2], address[3]);
                number = DecodeFourAndFour(address[4], address[5]); var checksum = DecodeFourAndFour(address[6], address[7]);
                headerValid = (byte)(volume ^ cylinder ^ number) == checksum;
            }
            var headerEnd = offset + AppleIIGcrFormat.AddressBlockBitCount;
            var dataOffset = Find(stream, headerEnd, Math.Min(stream.Bits.Length, headerEnd + AppleIIGcrFormat.DataSearchBitCount), AppleIIGcrFormat.DataPrologue);
            bool? dataValid = null; byte[]? sectorData = null; var structureEnd = headerEnd;
            if (dataOffset >= 0)
            {
                pairedData.Add(dataOffset);
                var decoded = TryDecodeFiveAndThree(stream.Bits, dataOffset + AppleIIGcrFormat.PrologueBitCount);
                if (decoded is not null)
                {
                    sectorData = decoded.Value.Data; dataValid = decoded.Value.Valid; structureEnd = decoded.Value.EndOffset;
                    bytes.AddRange(sectorData);
                    structures.Add(new(FluxStructureKind.AppleData, dataOffset, structureEnd - dataOffset,
                        $"Apple II 13-sector data block, checksum {(dataValid == true ? "valid" : "invalid")}"));
                }
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, 0, number, AppleIIGcrFormat.SectorSizeCode, AppleIIGcrFormat.SectorSize, integrity, offset, SectorIntegrityKind.Checksum, sectorData));
            structures.Add(new(FluxStructureKind.AppleAddress, offset, AppleIIGcrFormat.AddressBlockBitCount,
                $"Apple II 13-sector V{volume} T{cylinder} S{number}, address checksum {(headerValid == true ? "valid" : "invalid")}"));
            offset = headerValid == true ? Math.Max(offset + AppleIIGcrFormat.PrologueBitCount - 1, structureEnd - 1) : offset + AppleIIGcrFormat.PrologueBitCount - 1;
        }
    }

    private static byte DecodeFourAndFour(byte high, byte low) => (byte)(((high << 1) | 1) & low);
    private static byte[]? TryReadBytes(IReadOnlyList<bool> bits, int offset, int count)
    {
        if (offset + count * BitPrimitives.BitsPerByte > bits.Count) return null; var result = new byte[count];
        for (var index = 0; index < count; index++) for (var bit = 0; bit < BitPrimitives.BitsPerByte; bit++) if (bits[offset + index * BitPrimitives.BitsPerByte + bit]) result[index] |= (byte)(1 << (BitPrimitives.BitsPerByte - 1 - bit));
        return result;
    }
    private static int Find(FluxBitstream stream, int start, int end, uint mark)
    {
        for (var offset = Math.Max(0, start); offset + AppleIIGcrFormat.PrologueBitCount <= end; offset++) if (FluxBitReader.Match(stream, offset, mark, AppleIIGcrFormat.PrologueBitCount)) return offset;
        return -1;
    }
    private static (byte[] Data, bool Valid, int EndOffset)? TryDecodeSixAndTwo(IReadOnlyList<bool> bits, int offset)
    {
        // A real Disk II controller shifts bits until bit 7 becomes set. WOZ stores
        // the original bitstream, so sync fields can leave encoded bytes unaligned;
        // fixed 8-bit reads reject otherwise valid protected tracks.
        var cursor = offset;
        var encoded = AppleBitLatch.TryReadBytes(bits, ref cursor, AppleIIGcrFormat.SixAndTwoEncodedByteCount); if (encoded is null) return null; var values = new byte[AppleIIGcrFormat.SixAndTwoEncodedByteCount];
        for (var index = 0; index < values.Length; index++) if (!InverseSixAndTwo.TryGetValue(encoded[index], out values[index])) return null;
        var decoded = new byte[AppleIIGcrFormat.SixAndTwoDecodedByteCount]; byte previous = 0; var encodedIndex = 0;
        for (var index = AppleIIGcrFormat.SixAndTwoDecodedByteCount - 1; index >= AppleIIGcrFormat.SectorSize; index--) { decoded[index] = (byte)(values[encodedIndex++] ^ previous); previous = decoded[index]; }
        for (var index = 0; index < AppleIIGcrFormat.SectorSize; index++) { decoded[index] = (byte)(values[encodedIndex++] ^ previous); previous = decoded[index]; }
        var valid = (byte)(values[AppleIIGcrFormat.SixAndTwoEncodedByteCount - 1] ^ previous) == 0; var data = new byte[AppleIIGcrFormat.SectorSize]; byte auxiliaryOffset = 0;
        for (var index = 0; index < AppleIIGcrFormat.SectorSize; index++)
        {
            auxiliaryOffset = (byte)((auxiliaryOffset + AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount - 1) % AppleIIGcrFormat.SixAndTwoAuxiliaryByteCount); var auxiliary = decoded[AppleIIGcrFormat.SectorSize + auxiliaryOffset]; decoded[AppleIIGcrFormat.SectorSize + auxiliaryOffset] = (byte)(auxiliary >> 2);
            data[index] = (byte)((decoded[index] << 2) | ((auxiliary & 2) >> 1) | ((auxiliary & 1) << 1));
        }
        return (data, valid, cursor);
    }

    private static (byte[] Data, bool Valid, int EndOffset)? TryDecodeFiveAndThree(IReadOnlyList<bool> bits, int offset)
    {
        var cursor = offset;
        var encoded = AppleBitLatch.TryReadBytes(bits, ref cursor, AppleIIGcrFormat.FiveAndThreeEncodedByteCount); if (encoded is null) return null;
        var values = new byte[AppleIIGcrFormat.FiveAndThreeEncodedByteCount];
        for (var index = 0; index < values.Length; index++)
            if (!InverseFiveAndThree.TryGetValue(encoded[index], out values[index])) return null;
        const int threeSize = AppleIIGcrFormat.FiveAndThreeAuxiliaryByteCount; const int chunkSize = AppleIIGcrFormat.FiveAndThreeChunkByteCount;
        var threes = new byte[threeSize]; var bases = new byte[AppleIIGcrFormat.SectorSize]; byte checksum = 0; var source = 0;
        for (var index = threeSize - 1; index >= 0; index--) { checksum ^= values[source++]; threes[index] = checksum; }
        for (var index = 0; index < AppleIIGcrFormat.SectorSize; index++) { checksum ^= values[source++]; bases[index] = (byte)(checksum << 3); }
        var valid = values[source] == checksum; var data = new byte[AppleIIGcrFormat.SectorSize]; var destination = 0;
        for (var index = chunkSize - 1; index >= 0; index--)
        {
            var one = threes[index]; var two = threes[chunkSize + index]; var three = threes[chunkSize * 2 + index];
            var four = (byte)(((one & 2) << 1) | (two & 2) | ((three & 2) >> 1));
            var five = (byte)(((one & 1) << 2) | ((two & 1) << 1) | (three & 1));
            data[destination++] = (byte)(bases[index] | ((one >> 2) & 7));
            data[destination++] = (byte)(bases[chunkSize + index] | ((two >> 2) & 7));
            data[destination++] = (byte)(bases[chunkSize * 2 + index] | ((three >> 2) & 7));
            data[destination++] = (byte)(bases[chunkSize * 3 + index] | (four & 7));
            data[destination++] = (byte)(bases[chunkSize * 4 + index] | (five & 7));
        }
        data[destination] = (byte)(bases[AppleIIGcrFormat.SectorSize - 1] | (threes[threeSize - 1] & 7));
        return (data, valid, cursor);
    }

}
