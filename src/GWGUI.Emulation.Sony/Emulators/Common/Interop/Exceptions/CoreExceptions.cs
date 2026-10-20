using GWGUI.Emulation.Sony.Emulators.Common.Interop.Constants;
using System.Globalization;
using GWGUI.Emulation.Sony.Modules;

namespace GWGUI.Emulation.Sony.Emulators.Common.Interop.Exceptions;

internal static class CoreExceptions
{
    private static readonly EmulationModuleLocalization Localization = new(typeof(SonyEmulationModule).Assembly, CoreExceptionsResourceConstants.ResourceName);

    internal static string HostConfigurationInvalid() => Text(CoreExceptionsResourceConstants.HostConfigurationInvalid);
    internal static string HostNotInitialized() => Text(CoreExceptionsResourceConstants.HostNotInitialized);
    internal static string ContentRefused() => Text(CoreExceptionsResourceConstants.ContentRefused);
    internal static string InvalidStateSize(nuint size) => Text(CoreExceptionsResourceConstants.InvalidStateSize, size);
    internal static string UnknownHostCommand(byte command) => Text(CoreExceptionsResourceConstants.UnknownHostCommand, command);

    internal static string ForEmulator(string? emulatorId, string key, params object[] arguments)
    {
        var profile = emulatorId switch
        {
            Emulators.Pcsx2.Constants.Pcsx2Constants.Id => "Pcsx2",
            Emulators.Ppsspp.Constants.PpssppConstants.Id => "Ppsspp",
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
