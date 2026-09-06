using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

public static class ProDosVolumeFormatter
{
    public static void Validate(long capacity, string label)
    {
        if (capacity < 16 * 512 || capacity > 65535L * 512 || capacity % 512 != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        if (label.Length is < 1 or > 15 || label[0] is < 'A' or > 'Z' ||
            label.Any(c => !(c is >= 'A' and <= 'Z' or >= '0' and <= '9' or '.')))
            throw new ArgumentException("ProDOS labels require 1–15 uppercase letters, digits or dots and an initial letter.", nameof(label));
    }
    public static void Format(Stream volume, string label)
    {
        Validate(volume.Length, label);
        var blocks = checked((int)(volume.Length / 512));
        var bitmapBlocks = (blocks + 4095) / 4096;
        var metadata = new byte[(6 + bitmapBlocks) * 512];
        for (var block = 2; block <= 5; block++)
        {
            Set16(metadata, block * 512, block == 2 ? 0 : block - 1);
            Set16(metadata, block * 512 + 2, block == 5 ? 0 : block + 1);
        }
        const int header = 2 * 512 + 4;
        metadata[header] = (byte)(0xf0 | label.Length); Encoding.ASCII.GetBytes(label).CopyTo(metadata, header + 1);
        metadata[header + 0x1e] = 0xc3; metadata[header + 0x1f] = 39; metadata[header + 0x20] = 13;
        Set16(metadata, header + 0x23, 6); Set16(metadata, header + 0x25, blocks);
        // Bitmap bits run from MSB to LSB. Reserved and nonexistent blocks remain allocated.
        for (var block = 6 + bitmapBlocks; block < blocks; block++)
            metadata[6 * 512 + block / 8] |= (byte)(0x80 >> (block % 8));
        volume.Position = 0; volume.Write(metadata); volume.Flush();
    }
    private static void Set16(byte[] bytes, int offset, int value) => BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(offset), checked((ushort)value));
}
