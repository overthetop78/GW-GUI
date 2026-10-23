
namespace GWGUI.Emulation.Nintendo.Common.Constants;

internal static class CoreHostConstants
{
    /// <summary>Commande commune de consultation du statut des disques.</summary>
    internal const byte GetDiskStatusCommand = 13;

    /// <summary>Commande de réinitialisation logicielle commune aux hôtes.</summary>
    internal const byte SoftResetCommand = 16;
    /// <summary>Commande commune de changement du périphérique sur un port.</summary>
    internal const byte SetControllerPortDeviceCommand = 15;

    internal const string Windows = "windows";
    internal const string LocalPipeServer = ".";
    internal const int ConnectionTimeoutMilliseconds = 15_000;
}
