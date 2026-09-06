using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>THD with 8 heads, 33 sectors per track, 256-byte sectors and a 256-byte header.</summary>
public static class ThdImageWriter
{
    public const int CylinderBytes = 8 * 33 * 256;
    public static void Validate(long capacity)
    {
        if (capacity < CylinderBytes || capacity % CylinderBytes != 0 || capacity / CylinderBytes > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(capacity));
    }
    public static void Write(Stream destination,long capacity,Action<Stream>? initialize = null)
    {
        Validate(capacity);
        var header=new byte[256];
        BinaryPrimitives.WriteUInt16LittleEndian(header,(ushort)(capacity/CylinderBytes));
        LinearHeaderImageWriter.Write(destination,capacity,header,initialize);
    }
}
