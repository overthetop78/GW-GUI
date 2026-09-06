using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

public static class AmigaDosVolumeFormatter
{
    public static void Format(Stream volume, string label, bool fastFileSystem)
        => Format(volume,label,fastFileSystem?(byte)1:(byte)0);

    public static void Validate(long capacity,string label,byte variant)
    {
        if (variant > 5) throw new ArgumentOutOfRangeException(nameof(variant));
        if (capacity < 1024 * 1024 || capacity >= 2L * 1024 * 1024 * 1024 || capacity % 512 != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        if (label.Length is < 1 or > 30 || label.Any(c => c < 32 || c > 255 || c is ':' or '/'))
            throw new ArgumentException("Amiga volume names require 1–30 Latin-1 characters, without ':' or '/'.", nameof(label));
    }

    public static void Format(Stream volume,string label,byte variant)
    {
        Validate(volume.Length,label,variant);
        var blocks = checked((int)(volume.Length / 512));
        var rootIndex = blocks / 2;
        var bitmapCount = (blocks - 2 + 4063) / 4064;
        var extensionCount = (Math.Max(0, bitmapCount - 25) + 126) / 127;
        var cacheIndex = rootIndex + 1 + bitmapCount + extensionCount;
        var reserved = Enumerable.Range(rootIndex, 1 + bitmapCount + extensionCount + (variant >= 4 ? 1 : 0)).ToHashSet();
        var boot = new byte[1024]; "DOS"u8.CopyTo(boot); boot[3] = variant;
        Set(boot, 8, (uint)rootIndex);
        // Valid non-executable boot block: filesystem mounting does not require boot code.
        uint bootSum = 0;
        for (var i = 0; i < boot.Length; i += 4)
        { var value = BinaryPrimitives.ReadUInt32BigEndian(boot.AsSpan(i)); var next = unchecked(bootSum + value); if (next < bootSum) next++; bootSum = next; }
        Set(boot, 4, ~bootSum); volume.Position = 0; volume.Write(boot);
        var root = new byte[512]; Set(root, 0, 2); Set(root, 12, 72); Set(root, 312, uint.MaxValue); Set(root, 508, 1);
        root[432] = (byte)label.Length; Encoding.Latin1.GetBytes(label).CopyTo(root, 433);
        for (var i = 0; i < Math.Min(25, bitmapCount); i++) Set(root, 316 + i * 4, (uint)(rootIndex + 1 + i));
        if (extensionCount > 0) Set(root, 416, (uint)(rootIndex + 1 + bitmapCount));
        if (variant >= 4) Set(root, 504, (uint)cacheIndex);
        Checksum(root, 20); Put(volume, rootIndex, root);
        if (variant >= 4)
        {
            var cache=new byte[512]; Set(cache,0,33); Set(cache,4,(uint)cacheIndex); Set(cache,8,(uint)rootIndex);
            Checksum(cache,20); Put(volume,cacheIndex,cache);
        }
        for (var i = 0; i < bitmapCount; i++)
        {
            var bitmap = new byte[512];
            for (var bit = 0; bit < 4064; bit++)
            {
                var block = 2 + i * 4064 + bit;
                if (block >= blocks || reserved.Contains(block)) continue;
                var offset = 4 + (bit / 32) * 4;
                var value = BinaryPrimitives.ReadUInt32BigEndian(bitmap.AsSpan(offset));
                Set(bitmap, offset, value | (1u << (bit % 32)));
            }
            Checksum(bitmap, 0); Put(volume, rootIndex + 1 + i, bitmap);
        }
        for (var i = 0; i < extensionCount; i++)
        {
            var extension = new byte[512];
            for (var j = 0; j < 127 && 25 + i * 127 + j < bitmapCount; j++)
                Set(extension, j * 4, (uint)(rootIndex + 1 + 25 + i * 127 + j));
            if (i + 1 < extensionCount) Set(extension, 508, (uint)(rootIndex + 1 + bitmapCount + i + 1));
            Put(volume, rootIndex + 1 + bitmapCount + i, extension);
        }
        volume.Flush();
    }
    private static void Set(byte[] bytes, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(bytes.AsSpan(offset), value);
    private static void Put(Stream volume, int block, byte[] bytes) { volume.Position = (long)block * 512; volume.Write(bytes); }
    private static void Checksum(byte[] bytes, int offset)
    {
        uint sum = 0;
        for (var i = 0; i < bytes.Length; i += 4) sum = unchecked(sum + BinaryPrimitives.ReadUInt32BigEndian(bytes.AsSpan(i)));
        Set(bytes, offset, unchecked(0u - sum));
    }
}
