using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.Containers;

/// <summary>NHD R0 with a 512-byte header and explicit CHS geometry.</summary>
public static class NhdImageWriter
{
    public static void Validate(long capacity, int heads = 1, int sectors = 1, int sectorBytes = 512, string comment = "")
    {
        LinearHeaderImageWriter.Cylinders(capacity,heads,sectors,sectorBytes);
        if (capacity > 1L << 40) throw new ArgumentOutOfRangeException(nameof(capacity));
        if (comment.Length > 255 || comment.Any(c=>c < 32 || c > 126))
            throw new ArgumentException("The NHD comment must be printable ASCII and at most 255 characters.");
    }
    public static void Write(Stream destination,long capacity,Action<Stream>? initialize = null,
        int heads = 1,int sectors = 1,int sectorBytes = 512,string comment = "")
    {
        Validate(capacity,heads,sectors,sectorBytes,comment);
        var header=new byte[512]; "T98HDDIMAGE.R0"u8.CopyTo(header);
        Encoding.ASCII.GetBytes(comment).CopyTo(header,16);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(272),512);
        BinaryPrimitives.WriteUInt32LittleEndian(header.AsSpan(276),LinearHeaderImageWriter.Cylinders(capacity,heads,sectors,sectorBytes));
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(280),(ushort)heads);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(282),(ushort)sectors);
        BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(284),(ushort)sectorBytes);
        LinearHeaderImageWriter.Write(destination,capacity,header,initialize);
    }
}
