namespace GWGUI.MediaEngine.Constants;

internal static class DiskVisualizationDefaults
{
    internal const string Amstrad = "Amstrad";
    internal const string Dec = "DEC";
    internal const string IsoMfmDecoder = "iso.mfm";
    internal const string HighDensityMarker = "hd";
    internal const string HighDensitySuffix = "_hd";
    internal const string HighDensity1440 = "1440";
    internal const string ExtendedDensity2880 = "2880";

    internal static readonly IReadOnlyDictionary<string, string> DecoderByMachine =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Apple II"] = "apple2.gcr",
            ["Apple Macintosh"] = "applemac.gcr",
            ["Apple Lisa"] = "applelisa.fileware.gcr",
            ["Amiga"] = "amiga.mfm",
            ["Commodore"] = "commodore.gcr",
            [Dec] = "dec.rx02"
        };

    internal static readonly IReadOnlySet<string> ThreeHalfMachines =
        new HashSet<string>(["Atari ST", "Amiga", "IBM PC", "Apple Macintosh", "MSX"], StringComparer.OrdinalIgnoreCase);

    internal static readonly IReadOnlySet<string> FiveQuarterMachines =
        new HashSet<string>(["Apple II", "Commodore", "Acorn", "Acorn / BBC Micro"], StringComparer.OrdinalIgnoreCase);
}
