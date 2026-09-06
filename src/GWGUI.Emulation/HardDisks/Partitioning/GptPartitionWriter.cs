using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.Partitioning;

/// <summary>GPT with 128 entries, two complete metadata copies and zeroed reserved bytes.</summary>
public static class GptPartitionWriter
{
    private const int EntriesBytes = 128 * 128;
    private static readonly Encoding Names = new UnicodeEncoding(false, false, true);

    public static DiscUtils.Partitions.PartitionInfo Create(Stream disk, DiscUtils.Partitions.WellKnownPartitionType type)
    {
        // Preserve the legacy automatic layout, while initializing metadata and clearing reserved tails explicitly.
        Write(disk, []);
        var table = new DiscUtils.Partitions.GuidPartitionTable(disk, DiscUtils.Geometry.FromCapacity(disk.Length));
        var index = table.Create(type, true);
        var reserved = new byte[512 - 92];
        disk.Position = 512 + 92; disk.Write(reserved);
        disk.Position = disk.Length - 512 + 92; disk.Write(reserved);
        disk.Flush();
        return table.Partitions[index];
    }

    public static DiskFormatRegistry.PartitionTable Describe(string id, int sectorBytes = 512) =>
        new(id, (size, volumes) => Validate(size, volumes, sectorBytes),
            (stream, volumes) => Write(stream, volumes, sectorBytes))
        {
            LogicalSectorBytes = sectorBytes,
            Identity = new("GPT", $"128 entries, {sectorBytes}-byte logical sectors")
        };

    public static void Validate(long capacity, IReadOnlyList<DiskVolumePlan> volumes, int sectorBytes = 512)
    {
        if (sectorBytes is not (512 or 1024 or 2048 or 4096)) throw new ArgumentOutOfRangeException(nameof(sectorBytes));
        var arraySectors = EntriesBytes / sectorBytes;
        if (capacity % sectorBytes != 0 || capacity < (2L * arraySectors + 4) * sectorBytes || volumes.Count > 128)
            throw new ArgumentException("GPT requires aligned capacity and space for both metadata copies.");
        long end = (2L + arraySectors) * sectorBytes;
        foreach (var volume in volumes.OrderBy(value => value.OffsetBytes))
        {
            if (volume.OffsetBytes < end || volume.OffsetBytes % sectorBytes != 0 || volume.LengthBytes <= 0 ||
                volume.LengthBytes % sectorBytes != 0 || volume.OffsetBytes > capacity - (1L + arraySectors) * sectorBytes ||
                volume.LengthBytes > capacity - (1L + arraySectors) * sectorBytes - volume.OffsetBytes ||
                PartitionTypeDefaults.Gpt(volume) == Guid.Empty)
                throw new ArgumentException("GPT volumes overlap or occupy reserved or unaligned sectors.");
            var name = volume.EffectivePartitionName;
            if (name.Contains('\0') || Names.GetByteCount(name) > 72)
                throw new ArgumentException("GPT partition names must contain at most 36 UTF-16 code units and no null character.");
            end = checked(volume.OffsetBytes + volume.LengthBytes);
        }
    }

    public static void Write(Stream stream, IReadOnlyList<DiskVolumePlan> volumes, int sectorBytes = 512)
    {
        if (!stream.CanWrite || !stream.CanSeek) throw new ArgumentException("A writable seekable disk is required.");
        Validate(stream.Length, volumes, sectorBytes);
        var sectors = stream.Length / sectorBytes;
        var arraySectors = EntriesBytes / sectorBytes;
        var entries = new byte[EntriesBytes];
        for (var index = 0; index < volumes.Count; index++)
        {
            var volume = volumes[index];
            var entry = entries.AsSpan(index * 128, 128);
            PartitionTypeDefaults.Gpt(volume).TryWriteBytes(entry);
            Guid.NewGuid().TryWriteBytes(entry[16..]);
            Put64(entry, 32, (ulong)(volume.OffsetBytes / sectorBytes));
            Put64(entry, 40, (ulong)((volume.OffsetBytes + volume.LengthBytes) / sectorBytes - 1));
            Put64(entry, 48, unchecked((ulong)volume.Attributes));
            Names.GetBytes(volume.EffectivePartitionName, entry[56..]);
        }
        var entriesCrc = Crc(entries);
        var diskId = Guid.NewGuid();
        byte[] Header(long location, long alternate, long arrayLocation)
        {
            var header = new byte[sectorBytes];
            "EFI PART"u8.CopyTo(header);
            Put32(header, 8, 0x10000); Put32(header, 12, 92);
            Put64(header, 24, (ulong)location); Put64(header, 32, (ulong)alternate);
            Put64(header, 40, (ulong)(2 + arraySectors)); Put64(header, 48, (ulong)(sectors - arraySectors - 2));
            diskId.TryWriteBytes(header.AsSpan(56)); Put64(header, 72, (ulong)arrayLocation);
            Put32(header, 80, 128); Put32(header, 84, 128); Put32(header, 88, entriesCrc);
            Put32(header, 16, Crc(header.AsSpan(0, 92)));
            return header;
        }
        var mbr = new byte[sectorBytes];
        mbr[447] = 0; mbr[448] = 2; mbr[449] = 0; mbr[450] = 0xee;
        mbr.AsSpan(451, 3).Fill(0xff);
        Put32(mbr, 454, 1); Put32(mbr, 458, (uint)Math.Min(sectors - 1, uint.MaxValue));
        mbr[510] = 0x55; mbr[511] = 0xaa;
        stream.Position = 0; stream.Write(mbr);
        stream.Write(Header(1, sectors - 1, 2)); stream.Write(entries);
        stream.Position = (sectors - arraySectors - 1) * sectorBytes;
        stream.Write(entries); stream.Write(Header(sectors - 1, 1, sectors - arraySectors - 1));
        stream.Flush();
    }

    private static uint Crc(ReadOnlySpan<byte> data)
    {
        var crc = uint.MaxValue;
        foreach (var value in data)
        {
            crc ^= value;
            for (var bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320U : 0);
        }
        return ~crc;
    }
    private static void Put32(Span<byte> bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes[offset..], value);
    private static void Put64(Span<byte> bytes, int offset, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(bytes[offset..], value);
}
