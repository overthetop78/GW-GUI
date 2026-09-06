using System.Buffers.Binary;

namespace GWGUI.Emulation.HardDisks.Partitioning;

/// <summary>Writes an LBA extended partition chain with one reserved sector per logical volume.</summary>
internal static class ExtendedMbrWriter
{
    internal static void Write(Stream stream, IReadOnlyList<DiskVolumePlan> volumes)
    {
        var firstEbr = volumes[0].OffsetBytes / 512 - 1;
        for (var index = 0; index < volumes.Count; index++)
        {
            var volume = volumes[index];
            var sector = new byte[512];
            // Data addresses are relative to this EBR; links are relative to the first EBR.
            Entry(sector.AsSpan(446, 16), PartitionTypeDefaults.Mbr(volume), 1, volume.LengthBytes / 512);
            if (index + 1 < volumes.Count)
            {
                var next = volumes[index + 1];
                var nextEbr = next.OffsetBytes / 512 - 1;
                Entry(sector.AsSpan(462, 16), 0x0f, nextEbr - firstEbr, next.LengthBytes / 512 + 1);
            }
            sector[510] = 0x55; sector[511] = 0xaa;
            stream.Position = volume.OffsetBytes - 512;
            stream.Write(sector);
        }
    }

    private static void Entry(Span<byte> entry, byte type, long relativeStart, long length)
    {
        // Saturated CHS fields: this profile uses the LBA addresses, not legacy CHS addressing.
        entry[1] = entry[5] = 0xfe;
        entry[2] = entry[3] = entry[6] = entry[7] = 0xff;
        entry[4] = type;
        BinaryPrimitives.WriteUInt32LittleEndian(entry[8..], checked((uint)relativeStart));
        BinaryPrimitives.WriteUInt32LittleEndian(entry[12..], checked((uint)length));
    }
}
