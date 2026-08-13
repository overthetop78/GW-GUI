using GWGUI.MediaEngine.Geometries.Apple;

namespace GWGUI.MediaEngine.Encoding.Apple;

/// <summary>Définit les cadences physiques des pistes Apple GCR et de leurs zones de vitesse.</summary>
internal static class AppleTrackEncodingTimings
{
    public const uint AppleIIBitCellTicks = 160;
    public const uint IwmGcrBitCellTicks = 80;
    public const uint MacintoshZone1IndexTimeTicks = 6_091_371;
    public const uint MacintoshZone2IndexTimeTicks = 5_594_406;
    public const uint MacintoshZone3IndexTimeTicks = 5_084_746;
    public const uint MacintoshZone4IndexTimeTicks = 4_571_429;
    public const uint MacintoshZone5IndexTimeTicks = 4_067_797;
    private const uint LisaIndexTicksPerSector = 500_000;

    public static uint MacintoshIndexTimeTicks(int cylinder) => cylinder switch
    {
        < MacintoshGcrGeometry.Zone1End => MacintoshZone1IndexTimeTicks,
        < MacintoshGcrGeometry.Zone2End => MacintoshZone2IndexTimeTicks,
        < MacintoshGcrGeometry.Zone3End => MacintoshZone3IndexTimeTicks,
        < MacintoshGcrGeometry.Zone4End => MacintoshZone4IndexTimeTicks,
        _ => MacintoshZone5IndexTimeTicks
    };

    public static uint LisaIndexTimeTicks(int cylinder) => checked((uint)LisaFileWareGeometry.Sectors(cylinder) * LisaIndexTicksPerSector);
}
