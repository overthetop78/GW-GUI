using System.Globalization;
using GWGUI.Emulation.Atari.Modules;

namespace GWGUI.Emulation.Atari.Emulators.Libretro.Exceptions;

internal static class ExceptionText
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(AtariEmulationModule).Assembly, "GWGUI.Emulation.Atari.Resources.Emulation");

    internal static string Get(string resourceKey) =>
        Localization.TryGetString(resourceKey, CultureInfo.CurrentUICulture, out var value)
            ? value
            : resourceKey;
}
