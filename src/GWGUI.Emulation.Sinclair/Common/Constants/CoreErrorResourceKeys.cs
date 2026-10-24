namespace GWGUI.Emulation.Sinclair.Common.Constants;

/// <summary>Clés des ressources utilisées pour résoudre les diagnostics Common du module.</summary>
/// <remarks>Ces valeurs identifient les textes, elles ne contiennent aucune traduction. ExceptionText
/// consulte le catalogue dans la culture demandée ; ErrorMessages et CoreHostErrors choisissent la situation.
/// Une clé partagée n'est définie qu'une fois ici. Les fichiers de ressources conservent les valeurs
/// traduites et les paramètres de formatage, qui sont appliqués après la résolution du texte.</remarks>
internal static class CoreErrorResourceKeys
{
    /// <summary>Clé de la ressource CoreNotFound ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreNotFound = "Emulation.Sinclair.Error.CoreNotFound";
    /// <summary>Clé de la ressource CoreRejected ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string CoreRejected = "Emulation.Sinclair.Error.CoreRejected";
    /// <summary>Clé de la ressource Details ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string Details = "Emulation.Sinclair.Error.Details";
    /// <summary>Clé de la ressource HostExecutableMissing ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostExecutableMissing = "Emulation.Sinclair.Error.HostExecutableMissing";
    /// <summary>Clé de la ressource HostProtocolFailure ; sa valeur est résolue par le catalogue de localisation.</summary>
    internal const string HostProtocolFailure = "Emulation.Sinclair.Error.HostProtocolFailure";
    /// <summary>Clé du diagnostic StateIncompatible dans le catalogue du module.</summary>
    internal const string StateIncompatible = "Emulation.Sinclair.Error.StateIncompatible";
    /// <summary>Clé du diagnostic Unexpected dans le catalogue du module.</summary>
    internal const string Unexpected = "Emulation.Sinclair.Error.Unexpected";
    /// <summary>Clé du diagnostic FirmwareMissing dans le catalogue du module.</summary>
    internal const string FirmwareMissing = "Emulation.Sinclair.Error.FirmwareMissing";
    /// <summary>Clé du diagnostic FirmwareInvalid dans le catalogue du module.</summary>
    internal const string FirmwareInvalid = "Emulation.Sinclair.Error.FirmwareInvalid";
    /// <summary>Clé du diagnostic ContentNotFound dans le catalogue du module.</summary>
    internal const string ContentNotFound = "Emulation.Sinclair.Error.ContentNotFound";
    /// <summary>Clé du diagnostic ContentUnsupported dans le catalogue du module.</summary>
    internal const string ContentUnsupported = "Emulation.Sinclair.Error.ContentUnsupported";
    /// <summary>Clé du diagnostic OptionInvalid dans le catalogue du module.</summary>
    internal const string OptionInvalid = "Emulation.Sinclair.Error.OptionInvalid";
    /// <summary>Clé du diagnostic StateInvalid dans le catalogue du module.</summary>
    internal const string StateInvalid = "Emulation.Sinclair.Error.StateInvalid";
}
