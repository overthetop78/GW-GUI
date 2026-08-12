using System.Collections.Frozen;
using GWGUI.MediaEngine.Definitions;

namespace GWGUI.MediaEngine.FileSystems.Cpm;

/// <summary>Répertorie les dispositions CP/M génériques connues.</summary>
internal static class CpmLayoutCatalog
{
    /// <summary>Disposition Commodore 1541.</summary>
    public static readonly CpmLayout Commodore1541 = new(0x0a00, 0x0a00, 64, 1024, 2, false);
    /// <summary>Disposition Commodore 1571.</summary>
    public static readonly CpmLayout Commodore1571 = new(0x0a00, 0x0a00, 128, 2048, 2, true);
    /// <summary>Disposition Commodore 1581.</summary>
    public static readonly CpmLayout Commodore1581 = new(0, 0, 128, 2048, 2, true);
    /// <summary>Disposition Epson QX-10 320 Kio.</summary>
    public static readonly CpmLayout EpsonQx10_320 = new(4 * 2 * 16 * 256, 4 * 2 * 16 * 256, 64, 2048, 2, false);
    /// <summary>Disposition Epson QX-10 396 Kio.</summary>
    public static readonly CpmLayout EpsonQx10_396 = new(4 * 16 * 256, 4 * 16 * 256, 64, 2048, 2, false);
    /// <summary>Disposition Epson QX-10 399 Kio.</summary>
    public static readonly CpmLayout EpsonQx10_399 = new(16 * 256, 16 * 256, 64, 2048, 2, false);
    /// <summary>Disposition Epson QX-10 400 Kio.</summary>
    public static readonly CpmLayout EpsonQx10_400 = new(2 * 2 * 5 * 1024, 2 * 2 * 5 * 1024, 64, 2048, 2, false);
    /// <summary>Disposition Epson QX-10 LOGO.</summary>
    public static readonly CpmLayout EpsonQx10Logo = new(4 * 16 * 256, 4 * 16 * 256, 64, 2048, 2, false);

    private static readonly FrozenDictionary<string, CpmLayout> Layouts = new Dictionary<string, CpmLayout>(StringComparer.OrdinalIgnoreCase)
    {
        [DiskImageFormatIds.Commodore1541] = Commodore1541,
        [DiskImageFormatIds.Commodore1571] = Commodore1571,
        [DiskImageFormatIds.Commodore1581] = Commodore1581,
        [DiskImageFormatIds.EpsonQx10_320] = EpsonQx10_320,
        [DiskImageFormatIds.EpsonQx10_396] = EpsonQx10_396,
        [DiskImageFormatIds.EpsonQx10_399] = EpsonQx10_399,
        [DiskImageFormatIds.EpsonQx10_400] = EpsonQx10_400,
        [DiskImageFormatIds.EpsonQx10Logo] = EpsonQx10Logo
    }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    /// <summary>Identifiants de formats possédant une disposition cataloguée.</summary>
    public static IReadOnlySet<string> FormatIds { get; } = Layouts.Keys.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    /// <summary>Tailles sectorielles acceptées par les dispositions CP/M cataloguées.</summary>
    public static IReadOnlySet<int> SupportedSectorSizes { get; } = new[] { 256, 512, 1024 }.ToFrozenSet();

    /// <summary>Retourne la disposition associée à un identifiant.</summary>
    public static CpmLayout? Resolve(string formatId) => Layouts.GetValueOrDefault(formatId);

    /// <summary>Indique si la taille des blocs sectoriels reste compatible avec le format catalogué.</summary>
    public static bool SupportsBlockSize(string formatId, int blockSize) => Layouts.ContainsKey(formatId) && SupportedSectorSizes.Contains(blockSize);
}
