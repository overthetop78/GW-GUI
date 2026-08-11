using GWGUI.MediaEngine.Containers.Scp;

namespace GWGUI.MediaEngine.Decoding;

public sealed class MembrainMfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorHeader = [0x44, 0x89, 0x55, 0x54];
    private static readonly byte[] SectorData = [0x44, 0x89, 0x55, 0x4a];
    public override string Id => "membrain.mfm"; public override string DisplayName => "Membrain MFM";
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorHeader, FluxStructureKind.FormatHeader, "Membrain sector header"), (SectorData, FluxStructureKind.FormatData, "Membrain sector data")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxTransitionDecoder.DecodeAdaptiveClock(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int headerBits = 6 * 16; const int sectorBytes = 512; const int dataBlockBytes = 2 + sectorBytes + 2;
        var pairedData = new HashSet<int>();
        for (var offset = 0; offset + SectorHeader.Length * 8 <= stream.Bits.Length; offset++)
        {
            if (!FluxBitReader.MatchBytes(stream, offset, SectorHeader)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = Enumerable.Range(0, 6).Select(index => FluxBitReader.DecodeMfmByte(stream, offset + index * 16)).ToArray();
                var headerValid = header[1] == 0xfe && Crc16(header) == 0;
                var cylinder = (byte)(((header[2] & 0x1f) << 3) | ((header[3] & 0xe0) >> 5));
                var head = (byte)((header[3] >> 4) & 1); var number = (byte)(header[3] & 0x0f);
                bytes.AddRange(header);
                var dataOffset = FindMark(stream, offset + 1, Math.Min(stream.Bits.Length, offset + 104 * 8), SectorData);
                bool? dataValid = null; var structureEnd = offset + headerBits;
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset);
                    var dataEnd = dataOffset + dataBlockBytes * 16;
                    if (dataEnd <= stream.Bits.Length)
                    {
                        var data = Enumerable.Range(0, dataBlockBytes).Select(index => FluxBitReader.DecodeMfmByte(stream, dataOffset + index * 16)).ToArray();
                        dataValid = data[1] is >= 0xf8 and <= 0xfb && Crc16(data) == 0;
                        bytes.AddRange(data.Skip(2).Take(sectorBytes)); structureEnd = dataEnd;
                        structures.Add(new(FluxStructureKind.FormatData, dataOffset, dataEnd - dataOffset, $"Membrain data block, 512 bytes, CRC {(dataValid == true ? "valid" : "invalid")}"));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, SectorData.Length * 8, "Membrain data block, CRC unavailable"));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(cylinder, head, number, 2, sectorBytes, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"Membrain C{cylinder} H{head} R{number}, header CRC {(headerValid ? "valid" : "invalid")}, data CRC {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
                offset = Math.Max(offset + SectorHeader.Length * 8 - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, SectorHeader.Length * 8, "Membrain sector header"));
            if (!complete) offset += SectorHeader.Length * 8 - 1;
        }
        for (var offset = 0; offset + SectorData.Length * 8 <= stream.Bits.Length; offset++) if (FluxBitReader.MatchBytes(stream, offset, SectorData) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, SectorData.Length * 8, "Unpaired Membrain data block")); offset += SectorData.Length * 8 - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int FindMark(FluxBitstream stream, int start, int end, IReadOnlyList<byte> mark)
    {
        for (var offset = Math.Max(0, start); offset + mark.Count * 8 <= end; offset++) if (FluxBitReader.MatchBytes(stream, offset, mark)) return offset;
        return -1;
    }

    private static ushort Crc16(IEnumerable<byte> values) => Primitives.Crc16Calculator.Compute(values, polynomial: 0x8005, initial: 0);
}
