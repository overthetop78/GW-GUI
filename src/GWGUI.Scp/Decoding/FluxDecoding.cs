namespace GWGUI.Scp.Decoding;

public enum FluxStructureKind { Sync, IdAddressMark, DataAddressMark, DeletedDataAddressMark, AmigaSync, AppleAddress, AppleData, CommodoreSync, CommodoreHeader, FormatHeader, FormatData, TimingAnomaly }
public enum SectorIntegrityKind { Crc, Checksum }
public sealed record FluxStructure(FluxStructureKind Kind, int BitOffset, int BitLength, string Description);
public sealed record DecodedSector(byte Cylinder, byte Head, int Number, byte SizeCode, int SizeBytes, bool? IntegrityValid, int BitOffset, SectorIntegrityKind IntegrityKind = SectorIntegrityKind.Crc);
public sealed record FluxDecodeResult(string DecoderId, string DisplayName, double Confidence, double EstimatedBitCellTicks, IReadOnlyList<FluxStructure> Structures, IReadOnlyList<byte> DecodedBytes, IReadOnlyList<DecodedSector>? Sectors = null);

public interface IFluxDecoder
{
    string Id { get; }
    string DisplayName { get; }
    FluxDecodeResult Decode(ScpRevolution revolution);
}

public sealed class FluxDecoderRegistry
{
    public IReadOnlyList<IFluxDecoder> Decoders { get; } = [new IsoMfmDecoder(), new IsoFmDecoder(), new AmigaMfmDecoder(), new AppleGcrDecoder(), new AppleMacGcrDecoder(), new CommodoreGcrDecoder(), new MembrainMfmDecoder(), new Aed6200pMfmDecoder(), new QdMo5MfmDecoder(), new CenturionMfmDecoder(), new NorthstarMfmDecoder(), new HeathkitFmDecoder(), new MicralNFmDecoder(), new EmuFmDecoder(), new TycomFmDecoder(), new DecRx02Decoder(), new ArburgDecoder(), new Victor9kGcrDecoder(), new RawFluxDecoder()];
    public FluxDecodeResult DecodeAutomatic(ScpRevolution revolution) => Decoders.Select(x => x.Decode(revolution)).OrderByDescending(x => x.Confidence).First();
    public FluxDecodeResult Decode(string id, ScpRevolution revolution) => Decoders.First(x => x.Id == id).Decode(revolution);
    public (int RevolutionIndex, FluxDecodeResult Result)? DecodeBest(IReadOnlyList<ScpRevolution> revolutions, string? decoderId = null)
    {
        if (revolutions.Count == 0) return null;
        return revolutions.Select((revolution, index) => (RevolutionIndex: index, Result: decoderId is null ? DecodeAutomatic(revolution) : Decode(decoderId, revolution)))
            .OrderByDescending(candidate => candidate.Result.Confidence)
            .ThenByDescending(candidate => candidate.Result.Structures.Count)
            .First();
    }
}

public abstract class SignatureMfmDecoder : IFluxDecoder
{
    public abstract string Id { get; }
    public abstract string DisplayName { get; }
    protected abstract IReadOnlyList<(byte[] Pattern, FluxStructureKind Kind, string Description)> Signatures { get; }
    protected virtual double ExpectedStructures => 10;
    protected virtual bool IsFm => false;
    protected virtual bool IsNrzi => false;

    public virtual FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = IsNrzi ? FluxBitstream.FromNrziIntervals(revolution.FluxIntervals) : FluxBitstream.FromIntervals(revolution.FluxIntervals, IsFm); var structures = new List<FluxStructure>();
        for (var offset = 0; offset < stream.Bits.Length; offset++)
        {
            foreach (var signature in Signatures)
            {
                if (!stream.MatchBytes(offset, signature.Pattern)) continue;
                structures.Add(new(signature.Kind, offset, signature.Pattern.Length * 8, signature.Description));
                offset += signature.Pattern.Length * 8 - 1; break;
            }
        }
        return new(Id, DisplayName, Math.Min(1, structures.Count / ExpectedStructures), stream.BitCellTicks, structures, []);
    }

    protected static byte[] EncodeMfm(params byte[] data)
    {
        var bits = new List<bool>(data.Length * 16); var previousData = false;
        foreach (var value in data) for (var bit = 7; bit >= 0; bit--) { var current = (value & (1 << bit)) != 0; bits.Add(!previousData && !current); bits.Add(current); previousData = current; }
        return Pack(bits);
    }

    protected static byte[] EncodeFm(params byte[] data)
    {
        var bits = new List<bool>(data.Length * 16);
        foreach (var value in data) for (var bit = 7; bit >= 0; bit--) { bits.Add(true); bits.Add((value & (1 << bit)) != 0); }
        return Pack(bits);
    }

    private static byte[] Pack(IReadOnlyList<bool> bits)
    {
        var bytes = new byte[(bits.Count + 7) / 8]; for (var index = 0; index < bits.Count; index++) if (bits[index]) bytes[index / 8] |= (byte)(1 << (7 - index % 8)); return bytes;
    }
}

public sealed class MembrainMfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorHeader = [0x44, 0x89, 0x55, 0x54];
    private static readonly byte[] SectorData = [0x44, 0x89, 0x55, 0x4a];
    public override string Id => "membrain.mfm"; public override string DisplayName => "Membrain MFM";
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorHeader, FluxStructureKind.FormatHeader, "Membrain sector header"), (SectorData, FluxStructureKind.FormatData, "Membrain sector data")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int headerBits = 6 * 16; const int sectorBytes = 512; const int dataBlockBytes = 2 + sectorBytes + 2;
        var pairedData = new HashSet<int>();
        for (var offset = 0; offset + SectorHeader.Length * 8 <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, SectorHeader)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = Enumerable.Range(0, 6).Select(index => stream.DecodeMfmByte(offset + index * 16)).ToArray();
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
                        var data = Enumerable.Range(0, dataBlockBytes).Select(index => stream.DecodeMfmByte(dataOffset + index * 16)).ToArray();
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
        for (var offset = 0; offset + SectorData.Length * 8 <= stream.Bits.Length; offset++) if (stream.MatchBytes(offset, SectorData) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, SectorData.Length * 8, "Unpaired Membrain data block")); offset += SectorData.Length * 8 - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int FindMark(FluxBitstream stream, int start, int end, IReadOnlyList<byte> mark)
    {
        for (var offset = Math.Max(0, start); offset + mark.Count * 8 <= end; offset++) if (stream.MatchBytes(offset, mark)) return offset;
        return -1;
    }

    private static ushort Crc16(IEnumerable<byte> values)
    {
        ushort crc = 0;
        foreach (var value in values)
        {
            crc ^= (ushort)(value << 8);
            for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x8005 : crc << 1);
        }
        return crc;
    }
}

public sealed class Aed6200pMfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorHeader = [0x50, 0x94];
    private static readonly byte[][] SectorData = [[0x50, 0x8a], [0x50, 0x89], [0x50, 0x84], [0x50, 0x85]];
    public override string Id => "aed6200p.mfm"; public override string DisplayName => "AED 6200P MFM";
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorHeader, FluxStructureKind.FormatHeader, "AED 6200P C6 header mark"), .. SectorData.Select(mark => (mark, FluxStructureKind.FormatData, "AED 6200P data mark"))];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int headerBits = 7 * 16; var pairedData = new HashSet<int>();
        for (var offset = 0; offset + SectorHeader.Length * 8 <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, SectorHeader)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = Enumerable.Range(0, 7).Select(index => stream.DecodeMfmByte(offset + index * 16)).ToArray();
                var size = (header[4] << 8) | header[2]; var headerValid = header[0] == 0xc6 && Crc16(header) == 0; bytes.AddRange(header);
                var dataOffset = FindDataMark(stream, offset + 1, Math.Min(stream.Bits.Length, offset + 104 * 8));
                bool? dataValid = null; var structureEnd = offset + headerBits;
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset); var dataBlockBytes = 1 + size + 2; var dataEnd = (long)dataOffset + dataBlockBytes * 16L;
                    if (size > 0 && dataEnd <= stream.Bits.Length)
                    {
                        var data = Enumerable.Range(0, dataBlockBytes).Select(index => stream.DecodeMfmByte(dataOffset + index * 16)).ToArray();
                        dataValid = data[0] is >= 0xc0 and <= 0xc3 && Crc16(data) == 0; bytes.AddRange(data.Skip(1).Take(size)); structureEnd = (int)dataEnd;
                        structures.Add(new(FluxStructureKind.FormatData, dataOffset, (int)dataEnd - dataOffset, $"AED 6200P data {data[0]:X2}, {size} bytes, CRC {(dataValid == true ? "valid" : "invalid")}"));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, 16, "AED 6200P data block, CRC unavailable"));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(header[1], 0, header[3], SizeCode(size), size, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"AED 6200P C{header[1]} R{header[3]}, {size} bytes, header CRC {(headerValid ? "valid" : "invalid")}, data CRC {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
                offset = Math.Max(offset + SectorHeader.Length * 8 - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, SectorHeader.Length * 8, "AED 6200P C6 header mark"));
            if (!complete) offset += SectorHeader.Length * 8 - 1;
        }
        for (var offset = 0; offset + 16 <= stream.Bits.Length; offset++) if (SectorData.Any(mark => stream.MatchBytes(offset, mark)) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, 16, "Unpaired AED 6200P data block")); offset += 15; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int FindDataMark(FluxBitstream stream, int start, int end)
    {
        for (var offset = Math.Max(0, start); offset + 16 <= end; offset++) if (SectorData.Any(mark => stream.MatchBytes(offset, mark))) return offset;
        return -1;
    }

    private static byte SizeCode(int size)
    {
        for (byte code = 0; code < 8; code++) if ((128 << code) == size) return code;
        return 0;
    }

    private static ushort Crc16(IEnumerable<byte> values)
    {
        ushort crc = 0xffff;
        foreach (var value in values) { crc ^= (ushort)(value << 8); for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1); }
        return crc;
    }
}

public sealed class QdMo5MfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] HeaderMark = [0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0x44,0x91];
    private static readonly byte[] DataMark = [0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0xa9,0x14,0x91,0x44];
    public override string Id => "qdmo5.mfm"; public override string DisplayName => "QD MO5 MFM";
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(HeaderMark, FluxStructureKind.FormatHeader, "QD MO5 sector header"), (DataMark, FluxStructureKind.FormatData, "QD MO5 sector data")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedDataMarks = new HashSet<int>();
        const int markBits = 12 * 8;
        const int headerBits = 10 * 8 + 16 * 16;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, HeaderMark)) continue;
            if (offset + headerBits > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "QD MO5 sector header"));
                offset += markBits - 1; continue;
            }

            var high = stream.DecodeMfmByte(offset + markBits); var low = stream.DecodeMfmByte(offset + markBits + 16);
            var number = (high << 8) | low; bytes.Add(high); bytes.Add(low);
            var dataOffset = FindNextData(stream, offset + headerBits, (88 + 16) * 8);
            var completeData = dataOffset >= 0 && dataOffset + 10 * 8 + 130 * 16 <= stream.Bits.Length;
            bool? checksumValid = null;
            if (completeData)
            {
                byte checksum = 0; var data = new byte[128];
                for (var index = 0; index < 129; index++) { var value = stream.DecodeMfmByte(dataOffset + 10 * 8 + index * 16); checksum += value; if (index > 0) data[index - 1] = value; }
                var stored = stream.DecodeMfmByte(dataOffset + 10 * 8 + 129 * 16); checksumValid = checksum == stored;
                pairedDataMarks.Add(dataOffset); bytes.AddRange(data);
                structures.Add(new(FluxStructureKind.FormatData, dataOffset, 10 * 8 + 130 * 16, $"QD MO5 R{number} data, checksum {(checksumValid == true ? "valid" : "invalid")}"));
            }
            sectors.Add(new(0, 0, number, 0, 128, checksumValid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"QD MO5 R{number}, 128 bytes{(completeData ? $", data checksum {(checksumValid == true ? "valid" : "invalid")}" : ", data checksum unavailable")}"));
            offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!pairedDataMarks.Contains(offset) && stream.MatchBytes(offset, DataMark)) structures.Add(new(FluxStructureKind.FormatData, offset, markBits, "QD MO5 sector data"));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static int FindNextData(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - DataMark.Length * 8, start + maximumDistance);
        for (var offset = start; offset <= end; offset++)
        {
            if (stream.MatchBytes(offset, DataMark)) return offset;
            if (stream.MatchBytes(offset, HeaderMark)) return -1;
        }
        return -1;
    }
}

public sealed class CenturionMfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = [0x91, 0x22, 0x44, 0x89];
    private static readonly byte[] DataMark = [0xaa, 0xaa, 0xaa, 0xa9];
    public override string Id => "centurion.mfm"; public override string DisplayName => "Centurion MFM";
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "Centurion sector mark"), (DataMark, FluxStructureKind.FormatData, "Centurion data mark")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        const int markBits = 4 * 8;
        const int headerBits = markBits + 4 * 16;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, SectorMark)) continue;
            var complete = offset + headerBits <= stream.Bits.Length;
            if (complete)
            {
                var header = Enumerable.Range(0, 4).Select(index => stream.DecodeMfmByte(offset + markBits + index * 16)).ToArray();
                var headerValid = Crc16(header) == 0; bytes.AddRange(header);
                var dataOffset = FindDataMark(stream, offset + headerBits + 400); bool? dataValid = null; var size = 0; var structureEnd = offset + headerBits;
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset); var prefixEnd = dataOffset + markBits + 3 * 16;
                    if (prefixEnd <= stream.Bits.Length)
                    {
                        var key = stream.DecodeMfmByte(dataOffset + markBits); size = (stream.DecodeMfmByte(dataOffset + markBits + 16) << 8) | stream.DecodeMfmByte(dataOffset + markBits + 32);
                        var dataEnd = (long)prefixEnd + (size + 2L) * 16;
                        if (key == 0 && size > 0 && dataEnd <= stream.Bits.Length)
                        {
                            var block = Enumerable.Range(0, size + 4).Select(index => stream.DecodeMfmByte(dataOffset + markBits + 16 + index * 16)).ToArray();
                            dataValid = Crc16(block) == 0; bytes.AddRange(block.Skip(2).Take(size)); structureEnd = (int)dataEnd;
                            structures.Add(new(FluxStructureKind.FormatData, dataOffset, (int)dataEnd - dataOffset, $"Centurion data, {size} bytes, CRC {(dataValid == true ? "valid" : "invalid")}"));
                        }
                        else structures.Add(new(FluxStructureKind.FormatData, dataOffset, markBits + 3 * 16, key == 0 ? "Centurion data, CRC unavailable" : $"Centurion data with unsupported key {key}"));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, markBits, "Centurion data, CRC unavailable"));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(header[0], 0, header[1], SizeCode(size), size, integrity, offset));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"Centurion C{header[0]} R{header[1]}, {size} bytes, header CRC {(headerValid ? "valid" : "invalid")}, data CRC {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
                offset = Math.Max(offset + markBits - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "Centurion sector mark"));
            if (!complete) offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++) if (stream.MatchBytes(offset, DataMark) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, "Unpaired Centurion data block")); offset += markBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int FindDataMark(FluxBitstream stream, int start)
    {
        for (var offset = Math.Max(0, start); offset + DataMark.Length * 8 <= stream.Bits.Length; offset++) { if (stream.MatchBytes(offset, SectorMark)) return -1; if (stream.MatchBytes(offset, DataMark)) return offset; }
        return -1;
    }

    private static byte SizeCode(int size)
    {
        for (byte code = 0; code < 8; code++) if ((128 << code) == size) return code;
        return 0;
    }

    private static ushort Crc16(IEnumerable<byte> values)
    {
        ushort crc = 0;
        foreach (var value in values) { crc ^= (ushort)(value << 8); for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1); }
        return crc;
    }
}

public sealed class NorthstarMfmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = EncodeMfm(0, 0, 0, 0, 0, 0, 0, 0xfb);
    public override string Id => "northstar.mfm"; public override string DisplayName => "NorthStar hard-sectored MFM";
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "NorthStar hard-sector block")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int signatureBits = 8 * 16;
        const int payloadBits = 512 * 16;
        for (var offset = 0; offset + signatureBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, SectorMark)) continue;
            var hasIdentity = offset + signatureBits + 16 <= stream.Bits.Length; var fullBlock = offset + signatureBits + 16 + payloadBits + 16 <= stream.Bits.Length;
            var info = hasIdentity ? stream.DecodeMfmByte(offset + signatureBits) : (byte)0;
            var cylinder = (byte)(info >> 4); var sectorNumber = (byte)(info & 0x0f); bool? checksumValid = null;
            if (fullBlock)
            {
                byte checksum = 0; var data = new byte[512];
                for (var index = 0; index < 512; index++)
                {
                    var value = stream.DecodeMfmByte(offset + signatureBits + 16 + index * 16); data[index] = value;
                    checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1));
                }
                var stored = stream.DecodeMfmByte(offset + signatureBits + 16 + payloadBits);
                checksumValid = stored == checksum; bytes.Add(info); bytes.AddRange(data);
            }
            if (hasIdentity) sectors.Add(new(cylinder, 0, sectorNumber, 2, 512, checksumValid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, fullBlock ? signatureBits + 16 + payloadBits + 16 : signatureBits,
                fullBlock ? $"NorthStar C{cylinder} R{sectorNumber}, 512 bytes, checksum {(checksumValid == true ? "valid" : "invalid")}" : hasIdentity ? $"NorthStar C{cylinder} R{sectorNumber}, checksum unavailable" : "NorthStar hard-sector block"));
            offset += signatureBits - 1;
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }
}

public sealed class HeathkitFmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = EncodeFm(0, 0, 0, 0xbf);
    public override string Id => "heathkit.fm"; public override string DisplayName => "Heathkit hard-sectored FM";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "Heathkit hard-sector header")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals, fm: true);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        const int signatureBits = 4 * 16;
        const int headerTailBits = 4 * 16;
        for (var offset = 0; offset + signatureBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, SectorMark)) continue;
            var complete = offset + signatureBits + headerTailBits <= stream.Bits.Length;
            if (complete)
            {
                var volume = ReverseBits(stream.DecodeMfmByte(offset + signatureBits));
                var cylinder = ReverseBits(stream.DecodeMfmByte(offset + signatureBits + 16));
                var sectorNumber = ReverseBits(stream.DecodeMfmByte(offset + signatureBits + 32));
                var stored = ReverseBits(stream.DecodeMfmByte(offset + signatureBits + 48));
                byte checksum = 0;
                foreach (var value in new[] { volume, cylinder, sectorNumber }) { checksum ^= value; checksum = (byte)((checksum >> 7) | (checksum << 1)); }
                var headerValid = stored == checksum; var dataOffset = FindNextMark(stream, offset + signatureBits + headerTailBits, (88 + 16) * 8); bool? dataValid = null; var structureEnd = offset + signatureBits + headerTailBits;
                bytes.AddRange([volume, cylinder, sectorNumber]);
                if (dataOffset >= 0)
                {
                    pairedData.Add(dataOffset); var dataEnd = dataOffset + signatureBits + 257 * 16;
                    if (dataEnd <= stream.Bits.Length)
                    {
                        var data = Enumerable.Range(0, 256).Select(index => ReverseBits(stream.DecodeMfmByte(dataOffset + signatureBits + index * 16))).ToArray();
                        var dataStored = ReverseBits(stream.DecodeMfmByte(dataOffset + signatureBits + 256 * 16)); byte dataChecksum = 0;
                        foreach (var value in data) { dataChecksum ^= value; dataChecksum = (byte)((dataChecksum >> 7) | (dataChecksum << 1)); }
                        dataValid = dataStored == dataChecksum; bytes.AddRange(data); structureEnd = dataEnd;
                        structures.Add(new(FluxStructureKind.FormatData, dataOffset, dataEnd - dataOffset, $"Heathkit data, 256 bytes, checksum {(dataValid == true ? "valid" : "invalid")}"));
                    }
                    else structures.Add(new(FluxStructureKind.FormatData, dataOffset, signatureBits, "Heathkit data, checksum unavailable"));
                }
                bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
                sectors.Add(new(cylinder, 0, sectorNumber, 1, 256, integrity, offset, SectorIntegrityKind.Checksum));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, signatureBits + headerTailBits, $"Heathkit volume {volume}, C{cylinder} R{sectorNumber}, header checksum {(headerValid ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
                offset = Math.Max(offset + signatureBits - 1, structureEnd - 1);
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, signatureBits, "Heathkit hard-sector header"));
            if (!complete) offset += signatureBits - 1;
        }
        for (var offset = 0; offset + signatureBits <= stream.Bits.Length; offset++) if (stream.MatchBytes(offset, SectorMark) && !pairedData.Contains(offset) && structures.All(item => item.BitOffset != offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, signatureBits, "Unpaired Heathkit data block")); offset += signatureBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int FindNextMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - SectorMark.Length * 8, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) if (stream.MatchBytes(offset, SectorMark)) return offset;
        return -1;
    }

    private static byte ReverseBits(byte value)
    {
        var reversed = 0;
        for (var bit = 0; bit < 8; bit++) reversed = (reversed << 1) | ((value >> bit) & 1);
        return (byte)reversed;
    }
}

public sealed class MicralNFmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = EncodeFm(0, 0, 0, 0xff);
    public override string Id => "micraln.fm"; public override string DisplayName => "Micral N hard-sectored FM";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "Micral N hard-sector block")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals, fm: true);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int markBits = 4 * 16; const int syncOffset = 3 * 16; const int blockBytes = 1 + 2 + 128 + 1;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, SectorMark)) continue;
            var blockStart = offset + syncOffset;
            var complete = blockStart + blockBytes * 16 <= stream.Bits.Length;
            if (complete)
            {
                var number = stream.DecodeMfmByte(blockStart + 16);
                var cylinder = stream.DecodeMfmByte(blockStart + 32);
                var data = Enumerable.Range(0, 128).Select(index => stream.DecodeMfmByte(blockStart + (3 + index) * 16)).ToArray();
                var storedChecksum = stream.DecodeMfmByte(blockStart + 131 * 16);
                byte checksum = 0;
                foreach (var value in data) checksum = UpdateChecksum(checksum, value);
                var valid = checksum == storedChecksum;
                bytes.AddRange(data);
                sectors.Add(new(cylinder, 0, number, 0, 128, valid, offset, SectorIntegrityKind.Checksum));
                structures.Add(new(FluxStructureKind.FormatHeader, offset, syncOffset + blockBytes * 16,
                    $"Micral N C{cylinder} R{number}, 128 bytes, checksum {(valid ? "valid" : "invalid")}"));
            }
            else structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "Micral N hard-sector block, checksum unavailable"));
            offset += markBits - 1;
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte UpdateChecksum(byte checksum, byte data)
    {
        var carrySource = ((data ^ checksum) ^ 0xff) & ((data + checksum) ^ data);
        var carry = (carrySource & 0x80) != 0 ? 1 : 0;
        return (byte)(checksum + data + carry);
    }
}

public sealed class EmuFmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] SectorMark = [0x45,0x45,0x55,0x55,0x45,0x54,0x54,0x45];
    public override string Id => "emu.fm"; public override string DisplayName => "E-mu Emulator FM";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(SectorMark, FluxStructureKind.FormatHeader, "E-mu Emulator header/data mark")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var classifiedMarks = new HashSet<int>();
        const int markBits = 8 * 8;
        const int headerBits = 5 * 32;
        const int sectorSize = 0xe00;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, SectorMark) || offset + headerBits > stream.Bits.Length) continue;
            var rawTrack = stream.DecodeFmByte32(offset + markBits);
            var crcHigh = stream.DecodeFmByte32(offset + markBits + 32); var crcLow = stream.DecodeFmByte32(offset + markBits + 64);
            if (Crc16([rawTrack, crcHigh, crcLow]) != 0) continue;

            var track = ReverseBits(rawTrack); var cylinder = (byte)(track >> 1); var head = (byte)(track & 1); bytes.Add(track); classifiedMarks.Add(offset);
            var dataOffset = FindNextMark(stream, offset + 4 * 8 * 4, (88 + 16) * 8 * 2);
            var completeData = dataOffset >= 0 && dataOffset + markBits + (sectorSize + 2) * 32 <= stream.Bits.Length;
            bool? dataCrcValid = null;
            if (completeData)
            {
                ushort crc = 0; var data = new byte[sectorSize];
                for (var index = 0; index < sectorSize + 2; index++) { var value = stream.DecodeFmByte32(dataOffset + markBits + index * 32); crc = UpdateCrc(crc, value); if (index < sectorSize) data[index] = value; }
                dataCrcValid = crc == 0; classifiedMarks.Add(dataOffset); bytes.AddRange(data);
                structures.Add(new(FluxStructureKind.FormatData, dataOffset, markBits + (sectorSize + 2) * 32, $"E-mu C{cylinder} H{head} data, CRC {(dataCrcValid == true ? "valid" : "invalid")}"));
            }
            sectors.Add(new(cylinder, head, 1, 0, sectorSize, dataCrcValid, offset));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"E-mu C{cylinder} H{head} R1, 3584 bytes, header CRC valid{(completeData ? $", data CRC {(dataCrcValid == true ? "valid" : "invalid")}" : ", data CRC unavailable")}"));
            offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!classifiedMarks.Contains(offset) && stream.MatchBytes(offset, SectorMark)) structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "E-mu Emulator unclassified header/data mark"));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static int FindNextMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - SectorMark.Length * 8, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) if (stream.MatchBytes(offset, SectorMark)) return offset;
        return -1;
    }

    private static byte ReverseBits(byte value)
    {
        byte reversed = 0;
        for (var bit = 0; bit < 8; bit++) reversed = (byte)((reversed << 1) | ((value >> bit) & 1));
        return reversed;
    }

    private static ushort Crc16(IEnumerable<byte> values)
    {
        ushort crc = 0; foreach (var value in values) crc = UpdateCrc(crc, value); return crc;
    }

    private static ushort UpdateCrc(ushort crc, byte value)
    {
        crc ^= (ushort)(value << 8);
        for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x8005 : crc << 1);
        return crc;
    }
}

public sealed class TycomFmDecoder : SignatureMfmDecoder
{
    private static readonly byte[] HeaderMark = [0x55,0x11,0x15,0x54];
    private static readonly (byte[] Pattern, byte Mark)[] DataMarks = [([0x55,0x11,0x14,0x44], 0xf8), ([0x55,0x11,0x14,0x45], 0xf9), ([0x55,0x11,0x14,0x54], 0xfa), ([0x55,0x11,0x14,0x55], 0xfb)];
    public override string Id => "tycom.fm"; public override string DisplayName => "TYCOM FM";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(HeaderMark, FluxStructureKind.FormatHeader, "TYCOM sector header"), .. DataMarks.Select(item => (item.Pattern, FluxStructureKind.FormatData, $"TYCOM {item.Mark:X2} data"))];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var classifiedData = new HashSet<int>();
        const int markBits = 4 * 8;
        const int headerBits = 5 * 32;
        const int sectorSize = 128;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, HeaderMark)) continue;
            if (offset + headerBits > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "TYCOM sector header")); offset += markBits - 1; continue;
            }
            var cylinder = stream.DecodeFmByte32(offset + 32); var number = stream.DecodeFmByte32(offset + 64);
            var crcHigh = stream.DecodeFmByte32(offset + 96); var crcLow = stream.DecodeFmByte32(offset + 128);
            if (Crc16([0xfe, cylinder, (byte)number, crcHigh, crcLow]) != 0)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"TYCOM C{cylinder} R{number}, header CRC invalid")); offset += markBits - 1; continue;
            }
            bytes.AddRange([cylinder, (byte)number]);

            var data = FindNextDataMark(stream, offset + headerBits, (88 + 16) * 8 * 2);
            var completeData = data.Offset >= 0 && data.Offset + (1 + sectorSize + 2) * 32 <= stream.Bits.Length;
            bool? dataCrcValid = null;
            if (completeData)
            {
                ushort crc = 0xffff; var payload = new byte[sectorSize];
                for (var index = 0; index < 1 + sectorSize + 2; index++) { var value = stream.DecodeFmByte32(data.Offset + index * 32); crc = UpdateCrc(crc, value); if (index is > 0 and <= sectorSize) payload[index - 1] = value; }
                dataCrcValid = crc == 0; classifiedData.Add(data.Offset); bytes.AddRange(payload);
                structures.Add(new(FluxStructureKind.FormatData, data.Offset, (1 + sectorSize + 2) * 32, $"TYCOM {data.Mark:X2} C{cylinder} R{number} data, CRC {(dataCrcValid == true ? "valid" : "invalid")}"));
            }
            sectors.Add(new(cylinder, 0, number, 0, sectorSize, dataCrcValid, offset));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"TYCOM C{cylinder} R{number}, 128 bytes, header CRC valid{(completeData ? $", {data.Mark:X2} data CRC {(dataCrcValid == true ? "valid" : "invalid")}" : ", data CRC unavailable")}"));
            offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (classifiedData.Contains(offset)) continue;
            foreach (var item in DataMarks) if (stream.MatchBytes(offset, item.Pattern)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, $"TYCOM {item.Mark:X2} data")); offset += markBits - 1; break; }
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static (int Offset, byte Mark) FindNextDataMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - HeaderMark.Length * 8, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) foreach (var item in DataMarks) if (stream.MatchBytes(offset, item.Pattern)) return (offset, item.Mark);
        return (-1, 0);
    }

    private static ushort Crc16(IEnumerable<byte> values)
    {
        ushort crc = 0xffff; foreach (var value in values) crc = UpdateCrc(crc, value); return crc;
    }

    private static ushort UpdateCrc(ushort crc, byte value)
    {
        crc ^= (ushort)(value << 8);
        for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
        return crc;
    }
}

public sealed class DecRx02Decoder : SignatureMfmDecoder
{
    private static readonly byte[] HeaderMark = [0x55,0x11,0x15,0x54];
    private static readonly (byte[] Pattern, byte Mark)[] DataMarks = [([0x55,0x11,0x14,0x44], 0xf8), ([0x55,0x11,0x14,0x45], 0xf9), ([0x55,0x11,0x14,0x54], 0xfa), ([0x55,0x11,0x14,0x55], 0xfb), ([0x55,0x11,0x15,0x44], 0xfc), ([0x55,0x11,0x15,0x45], 0xfd)];
    public override string Id => "dec.rx02"; public override string DisplayName => "DEC RX02 M²FM";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(HeaderMark, FluxStructureKind.FormatHeader, "DEC RX02 sector header"), .. DataMarks.Select(item => (item.Pattern, FluxStructureKind.FormatData, $"DEC RX02 {item.Mark:X2} data"))];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var classifiedData = new HashSet<int>();
        const int markBits = 4 * 8;
        const int headerBits = 7 * 32;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, HeaderMark)) continue;
            if (offset + headerBits > stream.Bits.Length)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, markBits, "DEC RX02 sector header")); offset += markBits - 1; continue;
            }
            var cylinder = stream.DecodeFmByte32(offset + 32); var head = stream.DecodeFmByte32(offset + 64); var number = stream.DecodeFmByte32(offset + 96); var sizeCode = stream.DecodeFmByte32(offset + 128);
            var crcHigh = stream.DecodeFmByte32(offset + 160); var crcLow = stream.DecodeFmByte32(offset + 192);
            if (Crc16([0xfe, cylinder, head, number, sizeCode, crcHigh, crcLow]) != 0)
            {
                structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"DEC RX02 C{cylinder} H{head} R{number}, header CRC invalid")); offset += markBits - 1; continue;
            }
            bytes.AddRange([cylinder, head, number, sizeCode]);

            var data = FindNextDataMark(stream, offset + headerBits, (88 + 16) * 8 * 2);
            var m2fm = data.Mark is 0xf9 or 0xfd; var sectorSize = m2fm ? 256 : 128; var decodedCount = sectorSize + 2;
            var completeData = data.Offset >= 0 && (m2fm ? data.Offset + markBits + 1 + decodedCount * 16 : data.Offset + (1 + sectorSize + 2) * 32) <= stream.Bits.Length;
            bool? dataCrcValid = null;
            if (completeData)
            {
                ushort crc = UpdateCrc(0xffff, data.Mark); byte[] payload;
                if (m2fm)
                {
                    var decoded = DecodeM2Fm(stream, data.Offset + markBits + 1, decodedCount); foreach (var value in decoded) crc = UpdateCrc(crc, value); payload = decoded.Take(sectorSize).ToArray();
                }
                else
                {
                    payload = new byte[sectorSize];
                    for (var index = 1; index < 1 + sectorSize + 2; index++) { var value = stream.DecodeFmByte32(data.Offset + index * 32); crc = UpdateCrc(crc, value); if (index <= sectorSize) payload[index - 1] = value; }
                }
                dataCrcValid = crc == 0; classifiedData.Add(data.Offset); bytes.AddRange(payload);
                structures.Add(new(FluxStructureKind.FormatData, data.Offset, m2fm ? markBits + 1 + decodedCount * 16 : (1 + sectorSize + 2) * 32, $"DEC RX02 {data.Mark:X2} C{cylinder} H{head} R{number} {(m2fm ? "M²FM" : "FM")} data, CRC {(dataCrcValid == true ? "valid" : "invalid")}"));
            }
            sectors.Add(new(cylinder, head, number, sizeCode, sectorSize, dataCrcValid, offset));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, headerBits, $"DEC RX02 C{cylinder} H{head} R{number}, {sectorSize} bytes, header CRC valid{(completeData ? $", {data.Mark:X2} data CRC {(dataCrcValid == true ? "valid" : "invalid")}" : ", data CRC unavailable")}"));
            offset += markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (classifiedData.Contains(offset)) continue;
            foreach (var item in DataMarks) if (stream.MatchBytes(offset, item.Pattern)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, $"DEC RX02 {item.Mark:X2} data")); offset += markBits - 1; break; }
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 20d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static (int Offset, byte Mark) FindNextDataMark(FluxBitstream stream, int start, int maximumDistance)
    {
        var end = Math.Min(stream.Bits.Length - HeaderMark.Length * 8, start + maximumDistance);
        for (var offset = start; offset <= end; offset++) foreach (var item in DataMarks) if (stream.MatchBytes(offset, item.Pattern)) return (offset, item.Mark);
        return (-1, 0);
    }

    private static byte[] DecodeM2Fm(FluxBitstream stream, int start, int count)
    {
        var bits = new bool[count * 16 + 1];
        for (var index = 0; index < count * 16 && start + index < stream.Bits.Length; index++) bits[index + 1] = stream.Bits[start + index];
        bool[] encodedRule = [false, true, false, false, false, true, false, false, false, true, false];
        bool[] normalRule = [false, false, true, false, true, false, true, false, true, false, false];
        for (var offset = 0; offset + encodedRule.Length <= bits.Length; offset++)
        {
            var matches = true; for (var index = 0; index < encodedRule.Length; index++) if (bits[offset + index] != encodedRule[index]) { matches = false; break; }
            if (offset % 2 != 0 || !matches) continue;
            for (var index = 0; index < normalRule.Length; index++) bits[offset + index] = normalRule[index];
            offset += encodedRule.Length - 2;
        }
        var result = new byte[count];
        for (var index = 0; index < count; index++)
        {
            byte value = 0;
            for (var bit = 0; bit < 8; bit++) if (!bits[1 + index * 16 + bit * 2] && bits[1 + index * 16 + bit * 2 + 1]) value |= (byte)(1 << (7 - bit));
            result[index] = value;
        }
        return result;
    }

    private static ushort Crc16(IEnumerable<byte> values)
    {
        ushort crc = 0xffff; foreach (var value in values) crc = UpdateCrc(crc, value); return crc;
    }

    private static ushort UpdateCrc(ushort crc, byte value)
    {
        crc ^= (ushort)(value << 8);
        for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1);
        return crc;
    }
}

public sealed class ArburgDecoder : SignatureMfmDecoder
{
    private static readonly byte[] DataMark = [0x44,0x44,0x44,0x44,0x55,0x55,0x55,0x55];
    private static readonly byte[] SystemMark = [0x55,0x55,0x55,0x55,0x55,0x24,0x92,0x49];
    public override string Id => "arburg"; public override string DisplayName => "Arburg system/data";
    protected override bool IsFm => true;
    protected override IReadOnlyList<(byte[], FluxStructureKind, string)> Signatures => [(DataMark, FluxStructureKind.FormatData, "Arburg data block"), (SystemMark, FluxStructureKind.FormatHeader, "Arburg system block")];

    public override FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals);
        var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        ScanFmData(stream, structures, sectors, bytes);
        ScanSystemData(stream, structures, sectors, bytes);
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 8d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static void ScanFmData(FluxBitstream stream, List<FluxStructure> structures, List<DecodedSector> sectors, List<byte> bytes)
    {
        const int markBits = 8 * 8, blockSize = 0xa00, usefulSize = 0x9fe;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, DataMark)) continue;
            var complete = offset + markBits + blockSize * 32 <= stream.Bits.Length; bool? valid = null;
            if (complete)
            {
                ushort checksum = 0; var data = new byte[usefulSize];
                for (var index = 0; index < usefulSize; index++) { var value = ReverseBits(stream.DecodeFmByte32(offset + markBits + index * 32)); data[index] = value; checksum += value; }
                var low = ReverseBits(stream.DecodeFmByte32(offset + markBits + usefulSize * 32)); var high = ReverseBits(stream.DecodeFmByte32(offset + markBits + (usefulSize + 1) * 32));
                valid = low == (byte)checksum && high == (byte)(checksum >> 8); bytes.AddRange(data);
            }
            sectors.Add(new(0, 0, 1, 0, blockSize, valid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatData, offset, complete ? markBits + blockSize * 32 : markBits, $"Arburg data block, 2560 bytes, checksum {(valid is null ? "unavailable" : valid == true ? "valid" : "invalid")}"));
            offset += markBits - 1;
        }
    }

    private static void ScanSystemData(FluxBitstream stream, List<FluxStructure> structures, List<DecodedSector> sectors, List<byte> bytes)
    {
        const int markBits = 8 * 8, blockSize = 0xf00, usefulSize = 0xefe;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, SystemMark)) continue;
            var decoded = TryDecodeSystemBytes(stream, offset + markBits, blockSize); bool? valid = null;
            if (decoded is not null)
            {
                ushort checksum = 0; for (var index = 0; index < usefulSize; index++) checksum += decoded.Value.Bytes[index];
                valid = decoded.Value.Bytes[usefulSize] == (byte)checksum && decoded.Value.Bytes[usefulSize + 1] == (byte)(checksum >> 8); bytes.AddRange(decoded.Value.Bytes.Take(usefulSize));
            }
            sectors.Add(new(0, 0, 1, 0, blockSize, valid, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, decoded is null ? markBits : decoded.Value.EndOffset - offset, $"Arburg system block, 3840 bytes, checksum {(valid is null ? "unavailable" : valid == true ? "valid" : "invalid")}"));
            offset += markBits - 1;
        }
    }

    private static (byte[] Bytes, int EndOffset)? TryDecodeSystemBytes(FluxBitstream stream, int start, int count)
    {
        var result = new byte[count]; var offset = start;
        for (var index = 0; index < count; index++)
        {
            byte value = 0;
            for (var bit = 0; bit < 8; bit++)
            {
                if (offset + 2 > stream.Bits.Length || stream.Bits[offset]) return null;
                if (stream.Bits[offset + 1]) offset += 2;
                else
                {
                    if (offset + 3 > stream.Bits.Length || !stream.Bits[offset + 2]) return null;
                    value |= (byte)(1 << bit); offset += 3;
                }
            }
            result[index] = value;
        }
        return (result, offset);
    }

    private static byte ReverseBits(byte value)
    {
        byte reversed = 0; for (var bit = 0; bit < 8; bit++) reversed = (byte)((reversed << 1) | ((value >> bit) & 1)); return reversed;
    }
}

public sealed class Victor9kGcrDecoder : IFluxDecoder
{
    private static readonly byte[] HeaderMark = [0x55,0x55,0x55,0x55,0x55,0x55,0x11,0x11];
    private static readonly byte[] DataMark = [0x55,0x55,0x55,0x55,0x55,0x55,0x11,0x04];
    private static readonly byte[] Gcr = [0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,0xff,8,0,1,0xff,12,4,5,0xff,0xff,2,3,0xff,15,6,7,0xff,9,10,11,0xff,13,14,0xff];
    public string Id => "victor9k.gcr"; public string DisplayName => "Victor 9000 GCR";

    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromDoubledNrziIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        const int markBits = 64; const int headerBytes = 6; const int sectorBytes = 512; const int decodedDataBytes = 1 + sectorBytes + 2;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, HeaderMark)) continue;
            var header = TryDecodeBytes(stream.Bits, offset + 49, headerBytes); bool? headerValid = null; byte cylinder = 0; byte number = 0;
            if (header is not null)
            {
                cylinder = header.Value.Bytes[1]; number = header.Value.Bytes[2]; headerValid = cylinder + number == header.Value.Bytes[3]; bytes.AddRange(header.Value.Bytes);
            }
            var dataOffset = FindMark(stream, header?.EndOffset ?? offset + markBits, Math.Min(stream.Bits.Length, offset + 98 * 16), DataMark);
            bool? dataValid = null; var structureEnd = header?.EndOffset ?? offset + markBits;
            if (dataOffset >= 0)
            {
                pairedData.Add(dataOffset); var data = TryDecodeBytes(stream.Bits, dataOffset + 49, decodedDataBytes);
                if (data is not null)
                {
                    ushort checksum = 0; for (var index = 0; index < sectorBytes; index++) checksum += data.Value.Bytes[index + 1];
                    var stored = (ushort)(data.Value.Bytes[sectorBytes + 1] | data.Value.Bytes[sectorBytes + 2] << 8); dataValid = checksum == stored; structureEnd = data.Value.EndOffset;
                    bytes.AddRange(data.Value.Bytes.Skip(1).Take(sectorBytes));
                    structures.Add(new(FluxStructureKind.FormatData, dataOffset, data.Value.EndOffset - dataOffset, $"Victor 9000 data block, 512 bytes, checksum {(dataValid == true ? "valid" : "invalid")}"));
                }
                else structures.Add(new(FluxStructureKind.FormatData, dataOffset, markBits, "Victor 9000 data block, checksum unavailable"));
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, 0, number, 2, sectorBytes, integrity, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.FormatHeader, offset, Math.Max(markBits, (header?.EndOffset ?? offset + markBits) - offset), $"Victor 9000 C{cylinder} H0 R{number}, header {(headerValid is null ? "unavailable" : headerValid == true ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
            offset = Math.Max(offset + markBits - 1, structureEnd - 1);
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++) if (stream.MatchBytes(offset, DataMark) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.FormatData, offset, markBits, "Unpaired Victor 9000 data block")); offset += markBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 24d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static int FindMark(FluxBitstream stream, int start, int end, IReadOnlyList<byte> mark)
    {
        for (var offset = Math.Max(0, start); offset + mark.Count * 8 <= end; offset++) if (stream.MatchBytes(offset, mark)) return offset;
        return -1;
    }

    private static (byte[] Bytes, int EndOffset)? TryDecodeBytes(bool[] bits, int start, int count)
    {
        var result = new byte[count]; var offset = start;
        for (var index = 0; index < count; index++)
        {
            if (!TryDecodeNibble(bits, ref offset, out var high) || !TryDecodeNibble(bits, ref offset, out var low)) return null;
            result[index] = (byte)((high << 4) | low);
        }
        return (result, offset);
    }

    private static bool TryDecodeNibble(bool[] bits, ref int offset, out byte value)
    {
        var code = 0; value = 0;
        for (var bit = 0; bit < 5; bit++, offset += 2) { if (offset >= bits.Length) return false; code = (code << 1) | (bits[offset] ? 1 : 0); }
        value = Gcr[code]; return value != 0xff;
    }
}

public sealed class IsoFmDecoder : IFluxDecoder
{
    public string Id => "iso.fm"; public string DisplayName => "ISO FM (simple densité)";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals, fm: true); var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        var headers = new List<(int Offset, byte Cylinder, byte Head, byte Number, byte SizeCode, int Size, bool? Valid)>(); var dataMarks = new List<(int Offset, byte Mark)>();
        for (var offset = 0; offset + 16 <= stream.Bits.Length; offset++)
        {
            var mark = stream.Match(offset, 0xf57e) ? (byte)0xfe : stream.Match(offset, 0xf56f) ? (byte)0xfb : stream.Match(offset, 0xf56a) ? (byte)0xf8 : (byte)0;
            if (mark == 0) continue; bytes.Add(mark);
            var kind = mark == 0xfe ? FluxStructureKind.IdAddressMark : mark == 0xfb ? FluxStructureKind.DataAddressMark : FluxStructureKind.DeletedDataAddressMark;
            var description = mark == 0xfe ? "En-tête de secteur FM" : mark == 0xfb ? "Données de secteur FM" : "Données supprimées FM";
            if (mark == 0xfe && offset + 112 <= stream.Bits.Length)
            {
                var cylinder = stream.DecodeMfmByte(offset + 16); var head = stream.DecodeMfmByte(offset + 32); var number = stream.DecodeMfmByte(offset + 48); var sizeCode = stream.DecodeMfmByte(offset + 64);
                var storedCrc = (ushort)((stream.DecodeMfmByte(offset + 80) << 8) | stream.DecodeMfmByte(offset + 96)); var calculatedCrc = Crc16([0xfe, cylinder, head, number, sizeCode]); var valid = storedCrc == calculatedCrc;
                headers.Add((offset, cylinder, head, number, sizeCode, sizeCode <= 7 ? 128 << sizeCode : 0, valid)); description = $"Secteur FM C{cylinder} H{head} R{number} N{sizeCode}, CRC {(valid ? "valide" : "incorrect")}";
            }
            else if (mark == 0xfe) headers.Add((offset, 0, 0, 0, 0, 0, null));
            else dataMarks.Add((offset, mark));
            structures.Add(new(kind, offset, mark == 0xfe ? 112 : 16, description)); offset += 15;
        }
        structures.RemoveAll(structure => structure.Kind == FluxStructureKind.IdAddressMark);
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index]; var nextHeader = index + 1 < headers.Count ? headers[index + 1].Offset : int.MaxValue;
            (int Offset, byte Mark)? data = dataMarks.Where(candidate => candidate.Offset > header.Offset + 111 && candidate.Offset < nextHeader).Select(candidate => ((int, byte)?)candidate).FirstOrDefault(); bool? dataValid = null;
            if (data is not null && header.Size > 0)
            {
                var end = data.Value.Offset + 16 + (header.Size + 2) * 16;
                if (end <= stream.Bits.Length)
                {
                    var values = Enumerable.Range(0, header.Size).Select(byteIndex => stream.DecodeMfmByte(data.Value.Offset + 16 + byteIndex * 16)).ToArray();
                    var stored = (ushort)((stream.DecodeMfmByte(data.Value.Offset + 16 + header.Size * 16) << 8) | stream.DecodeMfmByte(data.Value.Offset + 16 + (header.Size + 1) * 16)); dataValid = stored == Crc16(new[] { data.Value.Mark }.Concat(values)); bytes.AddRange(values);
                    structures.RemoveAll(structure => structure.BitOffset == data.Value.Offset); structures.Add(new(data.Value.Mark == 0xfb ? FluxStructureKind.DataAddressMark : FluxStructureKind.DeletedDataAddressMark, data.Value.Offset, end - data.Value.Offset, $"FM {(data.Value.Mark == 0xf8 ? "deleted " : "")}data, {header.Size} bytes, CRC {(dataValid == true ? "valid" : "invalid")}"));
                }
            }
            bool? integrity = header.Valid == false || dataValid == false ? false : dataValid is null ? null : true; sectors.Add(new(header.Cylinder, header.Head, header.Number, header.SizeCode, header.Size, integrity, header.Offset));
            structures.Add(new(FluxStructureKind.IdAddressMark, header.Offset, header.Valid is null ? 16 : 112, $"FM C{header.Cylinder} H{header.Head} R{header.Number} N{header.SizeCode}, header CRC {(header.Valid is null ? "unavailable" : header.Valid == true ? "valid" : "invalid")}, data CRC {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 18d), stream.BitCellTicks, structures, bytes, sectors);
    }
    private static ushort Crc16(IEnumerable<byte> values) { ushort crc = 0xffff; foreach (var value in values) { crc ^= (ushort)(value << 8); for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1); } return crc; }
}

public sealed class RawFluxDecoder : IFluxDecoder
{
    public string Id => "raw"; public string DisplayName => "Flux brut";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var median = FluxBitstream.EstimateBitCell(revolution.FluxIntervals);
        var anomalies = new List<FluxStructure>();
        var bitOffset = 0;
        for (var index = 0; index < revolution.FluxIntervals.Count; index++)
        {
            var interval = revolution.FluxIntervals[index];
            var bitLength = Math.Clamp((int)Math.Round(interval / median), 1, 64);
            if (interval > median * 10) anomalies.Add(new(FluxStructureKind.TimingAnomaly, bitOffset, bitLength, "Intervalle de flux exceptionnellement long."));
            else if (index > 0 && interval < median * .55) anomalies.Add(new(FluxStructureKind.TimingAnomaly, bitOffset, bitLength, "Impulsion de flux exceptionnellement courte."));
            bitOffset += bitLength;
        }
        return new(Id, DisplayName, .05, median, anomalies, []);
    }
}

public sealed class IsoMfmDecoder : IFluxDecoder
{
    public string Id => "iso.mfm"; public string DisplayName => "ISO MFM (Atari ST / IBM PC)";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var bytes = new List<byte>(); var sectors = new List<DecodedSector>();
        var headers = new List<(int Offset, byte Cylinder, byte Head, byte Number, byte SizeCode, int Size, bool? Valid)>(); var dataMarks = new List<(int Offset, byte Mark)>();
        for (var offset = 0; offset + 64 <= stream.Bits.Length; offset++)
        {
            if (!stream.Match(offset, 0x4489) || !stream.Match(offset + 16, 0x4489) || !stream.Match(offset + 32, 0x4489)) continue;
            var mark = stream.DecodeMfmByte(offset + 48); var kind = mark switch { 0xfe => FluxStructureKind.IdAddressMark, 0xfb => FluxStructureKind.DataAddressMark, 0xf8 => FluxStructureKind.DeletedDataAddressMark, _ => FluxStructureKind.Sync };
            var description = mark is 0xfe ? "En-tête de secteur MFM" : mark is 0xfb ? "Données de secteur MFM" : mark is 0xf8 ? "Données supprimées MFM" : $"Synchronisation MFM, marque 0x{mark:X2}";
            if (mark == 0xfe && offset + 160 <= stream.Bits.Length)
            {
                var cylinder = stream.DecodeMfmByte(offset + 64); var head = stream.DecodeMfmByte(offset + 80); var number = stream.DecodeMfmByte(offset + 96); var sizeCode = stream.DecodeMfmByte(offset + 112);
                var storedCrc = (ushort)((stream.DecodeMfmByte(offset + 128) << 8) | stream.DecodeMfmByte(offset + 144));
                var calculatedCrc = Crc16([0xa1, 0xa1, 0xa1, 0xfe, cylinder, head, number, sizeCode]); var valid = storedCrc == calculatedCrc;
                headers.Add((offset, cylinder, head, number, sizeCode, sizeCode <= 7 ? 128 << sizeCode : 0, valid));
                description = $"Secteur C{cylinder} H{head} R{number} N{sizeCode} ({(sizeCode <= 7 ? 128 << sizeCode : 0)} octets), CRC {(valid ? "valide" : "incorrect")}";
            }
            else if (mark == 0xfe) headers.Add((offset, 0, 0, 0, 0, 0, null));
            else if (mark is 0xfb or 0xf8) dataMarks.Add((offset, mark));
            structures.Add(new(kind, offset, mark == 0xfe ? 160 : 64, description)); bytes.Add(mark); offset += 47;
        }
        structures.RemoveAll(structure => structure.Kind == FluxStructureKind.IdAddressMark);
        for (var index = 0; index < headers.Count; index++)
        {
            var header = headers[index]; var nextHeader = index + 1 < headers.Count ? headers[index + 1].Offset : int.MaxValue;
            (int Offset, byte Mark)? data = dataMarks.Where(candidate => candidate.Offset > header.Offset + 159 && candidate.Offset < nextHeader).Select(candidate => ((int, byte)?)candidate).FirstOrDefault(); bool? dataValid = null;
            if (data is not null && header.Size > 0)
            {
                var end = data.Value.Offset + 64 + (header.Size + 2) * 16;
                if (end <= stream.Bits.Length)
                {
                    var values = Enumerable.Range(0, header.Size).Select(byteIndex => stream.DecodeMfmByte(data.Value.Offset + 64 + byteIndex * 16)).ToArray();
                    var stored = (ushort)((stream.DecodeMfmByte(data.Value.Offset + 64 + header.Size * 16) << 8) | stream.DecodeMfmByte(data.Value.Offset + 64 + (header.Size + 1) * 16)); dataValid = stored == Crc16(new byte[] { 0xa1,0xa1,0xa1,data.Value.Mark }.Concat(values)); bytes.AddRange(values);
                    structures.RemoveAll(structure => structure.BitOffset == data.Value.Offset); structures.Add(new(data.Value.Mark == 0xfb ? FluxStructureKind.DataAddressMark : FluxStructureKind.DeletedDataAddressMark, data.Value.Offset, end - data.Value.Offset, $"MFM {(data.Value.Mark == 0xf8 ? "deleted " : "")}data, {header.Size} bytes, CRC {(dataValid == true ? "valid" : "invalid")}"));
                }
            }
            bool? integrity = header.Valid == false || dataValid == false ? false : dataValid is null ? null : true; sectors.Add(new(header.Cylinder, header.Head, header.Number, header.SizeCode, header.Size, integrity, header.Offset));
            structures.Add(new(FluxStructureKind.IdAddressMark, header.Offset, header.Valid is null ? 64 : 160, $"MFM C{header.Cylinder} H{header.Head} R{header.Number} N{header.SizeCode}, header CRC {(header.Valid is null ? "unavailable" : header.Valid == true ? "valid" : "invalid")}, data CRC {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
        }
        var confidence = Math.Min(1, (sectors.Count * 2 + structures.Count(x => x.Kind is FluxStructureKind.DataAddressMark or FluxStructureKind.DeletedDataAddressMark)) / 12d);
        return new(Id, DisplayName, confidence, stream.BitCellTicks, structures, bytes, sectors);
    }

    private static ushort Crc16(IEnumerable<byte> values) { ushort crc = 0xffff; foreach (var value in values) { crc ^= (ushort)(value << 8); for (var bit = 0; bit < 8; bit++) crc = (ushort)((crc & 0x8000) != 0 ? (crc << 1) ^ 0x1021 : crc << 1); } return crc; }
}

public sealed class AmigaMfmDecoder : IFluxDecoder
{
    public string Id => "amiga.mfm"; public string DisplayName => "Amiga MFM";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>();
        const int encodedBytes = 540; const int headerBytes = 28; const int dataOffset = 28; const int dataBytes = 512;
        for (var offset = 0; offset + 32 <= stream.Bits.Length; offset++)
        {
            if (!stream.Match(offset, 0x4489) || !stream.Match(offset + 16, 0x4489)) continue;
            var encoded = TryDecodeMfmBytes(stream, offset + 32, encodedBytes); var available = encoded ?? TryDecodeMfmBytes(stream, offset + 32, headerBytes);
            bool? headerValid = null; bool? dataValid = null; byte cylinder = 0; byte head = 0; byte number = 0; var length = 32;
            if (available is not null)
            {
                var header = DecodeOddEven(available.Take(4).ToArray()); cylinder = (byte)(header[1] >> 1); head = (byte)(header[1] & 1); number = header[2];
                var headerParity = CalculateParity(available, 0, 20); headerValid = header[0] == 0xff && available[22] == headerParity.High && available[23] == headerParity.Low;
                bytes.AddRange(header); length = 32 + available.Length * 16;
                if (encoded is not null)
                {
                    var parity = CalculateSplitParity(encoded, dataOffset, dataBytes); dataValid = encoded[26] == parity.High && encoded[27] == parity.Low;
                    var data = DecodeOddEven(encoded.Skip(dataOffset).Take(dataBytes).ToArray()); bytes.AddRange(data); length = 32 + encodedBytes * 16;
                }
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, head, number, 2, 512, integrity, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.AmigaSync, offset, length, $"Amiga C{cylinder} H{head} S{number}, header checksum {(headerValid is null ? "unavailable" : headerValid == true ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
            offset += Math.Max(31, length - 1);
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 3 + structures.Count) / 44d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte[]? TryDecodeMfmBytes(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * 16 > stream.Bits.Length) return null; var result = new byte[count];
        for (var index = 0; index < count; index++) result[index] = stream.DecodeMfmByte(offset + index * 16);
        return result;
    }
    private static byte[] DecodeOddEven(IReadOnlyList<byte> encoded)
    {
        var result = new byte[encoded.Count]; var half = encoded.Count / 2;
        for (var index = 0; index < half; index++)
        {
            var odd = encoded[index]; var even = encoded[index + half]; result[index * 2] = Interleave((byte)(odd >> 4), (byte)(even >> 4)); result[index * 2 + 1] = Interleave((byte)(odd & 15), (byte)(even & 15));
        }
        return result;
    }
    private static byte Interleave(byte odd, byte even)
    {
        byte value = 0; for (var index = 0; index < 4; index++) { value |= (byte)(((odd >> (3 - index)) & 1) << (7 - index * 2)); value |= (byte)(((even >> (3 - index)) & 1) << (6 - index * 2)); } return value;
    }
    private static (byte High, byte Low) CalculateParity(IReadOnlyList<byte> encoded, int offset, int count)
    {
        byte high = 0, low = 0; for (var index = 0; index < count; index += 4) { high ^= (byte)(encoded[offset + index] ^ encoded[offset + index + 2]); low ^= (byte)(encoded[offset + index + 1] ^ encoded[offset + index + 3]); } return (high, low);
    }
    private static (byte High, byte Low) CalculateSplitParity(IReadOnlyList<byte> encoded, int offset, int count)
    {
        byte high = 0, low = 0; var half = count / 2;
        for (var index = 0; index < half; index += 2) { high ^= (byte)(encoded[offset + index] ^ encoded[offset + half + index]); low ^= (byte)(encoded[offset + index + 1] ^ encoded[offset + half + index + 1]); } return (high, low);
    }
}

public sealed class AppleGcrDecoder : IFluxDecoder
{
    private static readonly byte[] SixAndTwo = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
    private static readonly Dictionary<byte, byte> InverseSixAndTwo = SixAndTwo.Select((value, index) => (value, index)).ToDictionary(x => x.value, x => (byte)x.index);
    public string Id => "apple2.gcr"; public string DisplayName => "Apple II GCR";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromNrziIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var bytes = new List<byte>(); var sectors = new List<DecodedSector>(); var pairedData = new HashSet<int>();
        for (var offset = 0; offset + 24 <= stream.Bits.Length; offset++)
        {
            if (!stream.Match(offset, 0xD5AA96, 24)) continue;
            var address = TryReadBytes(stream.Bits, offset + 24, 8); bool? headerValid = null; byte volume = 0; byte cylinder = 0; byte number = 0;
            if (address is not null)
            {
                volume = DecodeFourAndFour(address[0], address[1]); cylinder = DecodeFourAndFour(address[2], address[3]); number = DecodeFourAndFour(address[4], address[5]);
                var checksum = DecodeFourAndFour(address[6], address[7]); headerValid = (byte)(volume ^ cylinder ^ number) == checksum; bytes.AddRange([volume, cylinder, number, checksum]);
            }
            var headerEnd = offset + 24 + (address is null ? 0 : 64); var epilogueOffset = Find(stream, headerEnd, Math.Min(stream.Bits.Length, headerEnd + 512), 0xDEAAEB);
            var dataOffset = epilogueOffset < 0 ? -1 : Find(stream, epilogueOffset + 24, Math.Min(stream.Bits.Length, epilogueOffset + 24 + 512), 0xD5AAAD); bool? dataValid = null; var structureEnd = headerEnd;
            if (dataOffset >= 0)
            {
                pairedData.Add(dataOffset); var data = TryDecodeSixAndTwo(stream.Bits, dataOffset + 24);
                if (data is not null)
                {
                    dataValid = data.Value.Valid; structureEnd = data.Value.EndOffset; bytes.AddRange(data.Value.Data);
                    structures.Add(new(FluxStructureKind.AppleData, dataOffset, data.Value.EndOffset - dataOffset, $"Apple II data block, 256 bytes, checksum {(dataValid == true ? "valid" : "invalid")}"));
                }
                else structures.Add(new(FluxStructureKind.AppleData, dataOffset, 24, "Apple II data block, checksum unavailable"));
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, 0, number, 1, 256, integrity, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.AppleAddress, offset, Math.Max(24, headerEnd - offset), $"Apple II V{volume} T{cylinder} S{number}, address checksum {(headerValid is null ? "unavailable" : headerValid == true ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
            offset = headerValid == true ? Math.Max(offset + 23, structureEnd - 1) : offset + 23;
        }
        for (var offset = 0; offset + 24 <= stream.Bits.Length; offset++) if (stream.Match(offset, 0xD5AAAD, 24) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.AppleData, offset, 24, "Unpaired Apple II data prologue D5 AA AD")); offset += 23; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 32d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte DecodeFourAndFour(byte high, byte low) => (byte)(((high << 1) | 1) & low);
    private static byte[]? TryReadBytes(bool[] bits, int offset, int count)
    {
        if (offset + count * 8 > bits.Length) return null; var result = new byte[count];
        for (var index = 0; index < count; index++) for (var bit = 0; bit < 8; bit++) if (bits[offset + index * 8 + bit]) result[index] |= (byte)(1 << (7 - bit));
        return result;
    }
    private static int Find(FluxBitstream stream, int start, int end, uint mark)
    {
        for (var offset = Math.Max(0, start); offset + 24 <= end; offset++) if (stream.Match(offset, mark, 24)) return offset;
        return -1;
    }
    private static (byte[] Data, bool Valid, int EndOffset)? TryDecodeSixAndTwo(bool[] bits, int offset)
    {
        var encoded = TryReadBytes(bits, offset, 343); if (encoded is null) return null; var values = new byte[343];
        for (var index = 0; index < values.Length; index++) if (!InverseSixAndTwo.TryGetValue(encoded[index], out values[index])) return null;
        var decoded = new byte[342]; byte previous = 0; var encodedIndex = 0;
        for (var index = 341; index >= 256; index--) { decoded[index] = (byte)(values[encodedIndex++] ^ previous); previous = decoded[index]; }
        for (var index = 0; index < 256; index++) { decoded[index] = (byte)(values[encodedIndex++] ^ previous); previous = decoded[index]; }
        var valid = (byte)(values[342] ^ previous) == 0; var data = new byte[256]; byte auxiliaryOffset = 0;
        for (var index = 0; index < 256; index++)
        {
            auxiliaryOffset = (byte)((auxiliaryOffset + 85) % 86); var auxiliary = decoded[256 + auxiliaryOffset]; decoded[256 + auxiliaryOffset] = (byte)(auxiliary >> 2);
            data[index] = (byte)((decoded[index] << 2) | ((auxiliary & 2) >> 1) | ((auxiliary & 1) << 1));
        }
        return (data, valid, offset + 343 * 8);
    }
}

public sealed class AppleMacGcrDecoder : IFluxDecoder
{
    private static readonly byte[] SixAndTwo = [0x96,0x97,0x9a,0x9b,0x9d,0x9e,0x9f,0xa6,0xa7,0xab,0xac,0xad,0xae,0xaf,0xb2,0xb3,0xb4,0xb5,0xb6,0xb7,0xb9,0xba,0xbb,0xbc,0xbd,0xbe,0xbf,0xcb,0xcd,0xce,0xcf,0xd3,0xd6,0xd7,0xd9,0xda,0xdb,0xdc,0xdd,0xde,0xdf,0xe5,0xe6,0xe7,0xe9,0xea,0xeb,0xec,0xed,0xee,0xef,0xf2,0xf3,0xf4,0xf5,0xf6,0xf7,0xf9,0xfa,0xfb,0xfc,0xfd,0xfe,0xff];
    private static readonly Dictionary<byte, byte> Inverse = SixAndTwo.Select((value, index) => (value, index)).ToDictionary(item => item.value, item => (byte)item.index);
    private static readonly byte[] AddressMark = ExpandCells(0xd5, 0xaa, 0x96);
    private static readonly byte[] DataMark = ExpandCells(0xd5, 0xaa, 0xad);
    public string Id => "applemac.gcr"; public string DisplayName => "Apple Macintosh GCR";

    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromDoubledNrziIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var sectors = new List<DecodedSector>(); var bytes = new List<byte>(); var pairedData = new HashSet<int>();
        const int markBits = 48; const int headerSymbols = 5; const int dataSymbols = 704;
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++)
        {
            if (!stream.MatchBytes(offset, AddressMark)) continue;
            var header = TryReadSymbols(stream, offset + markBits, headerSymbols); bool? headerValid = null; byte cylinder = 0, head = 0, number = 0;
            if (header is not null && header.All(Inverse.ContainsKey))
            {
                var values = header.Select(value => Inverse[value]).ToArray();
                cylinder = (byte)(((values[2] & 3) << 6) | (values[0] & 0x3f)); head = (byte)((values[2] >> 5) & 1); number = values[1];
                headerValid = (byte)((values[0] ^ values[1] ^ values[2] ^ values[3]) & 0x3f) == values[4];
            }
            var headerEnd = offset + markBits + (header is null ? 0 : headerSymbols * 16);
            var dataOffset = headerValid == true ? FindMark(stream, headerEnd, Math.Min(stream.Bits.Length, headerEnd + 512), DataMark) : -1;
            bool? dataValid = null; var structureEnd = headerEnd;
            if (dataOffset >= 0)
            {
                pairedData.Add(dataOffset); var encoded = TryReadSymbols(stream, dataOffset + markBits, dataSymbols);
                if (encoded is not null && encoded.All(Inverse.ContainsKey))
                {
                    var values = encoded.Select(value => Inverse[value]).ToArray(); var decoded = DecodeSixAndTwo(values.AsSpan(1, 699), out var checksum);
                    dataValid = checksum[3] == values[700] && checksum[2] == values[701] && checksum[1] == values[702] && checksum[0] == values[703];
                    var data = decoded.Skip(12).Take(512).ToArray(); bytes.AddRange(data); structureEnd = dataOffset + markBits + dataSymbols * 16;
                    structures.Add(new(FluxStructureKind.AppleData, dataOffset, structureEnd - dataOffset, $"Apple Macintosh data block, 512 bytes, checksum {(dataValid == true ? "valid" : "invalid")}"));
                }
                else structures.Add(new(FluxStructureKind.AppleData, dataOffset, markBits, "Apple Macintosh data block, checksum unavailable"));
            }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, head, number, 2, 512, integrity, offset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.AppleAddress, offset, Math.Max(markBits, headerEnd - offset), $"Apple Macintosh C{cylinder} H{head} S{number}, address checksum {(headerValid is null ? "unavailable" : headerValid == true ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
            offset = headerValid == true ? Math.Max(offset + markBits - 1, structureEnd - 1) : offset + markBits - 1;
        }
        for (var offset = 0; offset + markBits <= stream.Bits.Length; offset++) if (stream.MatchBytes(offset, DataMark) && !pairedData.Contains(offset)) { structures.Add(new(FluxStructureKind.AppleData, offset, markBits, "Unpaired Apple Macintosh data prologue")); offset += markBits - 1; }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 24d), stream.BitCellTicks, structures.OrderBy(item => item.BitOffset).ToArray(), bytes, sectors);
    }

    private static byte[] DecodeSixAndTwo(ReadOnlySpan<byte> symbols, out byte[] checksum)
    {
        var b1 = new byte[175]; var b2 = new byte[175]; var b3 = new byte[175]; var source = 0;
        for (var index = 0; index <= 174; index++)
        {
            var w4 = symbols[source++]; var w1 = symbols[source++]; var w2 = symbols[source++]; var w3 = index == 174 ? (byte)0 : symbols[source++];
            b1[index] = (byte)((w1 & 0x3f) | ((w4 << 2) & 0xc0)); b2[index] = (byte)((w2 & 0x3f) | ((w4 << 4) & 0xc0)); b3[index] = (byte)((w3 & 0x3f) | ((w4 << 6) & 0xc0));
        }
        var output = new byte[524]; uint c1 = 0, c2 = 0, c3 = 0; var destination = 0;
        for (var index = 0; ; index++)
        {
            c1 = (c1 & 0xff) << 1; if ((c1 & 0x100) != 0) c1++;
            var value = (byte)(b1[index] ^ c1); c3 += value; if ((c1 & 0x100) != 0) { c3++; c1 &= 0xff; } output[destination++] = value;
            value = (byte)(b2[index] ^ c3); c2 += value; if (c3 > 0xff) { c2++; c3 &= 0xff; } output[destination++] = value;
            if (destination == 524) break;
            value = (byte)(b3[index] ^ c2); c1 += value; if (c2 > 0xff) { c1++; c2 &= 0xff; } output[destination++] = value;
        }
        checksum = [(byte)(c1 & 0x3f), (byte)(c2 & 0x3f), (byte)(c3 & 0x3f), (byte)(((c1 & 0xc0) >> 6) | ((c2 & 0xc0) >> 4) | ((c3 & 0xc0) >> 2))];
        return output;
    }

    private static byte[]? TryReadSymbols(FluxBitstream stream, int offset, int count)
    {
        if (offset + count * 16 > stream.Bits.Length) return null;
        return Enumerable.Range(0, count).Select(index => stream.DecodeMfmByte(offset + index * 16)).ToArray();
    }
    private static int FindMark(FluxBitstream stream, int start, int end, IReadOnlyList<byte> mark)
    {
        for (var offset = start; offset + mark.Count * 8 <= end; offset++) if (stream.MatchBytes(offset, mark)) return offset;
        return -1;
    }
    private static byte[] ExpandCells(params byte[] values)
    {
        var result = new byte[values.Length * 2]; var target = 0;
        foreach (var value in values)
        {
            ushort expanded = 0; for (var bit = 7; bit >= 0; bit--) expanded = (ushort)((expanded << 2) | ((value >> bit) & 1));
            result[target++] = (byte)(expanded >> 8); result[target++] = (byte)expanded;
        }
        return result;
    }
}

public sealed class CommodoreGcrDecoder : IFluxDecoder
{
    private static readonly Dictionary<int, int> Gcr = new() { [0b01010]=0,[0b01011]=1,[0b10010]=2,[0b10011]=3,[0b01110]=4,[0b01111]=5,[0b10110]=6,[0b10111]=7,[0b01001]=8,[0b11001]=9,[0b11010]=10,[0b11011]=11,[0b01101]=12,[0b11101]=13,[0b11110]=14,[0b10101]=15 };
    public string Id => "commodore.gcr"; public string DisplayName => "Commodore GCR";
    public FluxDecodeResult Decode(ScpRevolution revolution)
    {
        var stream = FluxBitstream.FromNrziIntervals(revolution.FluxIntervals); var structures = new List<FluxStructure>(); var bytes = new List<byte>(); var sectors = new List<DecodedSector>();
        var headers = new List<(int SyncOffset, int DataOffset, int EndOffset, byte[]? Bytes)>(); var dataBlocks = new List<(int SyncOffset, int DataOffset, int EndOffset, byte[]? Bytes)>();
        for (var offset = 0; offset < stream.Bits.Length; offset++)
        {
            if (!stream.Bits[offset]) continue; var end = offset; while (end < stream.Bits.Length && stream.Bits[end]) end++;
            var length = end - offset;
            if (length >= 10)
            {
                structures.Add(new(FluxStructureKind.CommodoreSync, offset, length, "Commodore GCR sync"));
                if (TryDecodeByte(stream.Bits, end, out var value))
                {
                    if (value == 0x08) { var decoded = TryDecodeBytes(stream.Bits, end, 6); headers.Add((offset, end, decoded is null ? end + 10 : end + 60, decoded)); if (decoded is not null) bytes.AddRange(decoded); else bytes.Add(value); }
                    else if (value == 0x07) { var decoded = TryDecodeBytes(stream.Bits, end, 258); dataBlocks.Add((offset, end, decoded is null ? end + 10 : end + 2580, decoded)); if (decoded is not null) bytes.AddRange(decoded); else bytes.Add(value); }
                    else bytes.Add(value);
                }
            }
            offset = end;
        }
        foreach (var block in dataBlocks)
        {
            bool? valid = null;
            if (block.Bytes is not null) { byte checksum = 0; for (var index = 1; index < 258; index++) checksum ^= block.Bytes[index]; valid = checksum == 0; }
            structures.Add(new(FluxStructureKind.FormatData, block.SyncOffset, Math.Max(10, block.EndOffset - block.SyncOffset), $"Commodore data block, 256 bytes, checksum {(valid is null ? "unavailable" : valid == true ? "valid" : "invalid")}"));
        }
        foreach (var block in headers)
        {
            bool? headerValid = null; byte cylinder = 0; byte number = 0;
            if (block.Bytes is not null)
            {
                cylinder = block.Bytes[3]; number = block.Bytes[2]; headerValid = block.Bytes[0] == 0x08 && (byte)(block.Bytes[1] ^ block.Bytes[2] ^ block.Bytes[3] ^ block.Bytes[4] ^ block.Bytes[5]) == 0;
            }
            var data = dataBlocks.FirstOrDefault(candidate => candidate.SyncOffset > block.EndOffset && candidate.SyncOffset - block.EndOffset < 784); bool? dataValid = null;
            if (data.Bytes is not null) { byte checksum = 0; for (var index = 1; index < 258; index++) checksum ^= data.Bytes[index]; dataValid = checksum == 0; }
            bool? integrity = headerValid == false || dataValid == false ? false : dataValid is null ? null : true;
            sectors.Add(new(cylinder, 0, number, 1, 256, integrity, block.SyncOffset, SectorIntegrityKind.Checksum));
            structures.Add(new(FluxStructureKind.CommodoreHeader, block.SyncOffset, Math.Max(10, block.EndOffset - block.SyncOffset), $"Commodore T{cylinder} S{number}, header checksum {(headerValid is null ? "unavailable" : headerValid == true ? "valid" : "invalid")}, data checksum {(dataValid is null ? "unavailable" : dataValid == true ? "valid" : "invalid")}"));
        }
        return new(Id, DisplayName, Math.Min(1, (sectors.Count * 2 + structures.Count) / 42d), stream.BitCellTicks, structures, bytes, sectors);
    }

    private static byte[]? TryDecodeBytes(bool[] bits, int offset, int count)
    {
        var result = new byte[count];
        for (var index = 0; index < count; index++) if (!TryDecodeByte(bits, offset + index * 10, out result[index])) return null;
        return result;
    }

    private static bool TryDecodeByte(bool[] bits, int offset, out byte value)
    {
        value = 0; if (offset + 10 > bits.Length) return false;
        var high = 0; var low = 0;
        for (var bit = 0; bit < 5; bit++) { high = (high << 1) | (bits[offset + bit] ? 1 : 0); low = (low << 1) | (bits[offset + 5 + bit] ? 1 : 0); }
        if (!Gcr.TryGetValue(high, out var highNibble) || !Gcr.TryGetValue(low, out var lowNibble)) return false;
        value = (byte)((highNibble << 4) | lowNibble); return true;
    }
}

internal sealed class FluxBitstream(bool[] bits, double bitCellTicks)
{
    public bool[] Bits { get; } = bits; public double BitCellTicks { get; } = bitCellTicks;
    public static FluxBitstream FromIntervals(IReadOnlyList<uint> intervals, bool fm = false)
    {
        return Reconstruct(intervals, EstimateBitCell(intervals, fm), 32);
    }
    public static FluxBitstream FromNrziIntervals(IReadOnlyList<uint> intervals)
    {
        return Reconstruct(intervals, EstimateBitCell(intervals, true), 64);
    }
    public static FluxBitstream FromDoubledNrziIntervals(IReadOnlyList<uint> intervals)
    {
        return Reconstruct(intervals, EstimateBitCell(intervals), 64);
    }
    private static FluxBitstream Reconstruct(IReadOnlyList<uint> intervals, double initialCell, int maximumCells)
    {
        var currentCell = initialCell; var accumulatedCell = 0d; var samples = 0; var bits = new List<bool>(intervals.Count * 4);
        for (var index = 0; index < intervals.Count; index++)
        {
            var interval = intervals[index]; var cells = Math.Clamp((int)Math.Round(interval / currentCell), 1, maximumCells);
            for (var zero = 1; zero < cells; zero++) bits.Add(false); bits.Add(true);
            if (index == 0) continue;
            var observedCell = interval / (double)cells;
            if (observedCell >= currentCell * .7 && observedCell <= currentCell * 1.3) currentCell += (observedCell - currentCell) * .08;
            accumulatedCell += currentCell; samples++;
        }
        return new(bits.ToArray(), samples == 0 ? initialCell : accumulatedCell / samples);
    }
    public static double EstimateBitCell(IReadOnlyList<uint> intervals, bool fm = false)
    {
        if (intervals.Count == 0) return 1;
        // The first interval starts at the index pulse rather than at a previous flux transition,
        // so it is not a complete cell-spacing sample and must not drive the PLL estimate.
        var samples = fm ? intervals : intervals.Skip(1);
        var sorted = samples.Where(x => x > 0).Order().ToArray(); if (sorted.Length == 0) sorted = intervals.Where(x => x > 0).Order().ToArray(); if (sorted.Length == 0) return 1;
        var sampleLength = Math.Max(1, sorted.Length / 5); var lowerCluster = sorted.Take(sampleLength).ToArray(); var robustLower = lowerCluster[lowerCluster.Length / 2];
        return Math.Max(1, fm ? robustLower : robustLower / 2d);
    }
    public bool Match(int offset, ushort pattern) { if (offset + 16 > Bits.Length) return false; for (var bit = 0; bit < 16; bit++) if (Bits[offset + bit] != ((pattern & (1 << (15 - bit))) != 0)) return false; return true; }
    public bool Match(int offset, uint pattern, int length) { if (length is < 1 or > 32 || offset + length > Bits.Length) return false; for (var bit = 0; bit < length; bit++) if (Bits[offset + bit] != ((pattern & (1u << (length - 1 - bit))) != 0)) return false; return true; }
    public bool MatchBytes(int offset, IReadOnlyList<byte> pattern) { if (offset + pattern.Count * 8 > Bits.Length) return false; for (var index = 0; index < pattern.Count; index++) for (var bit = 0; bit < 8; bit++) if (Bits[offset + index * 8 + bit] != ((pattern[index] & (1 << (7 - bit))) != 0)) return false; return true; }
    public byte DecodeMfmByte(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit * 2 + 1 < Bits.Length; bit++) if (Bits[offset + bit * 2 + 1]) value |= (byte)(1 << (7 - bit)); return value; }
    public byte DecodeFmByte32(int offset) { byte value = 0; for (var bit = 0; bit < 8 && offset + bit * 4 + 3 < Bits.Length; bit++) if (Bits[offset + bit * 4 + 3]) value |= (byte)(1 << (7 - bit)); return value; }
}
