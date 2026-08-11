namespace GWGUI.MediaEngine.Definitions;

public static partial class DiskImageFormatIds
{
    public const string I86fPrefix = "86f.";

    public static string I86fFromGeometry(int sectorSize, int cylinders, int heads, int sectorsPerTrack) => $"{I86fPrefix}{sectorSize}.{cylinders}.{heads}.{sectorsPerTrack}";
}
