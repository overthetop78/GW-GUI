
namespace GWGUI.Emulation.Nec.Common.Constants;

internal static class CoreHostConstants
{
    /// <summary>Commande commune de consultation du statut des disques.</summary>
    internal const byte GetDiskStatusCommand = 13;

    /// <summary>Commande de réinitialisation logicielle commune aux hôtes.</summary>
    internal const byte SoftResetCommand = 16;
    /// <summary>Commande commune de changement du périphérique sur un port.</summary>
    internal const byte SetControllerPortDeviceCommand = 15;

    internal const string DisplayName = "NEC";
    internal const string Windows = "windows";
    internal const string Value = ".";
    /// <summary>Publication explicite des changements d’un média par son propriétaire.</summary>
    internal const byte SaveMediaChangesCommand = 12;
    /// <summary>Consultation des changements possibles conservés par le backend.</summary>
    internal const byte HasUnsavedMediaChangesCommand = 14;
}
