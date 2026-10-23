
namespace GWGUI.Emulation.Nec.Common.Constants;

internal static class CoreHostConstants
{
    /// <summary>Commande de réinitialisation logicielle commune aux hôtes.</summary>
    internal const byte SoftResetCommand = 16;
    /// <summary>Commande commune de changement du périphérique sur un port.</summary>
    internal const byte SetControllerPortDeviceCommand = 15;

    internal const string DisplayName = "NEC";
    internal const string Windows = "windows";
    internal const string Value = ".";
}
