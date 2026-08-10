using GWGUI.Scp.SectorImages;

namespace GWGUI.Scp.Images;

internal static class AppleDiskGeometry
{
    public const int LisaFileWareBlockCount = 1702;
    public const int LisaFileWareCylinderCount = 46;
    public const int LisaFileWareHeadCount = 2;
    public const int LisaFileWareMaximumSectorsPerTrack = 22;
    public const int Macintosh400KBlockCount = 800;
    public const int MacintoshCylinderCount = 80;
    public const int Macintosh400KHeadCount = 1;
    public const int MacintoshMaximumSectorsPerTrack = 12;
    public const int GenericTaggedImageHeadCount = 1;
    public const int GenericTaggedImageSectorsPerTrack = 10;
    public const int MinimumCylinderCount = 1;

    public static readonly int[] ProDosToPhysical = [0, 2, 4, 6, 8, 10, 12, 14, 1, 3, 5, 7, 9, 11, 13, 15];
    public static readonly int[] PhysicalToDos = [0, 7, 14, 6, 13, 5, 12, 4, 11, 3, 10, 2, 9, 1, 8, 15];

    public static SectorAddress LisaFileWareAddress(int logicalBlock)
    {
        const int sectorsPerSide = LisaFileWareBlockCount / LisaFileWareHeadCount;
        var head = logicalBlock / sectorsPerSide;
        var remaining = logicalBlock % sectorsPerSide;
        for (var cylinder = 0; cylinder < LisaFileWareCylinderCount; cylinder++)
        {
            var count = LisaFileWareSectors(cylinder);
            if (remaining < count) return new(cylinder, head, remaining);
            remaining -= count;
        }
        throw new InvalidDataException("The Lisa FileWare logical block is outside the physical geometry.");
    }

    public static int LisaFileWareSectors(int cylinder) => cylinder switch
    {
        < 4 => LisaFileWareMaximumSectorsPerTrack,
        < 11 => 21,
        < 17 => 20,
        < 23 => 19,
        < 29 => 18,
        < 35 => 17,
        < 42 => 16,
        < 46 => 15,
        _ => throw new ArgumentOutOfRangeException(nameof(cylinder))
    };

    public static SectorAddress AppleMacZonedAddress(int logicalBlock, int heads)
    {
        var remaining = logicalBlock;
        for (var cylinder = 0; cylinder < MacintoshCylinderCount; cylinder++)
        {
            var sectors = AppleMacSectors(cylinder);
            var perCylinder = sectors * heads;
            if (remaining < perCylinder) return new(cylinder, remaining / sectors, remaining % sectors);
            remaining -= perCylinder;
        }
        throw new InvalidDataException("The Apple GCR logical block is outside the physical geometry.");
    }

    public static int AppleMacSectors(int cylinder) => cylinder switch
    {
        < 16 => MacintoshMaximumSectorsPerTrack,
        < 32 => 11,
        < 48 => 10,
        < 64 => 9,
        < MacintoshCylinderCount => 8,
        _ => throw new ArgumentOutOfRangeException(nameof(cylinder))
    };

    public static byte[] ConvertDosOrderToProDosBlocks(ReadOnlySpan<byte> dosOrder)
    {
        if (dosOrder.Length % (16 * 256) != 0)
            throw new InvalidDataException("The Apple 5.25-inch image has an invalid length.");
        var output = new byte[dosOrder.Length];
        var tracks = dosOrder.Length / (16 * 256);
        for (var track = 0; track < tracks; track++)
        for (var logicalSector = 0; logicalSector < 16; logicalSector++)
        {
            var physicalSector = ProDosToPhysical[logicalSector];
            var dosFileSector = PhysicalToDos[physicalSector];
            dosOrder.Slice((track * 16 + dosFileSector) * 256, 256)
                .CopyTo(output.AsSpan((track * 16 + logicalSector) * 256, 256));
        }
        return output;
    }
}
