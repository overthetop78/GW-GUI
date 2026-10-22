using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Sinclair.Common.Functions.CoreHostFunctions;
using System.Globalization;


namespace GWGUI.Emulation.Sinclair.Common.Exceptions;

internal static class ExceptionText
{
    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization( typeof(ExceptionText).Assembly, "GWGUI.Emulation.Sinclair.Resources.Emulation");

    internal static string Get(string resourceKey) => Localization.TryGetString(resourceKey, CultureInfo.CurrentUICulture, out var value) ? value : resourceKey;
}
