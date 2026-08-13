using GWGUI.MediaEngine.Geometries.Commodore;

namespace GWGUI.MediaEngine.Encoding.Commodore;

/// <summary>Définit les cadences physiques des pistes GCR Commodore zonées.</summary>
internal static class CommodoreTrackEncodingTimings
{
    public const uint Commodore1541Zone1BitCellTicks = 130;
    public const uint Commodore1541Zone2BitCellTicks = 140;
    public const uint Commodore1541Zone3BitCellTicks = 150;
    public const uint Commodore1541Zone4BitCellTicks = 160;
    public const uint Commodore900Zone1BitCellTicks = 86;
    public const uint Commodore900Zone2BitCellTicks = 93;
    public const uint Commodore900Zone3BitCellTicks = 100;
    public const uint Commodore900Zone4BitCellTicks = 106;

    public static uint Commodore1541BitCellTicks(int cylinder)
    {
        var track = cylinder + Commodore1541Geometry.FirstTrack;
        return track switch
        {
            <= Commodore1541Geometry.Zone1EndTrack => Commodore1541Zone1BitCellTicks,
            <= Commodore1541Geometry.Zone2EndTrack => Commodore1541Zone2BitCellTicks,
            <= Commodore1541Geometry.Zone3EndTrack => Commodore1541Zone3BitCellTicks,
            _ => Commodore1541Zone4BitCellTicks
        };
    }

    public static uint Commodore900BitCellTicks(int cylinder) => cylinder switch
    {
        < Commodore900Geometry.Zone2StartCylinder => Commodore900Zone1BitCellTicks,
        < Commodore900Geometry.Zone3StartCylinder => Commodore900Zone2BitCellTicks,
        < Commodore900Geometry.Zone4StartCylinder => Commodore900Zone3BitCellTicks,
        _ => Commodore900Zone4BitCellTicks
    };
}
