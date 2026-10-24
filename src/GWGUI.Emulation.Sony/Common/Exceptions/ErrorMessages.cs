namespace GWGUI.Emulation.Sony.Common.Exceptions;

/// <summary>Messages de validation et d'exécution du module, résolus par ExceptionText.</summary>
internal static class ErrorMessages
{

    internal static string HostConfigurationInvalid() => ExceptionText.Get(CoreExceptionsResourceConstants.HostConfigurationInvalid);
    internal static string HostNotInitialized() => ExceptionText.Get(CoreExceptionsResourceConstants.HostNotInitialized);
    internal static string ContentRefused() => ExceptionText.Get(CoreExceptionsResourceConstants.ContentRefused);
    internal static string InvalidStateSize(nuint size) => ExceptionText.Get(CoreExceptionsResourceConstants.InvalidStateSize, size);
    internal static string UnknownHostCommand(byte command) => ExceptionText.Get(CoreExceptionsResourceConstants.UnknownHostCommand, command);

    internal static string ForEmulator(string? emulatorId, string key, params object[] arguments)
    {
        // Identifiants des profils de ressources du module, indépendants des classes de backend.
        var profile = emulatorId switch
        {
            "pcsx2" => "Pcsx2",
            "ppsspp" => "Ppsspp",
            _ => null
        };
        return ExceptionText.Get(profile is null ? key : key.Replace("Emulation.Error.Core.", $"Emulation.Error.{profile}.", StringComparison.Ordinal), arguments);
    }

}
