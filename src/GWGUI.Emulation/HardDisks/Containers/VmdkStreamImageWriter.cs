using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Autonomous streamOptimized VMDK, 64 KiB zlib grains, trailing tables and footer.</summary>
public static class VmdkStreamImageWriter
{
    public const long MaximumCapacity = 1L << 40;
    private const int GrainBytes = 65536;
    private const int EntriesPerTable = 512;
    private const int DataStartSector = 128;

    public static void Validate(long capacity)
    {
        if (capacity < 512 || capacity > MaximumCapacity || capacity % 512 != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
    }

    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null)
    {
        ContainerValidation.Validate(destination, capacity); Validate(capacity);
        using var content = SparseImageContent.Create(capacity, initialize);
        var groups = new SortedDictionary<int, uint[]>();
        var tableCount = checked((int)((capacity + (long)GrainBytes * EntriesPerTable - 1) / ((long)GrainBytes * EntriesPerTable)));
        var directory = new byte[((tableCount * 4 + 511) / 512) * 512];
        var geometry = DiscUtils.Geometry.FromCapacity(capacity);
        var cid = Guid.NewGuid().ToString("N")[..8];
        var descriptor = Encoding.ASCII.GetBytes(FormattableString.Invariant(
            $"# Disk DescriptorFile\nversion=1\nCID={cid}\nparentCID=ffffffff\ncreateType=\"streamOptimized\"\nRW {capacity / 512} SPARSE \"disk.vmdk\"\nddb.adapterType = \"ide\"\nddb.geometry.cylinders = \"{geometry.Cylinders}\"\nddb.geometry.heads = \"{geometry.HeadsPerCylinder}\"\nddb.geometry.sectors = \"{geometry.SectorsPerTrack}\"\nddb.virtualHWVersion = \"4\"\n"));
        destination.SetLength(DataStartSector * 512);
        destination.Position = 0; destination.Write(Header(capacity, ulong.MaxValue));
        destination.Write(descriptor);
        destination.Position = DataStartSector * 512;
        var grain = new byte[GrainBytes];
        foreach (var unit in SparseImageContent.AllocatedUnits(content, GrainBytes))
        {
            Array.Clear(grain); content.Position = unit * GrainBytes;
            content.ReadExactly(grain.AsSpan(0, (int)Math.Min(GrainBytes, capacity - content.Position)));
            if (!grain.AsSpan().ContainsAnyExcept((byte)0)) continue;
            using var compressed = new MemoryStream();
            using (var zlib = new ZLibStream(compressed, CompressionLevel.Fastest, leaveOpen: true)) zlib.Write(grain);
            var group = checked((int)(unit / EntriesPerTable));
            if (!groups.TryGetValue(group, out var table)) groups.Add(group, table = new uint[EntriesPerTable]);
            table[unit % EntriesPerTable] = checked((uint)(destination.Position / 512));
            var marker = new byte[12]; Put64(marker, 0, (ulong)(unit * (GrainBytes / 512))); Put32(marker, 8, (uint)compressed.Length);
            destination.Write(marker); compressed.Position = 0; compressed.CopyTo(destination); Pad(destination);
        }
        foreach (var (group, table) in groups)
        {
            destination.Write(Marker(4, 1));
            Put32(directory, group * 4, checked((uint)(destination.Position / 512)));
            var bytes = new byte[EntriesPerTable * 4];
            for (var index = 0; index < table.Length; index++) Put32(bytes, index * 4, table[index]);
            destination.Write(bytes);
        }
        destination.Write(Marker((ulong)(directory.Length / 512), 2));
        var directorySector = (ulong)(destination.Position / 512); destination.Write(directory);
        destination.Write(Marker(1, 3)); destination.Write(Header(capacity, directorySector));
        destination.Write(Marker(0, 0)); destination.Flush();
    }

    private static byte[] Header(long capacity, ulong directorySector)
    {
        var header = new byte[512]; "KDMV"u8.CopyTo(header);
        Put32(header, 4, 3); Put32(header, 8, 0x30001);
        Put64(header, 12, (ulong)(capacity / 512)); Put64(header, 20, GrainBytes / 512);
        Put64(header, 28, 1); Put64(header, 36, 20); Put32(header, 44, EntriesPerTable);
        Put64(header, 56, directorySector); Put64(header, 64, DataStartSector);
        header[73] = 10; header[74] = 32; header[75] = 13; header[76] = 10;
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(77), 1);
        return header;
    }
    private static byte[] Marker(ulong sectors, uint type)
    {
        var marker = new byte[512]; Put64(marker, 0, sectors); Put32(marker, 12, type); return marker;
    }
    private static void Pad(Stream stream)
    {
        var padding = (int)((512 - stream.Position % 512) % 512);
        if (padding != 0) stream.Write(new byte[padding]);
    }
    private static void Put32(Span<byte> bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(bytes[offset..], value);
    private static void Put64(Span<byte> bytes, int offset, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(bytes[offset..], value);
}
