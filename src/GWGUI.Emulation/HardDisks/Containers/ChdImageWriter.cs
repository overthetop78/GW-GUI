using System.Buffers.Binary;
using System.Globalization;
using System.Text;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Writable standalone CHD v5, uncompressed hunks and map, 512-byte units.</summary>
public static class ChdImageWriter
{
    public const long MaximumCapacity = (long)int.MaxValue * 512;
    private const int Hunk = 65536;
    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null, int heads = 1, int sectors = 1)
    {
        ContainerValidation.Validate(destination, capacity);
        if (capacity > MaximumCapacity || heads < 1 || sectors < 1 || capacity / 512 % ((long)heads * sectors) != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "CHD capacity must fit the supplied geometry exactly.");
        var cylinders = capacity / 512 / heads / sectors;
        if (cylinders < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
        using var content = SparseImageContent.Create(capacity, initialize);
        var units = SparseImageContent.AllocatedUnits(content, Hunk);
        var count = checked((int)((capacity + Hunk - 1) / Hunk));
        var map = new byte[checked(count * 4)];
        var metadata = Encoding.ASCII.GetBytes(string.Create(CultureInfo.InvariantCulture,
            $"CYLS:{cylinders},HEADS:{heads},SECS:{sectors},BPS:512\0"));
        var metadataOffset = 124L + map.Length;
        var firstData = (metadataOffset + 16 + metadata.Length + Hunk - 1) / Hunk;
        destination.SetLength((firstData + units.Count) * Hunk);
        var header = new byte[124]; "MComprHD"u8.CopyTo(header);
        U32(header, 8, 124); U32(header, 12, 5); U64(header, 32, (ulong)capacity);
        U64(header, 40, 124); U64(header, 48, (ulong)metadataOffset); U32(header, 56, Hunk); U32(header, 60, 512);
        // Writable uncompressed CHDs carry no immutable SHA-1 identity or parent hash.
        destination.Position = 0; destination.Write(header);
        var metadataHeader = new byte[16]; "GDDD"u8.CopyTo(metadataHeader); U32(metadataHeader, 4, (uint)metadata.Length);
        destination.Position = metadataOffset; destination.Write(metadataHeader); destination.Write(metadata);
        var buffer = new byte[Hunk]; var next = firstData;
        foreach (var unit in units)
        {
            U32(map, checked((int)unit * 4), checked((uint)next));
            SparseImageContent.CopyUnit(content, destination, unit * Hunk, next++ * Hunk, buffer);
        }
        destination.Position = 124; destination.Write(map); destination.Flush();
    }
    private static void U32(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
    private static void U64(byte[] bytes, int offset, ulong value) => BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(offset), value);
}
