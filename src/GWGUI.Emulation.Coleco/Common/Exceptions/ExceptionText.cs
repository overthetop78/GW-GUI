using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Coleco.Common.Functions.CoreHostFunctions;
using System.Globalization;


namespace GWGUI.Emulation.Coleco.Common.Exceptions;

internal static class ExceptionText
{
    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(ExceptionText).Assembly, "GWGUI.Emulation.Coleco.Resources.Emulation");

    internal static string Get(string resourceKey) => Localization.TryGetString(resourceKey, CultureInfo.CurrentUICulture, out var value) ? value : resourceKey;
}
