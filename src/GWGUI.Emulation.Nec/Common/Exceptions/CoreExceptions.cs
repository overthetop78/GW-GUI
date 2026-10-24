using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Nec.Common.Functions.CoreHostFunctions;
using System.Globalization;
using GWGUI.Emulation.Nec.Modules;

namespace GWGUI.Emulation.Nec.Common.Exceptions;

internal static class CoreExceptions
{
    internal static string InvalidSaveRamSize(string path) => Text("Emulation.Error.Core.InvalidSaveRamSize", path);
    internal static string SaveRamTooLarge() => Text("Emulation.Error.Core.SaveRamTooLarge");
    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(NecEmulationModule).Assembly, "GWGUI.Emulation.Nec.Resources.Emulation");

    internal static string HostConfigurationInvalid() => Text("Emulation.Error.Core.HostConfigurationInvalid");
    internal static string HostNotInitialized() => Text("Emulation.Error.Core.HostNotInitialized");
    internal static string UnknownHostCommand(byte command) => Text("Emulation.Error.Core.UnknownHostCommand", command);

    internal static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}
