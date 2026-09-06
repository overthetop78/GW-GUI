using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>HDI with a 4096-byte header and explicit CHS geometry.</summary>
public static class HdiImageWriter
{
    public static void Validate(long capacity, int heads = 1, int sectors = 1, int sectorBytes = 512)
    {
        LinearHeaderImageWriter.Cylinders(capacity, heads, sectors, sectorBytes);
        if (capacity > uint.MaxValue) throw new ArgumentOutOfRangeException(nameof(capacity));
    }
    public static void Write(Stream destination, long capacity, Action<Stream>? initialize = null,
        int heads = 1, int sectors = 1, int sectorBytes = 512)
    {
        Validate(capacity, heads, sectors, sectorBytes);
        var header = new byte[4096];
        U32(header,8,4096); U32(header,12,(uint)capacity); U32(header,16,(uint)sectorBytes);
        U32(header,20,(uint)sectors); U32(header,24,(uint)heads);
        U32(header,28,LinearHeaderImageWriter.Cylinders(capacity,heads,sectors,sectorBytes));
        LinearHeaderImageWriter.Write(destination,capacity,header,initialize);
    }
    private static void U32(byte[] bytes,int offset,uint value)=>BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(offset),value);
}
