using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>Creates an empty Pascal volume with 512-byte blocks and a four-block directory.</summary>
public static class PascalVolumeFormatter
{
    public static void Validate(long capacity, string label)
    {
        if (capacity < 6 * 512 || capacity > 65535L * 512 || capacity % 512 != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        ArgumentNullException.ThrowIfNull(label);
        if (label.Length is < 1 or > 7 || label.Any(c => c < '!' || c > '~' ||
            c is >= 'a' and <= 'z' || "$=?,[#:".Contains(c)))
            throw new ArgumentException("Pascal labels require 1–7 uppercase printable ASCII characters without reserved punctuation.", nameof(label));
    }

    public static void Format(Stream volume, string label)
    {
        Validate(volume.Length, label);
        // Boot blocks are reserved but no operating system or boot code is installed.
        // Zero date denotes an unspecified date; allocation is described by directory entries.
        var metadata = new byte[6 * 512];
        var header = metadata.AsSpan(2 * 512, 26);
        BinaryPrimitives.WriteUInt16LittleEndian(header[2..], 6);
        header[6] = (byte)label.Length;
        Encoding.ASCII.GetBytes(label, header[7..]);
        BinaryPrimitives.WriteUInt16LittleEndian(header[14..], checked((ushort)(volume.Length / 512)));
        volume.Position = 0;
        volume.Write(metadata);
        volume.Flush();
    }
}
