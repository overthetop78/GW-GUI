using System.Buffers.Binary;
using System.Text;
using DiscUtils.Vhd;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>Serializes initialized VHD differencing metadata without the library's small-BAT allocation bug.</summary>
internal static class VhdDifferencingHeaderWriter
{
    internal static void Write(Stream stream, DiskImageFile parent, string absolutePath, string relativePath, DateTime modifiedUtc)
    {
        const int blockSize = 2 << 20;
        var epoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var seconds = (modifiedUtc - epoch).TotalSeconds;
        var name = Path.GetFileName(absolutePath);
        if (absolutePath.Length > 255 || relativePath.Length > 255 || name.Length > 255 || seconds < 0 || seconds > uint.MaxValue)
            throw new ArgumentException("VHD parent locators and timestamp exceed their fields.");
        var entries = checked((uint)((parent.Capacity + blockSize - 1) / blockSize));
        var tableBytes = checked((int)((entries * 4L + 511) / 512 * 512));
        var footer = new byte[512]; "conectix"u8.CopyTo(footer);
        U32(footer, 8, 2); U32(footer, 12, 0x10000); U64(footer, 16, 512);
        U32(footer, 24, checked((uint)(DateTime.UtcNow - epoch).TotalSeconds));
        "GWGU"u8.CopyTo(footer.AsSpan(28)); U32(footer, 32, 0x10000); "Wi2k"u8.CopyTo(footer.AsSpan(36));
        U64(footer, 40, (ulong)parent.Capacity); U64(footer, 48, (ulong)parent.Capacity);
        var geometry = parent.Geometry;
        BinaryPrimitives.WriteUInt16BigEndian(footer.AsSpan(56), checked((ushort)geometry.Cylinders));
        footer[58] = checked((byte)geometry.HeadsPerCylinder); footer[59] = checked((byte)geometry.SectorsPerTrack);
        U32(footer, 60, 4); Guid.NewGuid().TryWriteBytes(footer.AsSpan(68, 16), bigEndian: true, out _);
        U32(footer, 64, Checksum(footer));
        var header = new byte[1024]; "cxsparse"u8.CopyTo(header);
        U64(header, 8, ulong.MaxValue); U64(header, 16, 1536); U32(header, 24, 0x10000);
        U32(header, 28, entries); U32(header, 32, blockSize);
        parent.UniqueId.TryWriteBytes(header.AsSpan(40, 16), bigEndian: true, out _); U32(header, 56, (uint)seconds);
        Encoding.BigEndianUnicode.GetBytes(name).CopyTo(header, 64);
        void Locator(int slot, string code, string path, long offset)
        {
            var at = 576 + slot * 24; Encoding.ASCII.GetBytes(code).CopyTo(header, at);
            U32(header, at + 4, 512); U32(header, at + 8, (uint)(path.Length * 2)); U64(header, at + 16, (ulong)offset);
        }
        Locator(7, "W2ku", absolutePath, 1536L + tableBytes); Locator(6, "W2ru", relativePath, 2048L + tableBytes);
        U32(header, 36, Checksum(header));
        var bat = new byte[tableBytes]; Array.Fill(bat, (byte)0xff);
        var absolute = new byte[512]; Encoding.Unicode.GetBytes(absolutePath).CopyTo(absolute, 0);
        var relative = new byte[512]; Encoding.Unicode.GetBytes(relativePath).CopyTo(relative, 0);
        stream.Position = 0; stream.Write(footer); stream.Write(header); stream.Write(bat);
        stream.Write(absolute); stream.Write(relative); stream.Write(footer);
    }

    private static uint Checksum(byte[] bytes) => ~bytes.Aggregate(0u, (sum, value) => sum + value);
    private static void U32(byte[] bytes, int at, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(at), value);
    private static void U64(byte[] bytes, int at, ulong value) => BinaryPrimitives.WriteUInt64BigEndian(bytes.AsSpan(at), value);
}
