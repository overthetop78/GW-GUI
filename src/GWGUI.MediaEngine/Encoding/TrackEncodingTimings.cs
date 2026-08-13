namespace GWGUI.MediaEngine.Encoding;

/// <summary>Définit les cadences physiques standard utilisées par les profils sectoriels.</summary>
internal static class TrackEncodingTimings
{
    public const uint HighDensityMfmBitCellTicks = 40;
    public const uint DoubleDensityMfmBitCellTicks = 80;
    public const uint ExtraDensityMfmBitCellTicks = 20;
    public const uint SingleDensityFmBitCellTicks = 160;
    public const uint Rpm300IndexTimeTicks = 8_000_000;
    public const uint Rpm360IndexTimeTicks = 6_666_667;
}
