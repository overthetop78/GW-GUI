using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.FileSystems;

public static class AtariFat16VolumeFormatter
{
    public static void Format(Stream volume)
    {
        if (volume.Length < 8L * 1024 * 1024 || volume.Length > 1024L * 1024 * 1024)
            throw new ArgumentOutOfRangeException(nameof(volume));
        var sectorBytes = 512;
        while (volume.Length / sectorBytes > 65535) sectorBytes *= 2;
        var sectors = checked((int)(volume.Length / sectorBytes));
        var rootSectors = (512 * 32 + sectorBytes - 1) / sectorBytes;
        var fatSectors = 1;
        while (((sectors - 1 - rootSectors - 2 * fatSectors) / 2 + 2) * 2 > fatSectors * sectorBytes) fatSectors++;
        var boot = new byte[sectorBytes];
        boot[0] = 0x60; boot[1] = 0x1c;
        "GWGUI "u8.CopyTo(boot.AsSpan(2));
        Set16(boot, 11, sectorBytes); boot[13] = 2; Set16(boot, 14, 1); boot[16] = 2;
        Set16(boot, 17, 512); Set16(boot, 19, sectors); boot[21] = 0xf8;
        Set16(boot, 22, fatSectors); Set16(boot, 24, 32); Set16(boot, 26, 1);
        volume.Position = 0; volume.Write(boot);
        var fat = new byte[fatSectors * sectorBytes]; Set16(fat, 0, 0xfff8); Set16(fat, 2, 0xffff);
        volume.Write(fat); volume.Write(fat); volume.Write(new byte[rootSectors * sectorBytes]); volume.Flush();
    }
    private static void Set16(byte[] data, int offset, int value) => BinaryPrimitives.WriteUInt16LittleEndian(data.AsSpan(offset), checked((ushort)value));
}
