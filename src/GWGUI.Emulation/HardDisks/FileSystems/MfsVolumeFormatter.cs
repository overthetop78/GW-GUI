using System.Buffers.Binary;
using System.Text;

namespace GWGUI.Emulation.HardDisks.FileSystems;

/// <summary>Creates a non-bootable MFS volume with a two-sector MDB and a fixed directory.</summary>
public static class MfsVolumeFormatter
{
    public static void Validate(long capacity, string label)
    {
        if (capacity < 128L << 10 || capacity > 64L << 20 || capacity % 512 != 0)
            throw new ArgumentOutOfRangeException(nameof(capacity));
        _=ClassicMacLabel.Encode(label);
    }

    public static void Format(Stream volume, string label)
    {
        Validate(volume.Length, label);
        var availableSectors = volume.Length / 512 - 18;
        var allocationSectors = 1;
        while (availableSectors / allocationSectors > 640) allocationSectors *= 2;
        var allocationCount = checked((ushort)(availableSectors / allocationSectors));
        var mdb = new byte[1024];
        U16(mdb, 0, 0xd2d7);
        U16(mdb, 14, 4); U16(mdb, 16, 12);
        U16(mdb, 18, allocationCount);
        U32(mdb, 20, (uint)allocationSectors * 512); U32(mdb, 24, (uint)allocationSectors * 512);
        U16(mdb, 28, 16); U32(mdb, 30, 1); U16(mdb, 34, allocationCount);
        var name=ClassicMacLabel.Encode(label); mdb[36] = (byte)name.Length; name.CopyTo(mdb,37);
        // The empty 12-bit allocation map contains only free entries. The directory lies outside it.
        volume.Position = 0; volume.Write(new byte[16 * 512]);
        volume.Position = 1024; volume.Write(mdb);
        volume.Position = volume.Length - 1024; volume.Write(mdb);
        volume.Flush();
    }
    private static void U16(byte[] data, int offset, ushort value) => BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(offset), value);
    private static void U32(byte[] data, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(offset), value);
}
