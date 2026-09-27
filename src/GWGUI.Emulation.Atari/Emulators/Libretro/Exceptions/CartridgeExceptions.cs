using System.Globalization;
using GWGUI.Emulation.Atari.Modules;

namespace GWGUI.Emulation.Atari.Emulators.Libretro.Exceptions;

internal static class CartridgeExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(AtariEmulationModule).Assembly, "GWGUI.Emulation.Atari.Resources.Emulation");

    internal static EmulationException UnsupportedCore() =>
        Error(ErrorCategory.Content, ErrorCode.ContentUnsupported,
            "Emulation.Atari.Error.Cartridge.UnsupportedCore");

    internal static EmulationException CartridgeRequired() =>
        Error(ErrorCategory.Content, ErrorCode.ContentRequired,
            "Emulation.Atari.Error.Cartridge.Required");

    internal static EmulationException ExtensionUnsupported(IReadOnlyDictionary<string, string> context) =>
        Error(ErrorCategory.Content, ErrorCode.ContentUnsupported,
            "Emulation.Atari.Error.Cartridge.ExtensionUnsupported", context);

    internal static EmulationException FileUnreadable(
        IReadOnlyDictionary<string, string> context,
        Exception innerException) =>
        Error(ErrorCategory.Content, ErrorCode.ContentNotFound,
            "Emulation.Atari.Error.Cartridge.FileUnreadable", context, innerException);

    internal static EmulationException ReplacementFailed() =>
        Error(ErrorCategory.Content, ErrorCode.ContentUnsupported,
            "Emulation.Atari.Error.Cartridge.ReplacementFailed");

    internal static EmulationException RollbackFailed() =>
        Error(ErrorCategory.Content, ErrorCode.ContentUnsupported,
            "Emulation.Atari.Error.Cartridge.RollbackFailed");

    internal static EmulationException EjectionUnsupported() =>
        Error(ErrorCategory.Content, ErrorCode.ContentUnsupported,
            "Emulation.Atari.Error.Cartridge.EjectionUnsupported");

    internal static EmulationException RegionUnsupported() =>
        Error(ErrorCategory.Option, ErrorCode.OptionInvalid,
            "Emulation.Atari.Error.Cartridge.RegionUnsupported");

    internal static EmulationException SecamUnsupported() =>
        Error(ErrorCategory.Option, ErrorCode.OptionInvalid,
            "Emulation.Atari.Error.Cartridge.SecamUnsupported");

    private static EmulationException Error(
        ErrorCategory category,
        ErrorCode code,
        string resourceKey,
        IReadOnlyDictionary<string, string>? context = null,
        Exception? innerException = null) =>
        new(category, code, Text(resourceKey), context, innerException, isLocalized: true);

    private static string Text(string resourceKey)
    {
        return Localization.TryGetString(resourceKey, CultureInfo.CurrentUICulture, out var value)
            ? value
            : resourceKey;
    }
}
