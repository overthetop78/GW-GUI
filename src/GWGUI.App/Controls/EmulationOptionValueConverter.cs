using System.Globalization;
using System.IO;
using GWGUI.App.Localization;
using GWGUI.Emulation.Amiga;

namespace GWGUI.App.Controls;

internal static class EmulationOptionValueConverter
{
    internal static int DeviceIndex(string identifier) =>
        int.TryParse(new string(identifier.Where(char.IsDigit).ToArray()), out var index) ? index : 0;

    internal static string FloppyModelName(string model) => model == "35hd"
        ? LocExtension.Get("Emulation.AmigaHdFloppy")
        : LocExtension.Get("Emulation.AmigaDdFloppy");

    internal static AmigaMediaKind InferMediaKind(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".hdf" or ".hdz" => AmigaMediaKind.HardDrive,
        ".cue" or ".ccd" or ".chd" or ".nrg" or ".mds" or ".iso" => AmigaMediaKind.CompactDisc,
        ".lha" or ".slave" or ".info" => AmigaMediaKind.WhdLoad,
        ".uae" => AmigaMediaKind.Configuration,
        _ => AmigaMediaKind.Floppy
    };

    internal static string MouseSpeedRatioText(string percentage)
    {
        var parsed = int.TryParse(percentage, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 100;
        return (Math.Clamp(parsed, 1, 1000) / 100d).ToString("0.00", CultureInfo.CurrentCulture);
    }

    internal static int MouseSpeedPercentage(string ratio)
    {
        var normalized = ratio.Trim().TrimEnd('×', 'x', 'X').Replace(',', '.');
        var parsed = double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 1d;
        return Math.Clamp((int)Math.Round(parsed * 100d), 1, 1000);
    }

    internal static int ParsePercentage(string value, int fallback) =>
        int.TryParse(value.Trim().TrimEnd('%'), out var parsed) ? Math.Clamp(parsed, 0, 100) : fallback;
}
