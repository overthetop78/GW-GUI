
namespace GWGUI.Emulation.Commodore.Common.Constants;

internal static class CoreHostConstants
{
    /// <summary>Commande commune de consultation du statut des disques.</summary>
    internal const byte GetDiskStatusCommand = 13;

    /// <summary>Commande de réinitialisation logicielle commune aux hôtes.</summary>
    internal const byte SoftResetCommand = 16;
    /// <summary>Commande commune de changement du périphérique sur un port.</summary>
    internal const byte SetControllerPortDeviceCommand = 15;

    internal const string Windows = "windows";
    internal const string LocalServerName = ".";
    internal const int PipeConnectionTimeoutMilliseconds = 15_000;
    internal const long InitialVideoSequence = 0;
    internal const int InitialDiagnosticCount = 0;
    internal const int SuccessfulExitCode = 0;
}
