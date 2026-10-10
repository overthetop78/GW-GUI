using System.Globalization;


namespace GWGUI.Emulation.Apple.Emulators.Common.Exceptions;

internal static class ExceptionText
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(ExceptionText).Assembly, "GWGUI.Emulation.Apple.Resources.Emulation");

    internal static string Get(string resourceKey) =>
        Localization.TryGetString(resourceKey, CultureInfo.CurrentUICulture, out var value)
            ? value
            : resourceKey;
}
