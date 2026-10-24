using GWGUI.Emulation.Interfaces;
using CommonCoreHostFunctions = GWGUI.Emulation.Sony.Common.Functions.CoreHostFunctions;
using System.Globalization;
using GWGUI.Emulation.Sony.Modules;

namespace GWGUI.Emulation.Sony.Common.Exceptions;

internal static class CoreExceptions
{
    private static readonly IEmulationModuleLocalization Localization = CommonCoreHostFunctions.CreateLocalization(typeof(SonyEmulationModule).Assembly, CoreExceptionsResourceConstants.ResourceName);

    internal static string HostConfigurationInvalid() => Text(CoreExceptionsResourceConstants.HostConfigurationInvalid);
    internal static string HostNotInitialized() => Text(CoreExceptionsResourceConstants.HostNotInitialized);
    internal static string ContentRefused() => Text(CoreExceptionsResourceConstants.ContentRefused);
    internal static string InvalidStateSize(nuint size) => Text(CoreExceptionsResourceConstants.InvalidStateSize, size);
    internal static string UnknownHostCommand(byte command) => Text(CoreExceptionsResourceConstants.UnknownHostCommand, command);

    internal static string ForEmulator(string? emulatorId, string key, params object[] arguments)
    {
        // Identifiants des profils de ressources du module, indépendants des classes de backend.
        var profile = emulatorId switch
        {
            "pcsx2" => "Pcsx2",
            "ppsspp" => "Ppsspp",
            _ => null
        };
        return Text(profile is null ? key : key.Replace("Emulation.Error.Core.", $"Emulation.Error.{profile}.", StringComparison.Ordinal), arguments);
    }

    private static string Text(string key, params object[] arguments)
    {
        if (!Localization.TryGetString(key, CultureInfo.CurrentUICulture, out var value)) value = key;
        return arguments.Length == 0 ? value : string.Format(CultureInfo.CurrentCulture, value, arguments);
    }
}
