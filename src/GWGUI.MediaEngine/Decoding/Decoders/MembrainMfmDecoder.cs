using GWGUI.MediaEngine.Containers.Scp;

using GWGUI.MediaEngine.Primitives;

namespace GWGUI.MediaEngine.Decoding;

public sealed class MembrainMfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorHeader = [0x44, 0x89, 0x55, 0x54];
    private static readonly byte[] SectorData = [0x44, 0x89, 0x55, 0x4a];
    public override string Id => FluxCodecIds.MembrainMfm; public override string DisplayName => FluxCodecDisplayNames.MembrainMfm;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorHeader, FluxStructureKind.FormatHeader, "Membrain sector header"), (SectorData, FluxStructureKind.FormatData, "Membrain sector data")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveMfm(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int headerBits = 6 * 16; const int sectorBytes = 512; const int dataBlockBytes = 2 + sectorBytes + 2;
        var pairedData = new HashSet<int>();
        for (var offset = 0; offset + SectorHeader.Length * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorHeader)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = TryDecodeMfmBytes(stream, offset, 6);
                if (header is null) continue;
                var headerValid = header[1] == 0xfe && Primitives.Crc16Calculator.Compute(header, polynomial: Primitives.Crc16Calculator.IbmPolynomial, initial: Primitives.Crc16Calculator.ZeroInitialValue) == 0;
                var cylinder = (byte)(((header[2] & 0x1f) << 3) | ((header[3] & 0xe0) >> 5));
                var head = (byte)((header[3] >> 4) & 1); var number = (byte)(header[3] & 0x0f);
                bytes.AddRange(header);
                var dataOffset = FindMark(stream, offset + 1, Math.Min(stream.Bits.Length, offset + 104 * BitPrimitives.BitsPerByte), SectorData);
                bool? dataValid = null; var structureEnd = offset + headerBits;
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset);
                    var dataEnd = dataOffset + dataBlockBytes * 16;
                    if (dataEnd <= stream.Bits.Length)
                    {
                        var data = TryDecodeMfmBytes(stream, dataOffset, dataBlockBytes);
                        if (data is null) continue;
                        dataValid = data[1] is >= 0xf8 and <= 0xfb && Primitives.Crc16Calculator.Compute(data, polynomial: Primitives.Crc16Calculator.IbmPolynomial, initial: Primitives.Crc16Calculator.ZeroInitialValue) == 0;
                        bytes.AddRange(data.Skip(2).Take(sectorBytes)); structureEnd = dataEnd;
                        structures.Add(new(FluxStructureKind.FormatData, dataOffset, dataEnd - dataOffset, $"Membrain data block, 512 bytes, CRC {(dataValid == true ? "valid" : "invalid")}"));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, SectorData.Length * BitPrimitives.BitsPerByte, "Membrain data block, CRC unavailable"));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(cylinder, head, number, 2, sectorBytes, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"Membrain C{cylinder} H{head} R{number}, header CRC {(headerValid ? "valid" : "invalid")}, data CRC {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
                offset = Math.Max(offset + SectorHeader.Length * BitPrimitives.BitsPerByte - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, SectorHeader.Length * BitPrimitives.BitsPerByte, "Membrain sector header"));
            if (!complete) offset += SectorHeader.Length * BitPrimitives.BitsPerByte - 1;
        }
        for (var offset = 0; offset + SectorData.Length * BitPrimitives.BitsPerByte <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, SectorData) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, SectorData.Length * BitPrimitives.BitsPerByte, "Unpaired Membrain data block")); offset += SectorData.Length * BitPrimitives.BitsPerByte - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int FindMark(FluxBitstream stream, int start, int end, IReadOnlyList<byte> mark)
    {
        for (var offset = Math.Max(0, start); offset + mark.Count * BitPrimitives.BitsPerByte <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, mark)) return offset;
        return -1;
    }

    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!FluxBitReader.TryDecodeMfmByte(stream, offset + index * 16, out result[index])) return null;
        return result;
    }
}
