namespace GWGUI.Emulation.Example.Common.Constants;

/// <summary>Clés des ressources utilisées pour résoudre les diagnostics Common du module.</summary>
/// <remarks>Ces valeurs identifient les textes, elles ne contiennent aucune traduction. ExceptionText
/// consulte le catalogue dans la culture demandée ; ErrorMessages et CoreHostErrors choisissent la situation.
/// Une clé partagée n'est définie qu'une fois ici. Les fichiers de ressources conservent les valeurs
/// traduites et les paramètres de formatage, qui sont appliqués après la résolution du texte.</remarks>
internal static class CoreErrorResourceKeys
{
    /// <summary>Média configuré introuvable ; le contexte du diagnostic conserve le chemin concerné.</summary>
    internal const string ContentNotFound = "Emulation.Example.Error.ContentNotFound";
    /// <summary>Contenu refusé ou incompatible avec le lecteur ou la capacité demandée.</summary>
    internal const string ContentUnsupported = "Emulation.Example.Error.ContentUnsupported";
    /// <summary>Bibliothèque ou version de cœur introuvable dans l'installation demandée.</summary>
    internal const string CoreNotFound = "Emulation.Example.Error.CoreNotFound";
    /// <summary>Cœur refusé lors de sa validation, de son initialisation ou de l'examen de son API.</summary>
    internal const string CoreRejected = "Emulation.Example.Error.CoreRejected";
    /// <summary>Modèle de détail ajouté à un diagnostic pour restituer sa valeur de contexte.</summary>
    internal const string Details = "Emulation.Example.Error.Details";
    /// <summary>Firmware illisible, incompatible ou dont l'identité ne peut pas être déterminée.</summary>
    internal const string FirmwareInvalid = "Emulation.Example.Error.FirmwareInvalid";
    /// <summary>Firmware obligatoire absent ou non sélectionné dans la configuration.</summary>
    internal const string FirmwareMissing = "Emulation.Example.Error.FirmwareMissing";
    /// <summary>Exécutable d'hôte absent au moment de créer le processus qui doit posséder le cœur.</summary>
    internal const string HostExecutableMissing = "Emulation.Example.Error.HostExecutableMissing";
    /// <summary>Diagnostic général partagé par les incidents du transport et du protocole d'hôte.</summary>
    internal const string HostProtocolFailure = "Emulation.Example.Error.HostProtocolFailure";
    /// <summary>Réglage, déclaration de modèle ou association de périphérique invalide.</summary>
    internal const string OptionInvalid = "Emulation.Example.Error.OptionInvalid";
    /// <summary>État valide dont l'identité ne correspond pas à la machine courante.</summary>
    internal const string StateIncompatible = "Emulation.Example.Error.StateIncompatible";
    /// <summary>État absent, corrompu ou refusé lors de sa lecture, de son écriture ou de sa restauration.</summary>
    internal const string StateInvalid = "Emulation.Example.Error.StateInvalid";
    /// <summary>Incident de session ou commande incompatible avec l'état courant de la machine.</summary>
    internal const string Unexpected = "Emulation.Example.Error.Unexpected";
}
