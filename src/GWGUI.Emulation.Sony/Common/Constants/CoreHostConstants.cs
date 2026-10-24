
namespace GWGUI.Emulation.Sony.Common.Constants;

internal static class CoreHostConstants
{
    /// <summary>Version commune des en-têtes de requête et de réponse.</summary>
    internal const int ProtocolVersion = 5;
    /// <summary>Statut commun précédant les données d’une opération réussie.</summary>
    internal const byte SuccessResponse = 1;
    /// <summary>Statut commun précédant le diagnostic d’une opération échouée.</summary>
    internal const byte FailureResponse = 2;

    /// <summary>Commande commune de consultation du statut des disques.</summary>
    internal const byte GetDiskStatusCommand = 13;

    /// <summary>Commande de réinitialisation logicielle commune aux hôtes.</summary>
    internal const byte SoftResetCommand = 16;
    /// <summary>Commande commune de changement du périphérique sur un port.</summary>
    internal const byte SetControllerPortDeviceCommand = 15;

    internal const string Windows = "windows";
    internal const string LocalPipeServer = ".";
    /// <summary>Publication explicite des changements d’un média par son propriétaire.</summary>
    internal const byte SaveMediaChangesCommand = 12;
    /// <summary>Consultation des changements possibles conservés par le backend.</summary>
    internal const byte HasUnsavedMediaChangesCommand = 14;
}
