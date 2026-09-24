using GWGUI.MediaEngine.Constants;

namespace GWGUI.App.Constants.Controls.Visual;

internal readonly record struct MediaIconVisual(string Geometry, string Foreground, string Background, string? ImageFile);

internal static class MediaIconVisualConstants
{
    private const string FloppyGeometry = "M3,2 H17 L21,6 V22 H3 Z M6,3 V9 H16 V3 Z M6,14 H18 V20 H6 Z M9,15 V19 H15 V15 Z";
    private const string FiveAndQuarterFloppyGeometry = "F0 M3,2 H19 L21,4 V22 H3 Z M6,4 H16 V8 H6 Z M8,14 A4,4 0 1 0 16,14 A4,4 0 1 0 8,14 M11,14 A1,1 0 1 0 13,14 A1,1 0 1 0 11,14 M17,9 A1,1 0 1 0 19,9 A1,1 0 1 0 17,9";
    private const string HardDiskGeometry = "M3,4 H21 V20 H3 Z M5,6 H19 V14 H5 Z M6,17 A1,1 0 1 0 8,17 A1,1 0 1 0 6,17 M10,16 H18 V18 H10 Z";
    private const string OpticalGeometry = "M12,2 A10,10 0 1 0 12,22 A10,10 0 1 0 12,2 M12,9 A3,3 0 1 0 12,15 A3,3 0 1 0 12,9 M14,4 L13,8 A4,4 0 0 1 16,11 L20,10 A8,8 0 0 0 14,4 Z";
    private const string CassetteGeometry = "M2,5 H22 V19 H2 Z M5,8 H19 V13 H5 Z M7,15 A2,2 0 1 0 11,15 A2,2 0 1 0 7,15 M13,15 A2,2 0 1 0 17,15 A2,2 0 1 0 13,15 M7,18 L9,14 H15 L17,18 Z";
    private const string TapeGeometry = "M6,3 A5,5 0 1 0 6,13 A5,5 0 1 0 6,3 M18,3 A5,5 0 1 0 18,13 A5,5 0 1 0 18,3 M6,6 A2,2 0 1 0 6,10 A2,2 0 1 0 6,6 M18,6 A2,2 0 1 0 18,10 A2,2 0 1 0 18,6 M6,13 H18 V21 H6 Z";
    private const string CartridgeGeometry = "M5,2 H19 V16 L16,22 H8 L5,16 Z M8,5 H16 V12 H8 Z M9,17 H15 L14,20 H10 Z";
    private const string FileGeometry = "M4,2 H15 L20,7 V22 H4 Z M14,3 V8 H19";
    private const string FloppyForeground = "#FF24658A";
    private const string FloppyBackground = "#FFE4EDF5";
    private const string HardDiskForeground = "#FF77572B";
    private const string HardDiskBackground = "#FFF2E9DA";
    private const string OpticalForeground = "#FF6B4BB6";
    private const string OpticalBackground = "#FFEDE8F8";
    private const string CassetteForeground = "#FFF4F7F9";
    private const string CassetteBackground = "#FF344A57";
    private const string TapeForeground = "#FF9A3D68";
    private const string TapeBackground = "#FFF5E3EC";
    private const string CartridgeForeground = "#FF3F7C48";
    private const string CartridgeBackground = "#FFE3F1E5";
    private const string FloppyThreeInchImage = "floppy-3.png";
    private const string FloppyExtendedImage = "floppy-3.5-ed-black.png";
    private const string FloppyHighImage = "floppy-3.5-hd-black.png";
    private const string FloppyDoubleImage = "floppy-3.5-dd-blue.png";
    private const string FloppyFiveAndQuarterImage = "floppy-5.25.png";
    private const string FloppyEightInchImage = "floppy-8.png";
    private const string CassetteImage = "cassette-data.png";

    internal static MediaIconVisual For(string iconId) => iconId switch
    {
        MediaIconIds.FloppyThreeInch => new(FloppyGeometry, FloppyForeground, FloppyBackground, FloppyThreeInchImage),
        MediaIconIds.FloppyThreeAndHalfExtended => new(FloppyGeometry, FloppyForeground, FloppyBackground, FloppyExtendedImage),
        MediaIconIds.FloppyThreeAndHalfHigh => new(FloppyGeometry, FloppyForeground, FloppyBackground, FloppyHighImage),
        MediaIconIds.FloppyThreeAndHalfDouble => new(FloppyGeometry, FloppyForeground, FloppyBackground, FloppyDoubleImage),
        MediaIconIds.FloppyFiveAndQuarter => new(FiveAndQuarterFloppyGeometry, FloppyForeground, FloppyBackground, FloppyFiveAndQuarterImage),
        MediaIconIds.FloppyEightInch => new(FloppyGeometry, FloppyForeground, FloppyBackground, FloppyEightInchImage),
        MediaIconIds.Floppy => new(FloppyGeometry, FloppyForeground, FloppyBackground, null),
        MediaIconIds.HardDisk => new(HardDiskGeometry, HardDiskForeground, HardDiskBackground, null),
        MediaIconIds.Optical => new(OpticalGeometry, OpticalForeground, OpticalBackground, null),
        MediaIconIds.Cassette => new(CassetteGeometry, CassetteForeground, CassetteBackground, CassetteImage),
        MediaIconIds.Tape => new(TapeGeometry, TapeForeground, TapeBackground, null),
        MediaIconIds.Cartridge => new(CartridgeGeometry, CartridgeForeground, CartridgeBackground, null),
        _ => new(FileGeometry, FloppyForeground, FloppyBackground, null)
    };
}
