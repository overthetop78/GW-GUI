using GWGUI.MediaEngine.Constants;
using GWGUI.MediaEngine.Enums;
using GWGUI.MediaEngine.Images.Formats;
using System.IO;

namespace GWGUI.MediaEngine.Images.Visualization;

public static class MediaIconSelector
{
    public static string Select(MediaKind? mediaKind, string path, DiskFormat? format = null)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        return mediaKind switch
        {
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.ThreeInch => MediaIconIds.FloppyThreeInch,
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.ThreeAndHalfInch &&
                                  format.Density == FloppyDensity.ExtendedDensity => MediaIconIds.FloppyThreeAndHalfExtended,
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.ThreeAndHalfInch &&
                                  format.Density == FloppyDensity.HighDensity => MediaIconIds.FloppyThreeAndHalfHigh,
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.ThreeAndHalfInch => MediaIconIds.FloppyThreeAndHalfDouble,
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.FiveAndQuarterInch => MediaIconIds.FloppyFiveAndQuarter,
            MediaKind.Floppy when format?.FormFactor == FloppyFormFactor.EightInch => MediaIconIds.FloppyEightInch,
            MediaKind.Floppy => MediaIconIds.Floppy,
            MediaKind.HardDisk => MediaIconIds.HardDisk,
            MediaKind.Optical => MediaIconIds.Optical,
            MediaKind.Tape when extension is MediaIconExtensions.Cas or MediaIconExtensions.Cdt or MediaIconExtensions.Tzx or MediaIconExtensions.Wav => MediaIconIds.Cassette,
            MediaKind.Tape => MediaIconIds.Tape,
            _ when extension is MediaIconExtensions.Crt or MediaIconExtensions.Car or MediaIconExtensions.Rom or MediaIconExtensions.A26 or MediaIconExtensions.A52 or MediaIconExtensions.A78 or MediaIconExtensions.Nes => MediaIconIds.Cartridge,
            _ when extension is MediaIconExtensions.Iso or MediaIconExtensions.Cue or MediaIconExtensions.Ccd or MediaIconExtensions.Mds => MediaIconIds.Optical,
            _ => MediaIconIds.File
        };
    }
}
