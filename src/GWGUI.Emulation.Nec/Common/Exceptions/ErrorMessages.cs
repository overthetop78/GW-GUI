namespace GWGUI.Emulation.Nec.Common.Exceptions;

/// <summary>Messages de validation et d'exécution du module, résolus par ExceptionText.</summary>
internal static class ErrorMessages
{
    internal static string InvalidSaveRamSize(string path) => ExceptionText.Get("Emulation.Error.Core.InvalidSaveRamSize", path);
    internal static string SaveRamTooLarge() => ExceptionText.Get("Emulation.Error.Core.SaveRamTooLarge");

    internal static string HostConfigurationInvalid() => ExceptionText.Get("Emulation.Error.Core.HostConfigurationInvalid");
    internal static string HostNotInitialized() => ExceptionText.Get("Emulation.Error.Core.HostNotInitialized");
    internal static string UnknownHostCommand(byte command) => ExceptionText.Get("Emulation.Error.Core.UnknownHostCommand", command);

}
