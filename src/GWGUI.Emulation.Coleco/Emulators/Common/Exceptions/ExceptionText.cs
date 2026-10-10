using System.Globalization;


namespace GWGUI.Emulation.Coleco.Emulators.Common.Exceptions;

internal static class ExceptionText
{
    private static readonly EmulationModuleLocalization Localization = new(
        typeof(ExceptionText).Assembly, "GWGUI.Emulation.Coleco.Resources.Emulation");

    internal static string Get(string resourceKey) =>
        Localization.TryGetString(resourceKey, CultureInfo.CurrentUICulture, out var value)
            ? value
            : resourceKey;
}
