using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Fairchild.Common.Functions.CoreHostFunctions;
using System.Globalization;


namespace GWGUI.Emulation.Fairchild.Common.Exceptions;

internal static class ExceptionText
{
    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(ExceptionText).Assembly, "GWGUI.Emulation.Fairchild.Resources.Emulation");

    internal static string Get(string resourceKey) => Localization.TryGetString(resourceKey, CultureInfo.CurrentUICulture, out var value) ? value : resourceKey;
}
