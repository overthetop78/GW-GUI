using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.Encoding;

/// <summary>Associe les formats sectoriels catalogués à leur cadence physique nominale.</summary>
internal static class SectorImageTrackTimingCatalog
{
    public static uint BitCellTicks(string formatId)
    {
        if (formatId.Equals(DiskImageFormatIds.AmigaDosHighDensity, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Ibm2880, StringComparison.OrdinalIgnoreCase)) return TrackEncodingTimings.ExtraDensityMfmBitCellTicks;
        if (formatId.StartsWith(DiskImageFormatIds.AmigaPrefix, StringComparison.OrdinalIgnoreCase)) return TrackEncodingTimings.HighDensityMfmBitCellTicks;
        if (formatId.Equals(DiskImageFormatIds.Atari90, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AcornDfsPrefix, StringComparison.OrdinalIgnoreCase)) return TrackEncodingTimings.SingleDensityFmBitCellTicks;
        if (formatId.Equals(DiskImageFormatIds.AtariSt1440, StringComparison.OrdinalIgnoreCase) || IsHighDensityIbm(formatId)) return TrackEncodingTimings.HighDensityMfmBitCellTicks;
        if (IsDoubleDensityFamily(formatId)) return TrackEncodingTimings.DoubleDensityMfmBitCellTicks;
        return TrackEncodingDefaults.BitCellTicks;
    }

    public static uint IndexTimeTicks(string formatId) => formatId.Equals(DiskImageFormatIds.Ibm1200, StringComparison.OrdinalIgnoreCase) ? TrackEncodingTimings.Rpm360IndexTimeTicks : TrackEncodingTimings.Rpm300IndexTimeTicks;

    private static bool IsHighDensityIbm(string formatId) => formatId.Equals(DiskImageFormatIds.Ibm1200, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Ibm1440, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.Ibm1680, StringComparison.OrdinalIgnoreCase) || formatId.Equals(DiskImageFormatIds.IbmDmf, StringComparison.OrdinalIgnoreCase);

    private static bool IsDoubleDensityFamily(string formatId) => formatId.StartsWith(DiskImageFormatIds.AtariPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AtariStPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.IbmPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.MsxPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AcornAdfsPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.AmstradPrefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.EpsonQx10Prefix, StringComparison.OrdinalIgnoreCase) || formatId.StartsWith(DiskImageFormatIds.UcsdPrefix, StringComparison.OrdinalIgnoreCase);
}
